using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Singulink.Reflection;

/// <summary>
/// Provides methods to create objects and delegates that create objects based on specified constructor signatures.
/// </summary>
public static class ObjectFactory
{
    private const DynamicallyAccessedMemberTypes AllConstructors = DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors;
    private const DynamicallyAccessedMemberTypes PublicDefaultConstructor = DynamicallyAccessedMemberTypes.PublicParameterlessConstructor;
    private const DynamicallyAccessedMemberTypes DefaultConstructors = DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.NonPublicConstructors;

    private static readonly ConcurrentDictionary<(Type ObjectType, Type DelegateType), (object Activator, bool IsPublic)> _delegateActivatorCache = new();

    #region Create Instance

    /// <summary>
    /// Creates an object of the specified type using the public default constructor.
    /// </summary>
    public static object CreateInstance([DynamicallyAccessedMembers(PublicDefaultConstructor)] Type type)
    {
#pragma warning disable IL2067 // Justification: Only public default ctor needed.
        return CreateInstance(type, false);
#pragma warning restore IL2067
    }

    /// <summary>
    /// Creates an object of the specified type using the default constructor, optionally calling non-public constructors as well.
    /// </summary>
    public static object CreateInstance([DynamicallyAccessedMembers(DefaultConstructors)] Type type, bool nonPublic)
    {
        return GetActivator(type, nonPublic).Invoke();
    }

    /// <summary>
    /// Creates an object of the specified type using the public default constructor.
    /// </summary>
    public static TObject CreateInstance<[DynamicallyAccessedMembers(PublicDefaultConstructor)] TObject>()
    {
#pragma warning disable IL2091 // Justification: Only public default ctor needed.
        return CreateInstance<TObject>(false);
#pragma warning restore IL2091
    }

    /// <summary>
    /// Creates an object of the specified type using the default constructor, optionally calling non-public constructors as well.
    /// </summary>
    public static TObject CreateInstance<[DynamicallyAccessedMembers(DefaultConstructors)] TObject>(bool nonPublic)
    {
        if (typeof(TObject).IsValueType)
            return Activator.CreateInstance<TObject>();

        return GetActivator<TObject>(nonPublic).Invoke();
    }

    /// <summary>
    /// Creates an object of the specified type using the default constructor, optionally calling non-public constructors as well.
    /// </summary>
    public static TResult CreateInstance<TResult>([DynamicallyAccessedMembers(PublicDefaultConstructor)] Type objectType)
    {
#pragma warning disable IL2067 // Justification: Only public default ctor needed.
        return CreateInstance<TResult>(objectType, false);
#pragma warning restore IL2067
    }

    /// <summary>
    /// Creates an object of the specified type using the default constructor, optionally calling non-public constructors as well.
    /// </summary>
    public static TResult CreateInstance<TResult>([DynamicallyAccessedMembers(DefaultConstructors)] Type objectType, bool nonPublic)
    {
        return GetActivator<TResult>(objectType, nonPublic).Invoke();
    }

    #endregion

    #region Create Uninitialized Instance

    /// <summary>
    /// Creates an uninitialized object of the specified type.
    /// </summary>
    public static TObject CreateUninitializedInstance<[DynamicallyAccessedMembers(AllConstructors)] TObject>()
    {
        if (typeof(TObject).IsValueType)
            return default!;

        return (TObject)RuntimeHelpers.GetUninitializedObject(typeof(TObject));
    }

    /// <summary>
    /// Creates an uninitialized object of the specified type.
    /// </summary>
    public static object CreateUninitializedInstance([DynamicallyAccessedMembers(AllConstructors)] Type objectType)
    {
        return RuntimeHelpers.GetUninitializedObject(objectType);
    }

    /// <summary>
    /// Creates an uninitialized object of the specified type.
    /// </summary>
    public static TResult CreateUninitializedInstance<TResult>([DynamicallyAccessedMembers(AllConstructors)] Type objectType)
    {
        if (typeof(TResult).IsValueType && typeof(TResult) == objectType)
            return default!;

        return (TResult)RuntimeHelpers.GetUninitializedObject(objectType);
    }

    #endregion

    #region Get Activator Delegate

    /// <summary>
    /// Gets a delegate that calls a matching signature constructor on the given object type.
    /// </summary>
    /// <typeparam name="TDelegate">The type of delegate which parameters will be matched to the constructor signature.</typeparam>
    /// <param name="objectType">The type of object to construct.</param>
    /// <param name="nonPublic"><see langword="true"/> to get a matching non-public constructor, otherwise false.</param>
    /// <returns>A delegate that calls the object constructor.</returns>
    public static TDelegate GetActivatorDelegate<TDelegate>([DynamicallyAccessedMembers(AllConstructors)] Type objectType, bool nonPublic = false)
        where TDelegate : Delegate
    {
        if (!_delegateActivatorCache.TryGetValue((objectType, typeof(TDelegate)), out var info))
            info = _delegateActivatorCache.GetOrAdd((objectType, typeof(TDelegate)), _ => CreateActivator<TDelegate>(objectType, nonPublic));

        if (!nonPublic && !info.IsPublic)
            ThrowNonPublicConstructorException(objectType);

        return Unsafe.As<object, TDelegate>(ref info.Activator);
    }

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the public default constructor.
    /// </summary>
    public static Func<object> GetActivator([DynamicallyAccessedMembers(PublicDefaultConstructor)] Type objectType)
    {
#pragma warning disable IL2067 // Justification: Only public default ctor needed.
        return GetActivator(objectType, false);
#pragma warning restore IL2067
    }

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the default constructor, optionally calling non-public constructors
    /// as well.
    /// </summary>
    public static Func<object> GetActivator([DynamicallyAccessedMembers(DefaultConstructors)] Type objectType, bool nonPublic)
    {
        return GetActivator<object>(objectType, nonPublic);
    }

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the public default constructor.
    /// </summary>
    public static Func<TObject> GetActivator<[DynamicallyAccessedMembers(PublicDefaultConstructor)] TObject>()
    {
#pragma warning disable IL2091 // Justification: Only public default ctor needed.
        return GetActivator<TObject>(false);
#pragma warning restore IL2091
    }

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the default constructor, optionally calling non-public constructors
    /// as well.
    /// </summary>
    public static Func<TObject> GetActivator<[DynamicallyAccessedMembers(DefaultConstructors)] TObject>(bool nonPublic)
    {
        var info = DefaultActivatorCache<TObject>.Info;

        if (info.Activator == null)
            throw new MissingMethodException($"Type '{typeof(TObject)}' does not have a default constructor.");

        if (!nonPublic && !info.IsPublic)
            ThrowNonPublicConstructorException(typeof(TObject));

        return info.Activator;
    }

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter type, optionally
    /// calling non-public constructors as well.
    /// </summary>
    public static Func<TParam, TObject> GetActivator<TParam, [DynamicallyAccessedMembers(AllConstructors)] TObject>(bool nonPublic = false)
        => GetActivatorDelegate<Func<TParam, TObject>>(typeof(TObject), nonPublic);

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter types, optionally
    /// calling non-public constructors as well.
    /// </summary>
    public static Func<TParam1, TParam2, TObject> GetActivator<TParam1, TParam2, [DynamicallyAccessedMembers(AllConstructors)] TObject>(bool nonPublic = false)
        => GetActivatorDelegate<Func<TParam1, TParam2, TObject>>(typeof(TObject), nonPublic);

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter types, optionally
    /// calling non-public constructors as well.
    /// </summary>
    public static Func<TParam1, TParam2, TParam3, TObject> GetActivator<TParam1, TParam2, TParam3, [DynamicallyAccessedMembers(AllConstructors)] TObject>(bool nonPublic = false)
        => GetActivatorDelegate<Func<TParam1, TParam2, TParam3, TObject>>(typeof(TObject), nonPublic);

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter types, optionally
    /// calling non-public constructors as well.
    /// </summary>
    public static Func<TParam1, TParam2, TParam3, TParam4, TObject> GetActivator<TParam1, TParam2, TParam3, TParam4, [DynamicallyAccessedMembers(AllConstructors)] TObject>(bool nonPublic = false)
        => GetActivatorDelegate<Func<TParam1, TParam2, TParam3, TParam4, TObject>>(typeof(TObject), nonPublic);

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter types, optionally
    /// calling non-public constructors as well.
    /// </summary>
    public static Func<TParam1, TParam2, TParam3, TParam4, TParam5, TObject> GetActivator<TParam1, TParam2, TParam3, TParam4, TParam5, [DynamicallyAccessedMembers(AllConstructors)] TObject>(bool nonPublic = false)
        => GetActivatorDelegate<Func<TParam1, TParam2, TParam3, TParam4, TParam5, TObject>>(typeof(TObject), nonPublic);

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the public default constructor.
    /// </summary>
    public static Func<TResult> GetActivator<TResult>([DynamicallyAccessedMembers(PublicDefaultConstructor)] Type objectType)
    {
#pragma warning disable IL2067 // Justification: Only public default ctor needed.
        return GetActivator<TResult>(objectType, false);
#pragma warning restore IL2067
    }

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the default constructor, optionally calling non-public constructors
    /// as well.
    /// </summary>
    public static Func<TResult> GetActivator<TResult>([DynamicallyAccessedMembers(DefaultConstructors)] Type objectType, bool nonPublic)
    {
#pragma warning disable IL2091 // Justification: TResult ctors not used.
        var info = DefaultActivatorCache<TResult>.GetTypedInfo(objectType, nonPublic);
#pragma warning restore IL2091

        if (!nonPublic && !info.IsPublic)
            ThrowNonPublicConstructorException(objectType);

        return info.Activator;
    }

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter types, optionally
    /// calling non-public constructors as well.
    /// </summary>
    public static Func<TParam, TResult> GetActivator<TParam, TResult>([DynamicallyAccessedMembers(AllConstructors)] Type objectType, bool nonPublic = false)
        => GetActivatorDelegate<Func<TParam, TResult>>(objectType, nonPublic);

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter types, optionally
    /// calling non-public constructors as well.
    /// </summary>
    public static Func<TParam1, TParam2, TResult> GetActivator<TParam1, TParam2, TResult>([DynamicallyAccessedMembers(AllConstructors)] Type objectType, bool nonPublic = false)
        => GetActivatorDelegate<Func<TParam1, TParam2, TResult>>(objectType, nonPublic);

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter types, optionally
    /// calling non-public constructors as well.
    /// </summary>
    public static Func<TParam1, TParam2, TParam3, TResult> GetActivator<TParam1, TParam2, TParam3, TResult>([DynamicallyAccessedMembers(AllConstructors)] Type objectType, bool nonPublic = false)
        => GetActivatorDelegate<Func<TParam1, TParam2, TParam3, TResult>>(objectType, nonPublic);

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter types, optionally
    /// calling non-public constructors as well.
    /// </summary>
    public static Func<TParam1, TParam2, TParam3, TParam4, TResult> GetActivator<TParam1, TParam2, TParam3, TParam4, TResult>([DynamicallyAccessedMembers(AllConstructors)] Type objectType, bool nonPublic = false)
        => GetActivatorDelegate<Func<TParam1, TParam2, TParam3, TParam4, TResult>>(objectType, nonPublic);

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter types, optionally
    /// calling non-public constructors as well.
    /// </summary>
    public static Func<TParam1, TParam2, TParam3, TParam4, TParam5, TResult> GetActivator<TParam1, TParam2, TParam3, TParam4, TParam5, TResult>([DynamicallyAccessedMembers(AllConstructors)] Type objectType, bool nonPublic = false)
        => GetActivatorDelegate<Func<TParam1, TParam2, TParam3, TParam4, TParam5, TResult>>(objectType, nonPublic);

    #endregion

    private static (TDelegate Activator, bool IsPublic) CreateActivator<TDelegate>([DynamicallyAccessedMembers(AllConstructors)] Type objectType, bool nonPublic) where TDelegate : Delegate
    {
#pragma warning disable IL2090 // Justification: Delegates always generate metadata for the Invoke method
        var delegateInfo = typeof(TDelegate).GetMethod("Invoke", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
#pragma warning restore IL2090

        if (delegateInfo == null)
            throw new MissingMethodException("Missing metadata for delegate Invoke method.");

        var returnType = delegateInfo.ReturnType;

        if (returnType.IsByRef)
            throw new NotSupportedException("The delegate cannot have a ref return type.");

        if (!returnType.IsAssignableFrom(objectType))
            throw new InvalidCastException("The object type must be assignable to the delegate return type.");

        var parameters = delegateInfo.GetParameters();
        var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();

        if (parameters.Any(p => p.IsOut) || parameterTypes.Any(p => p.IsByRef))
            throw new NotSupportedException("The delegate cannot have ref or out parameters.");

        var parameterExpressions = parameterTypes.Select((p, i) => Expression.Parameter(p, "p" + i)).ToArray();
        NewExpression newExpression;

        bool isPublic;

        if (objectType.IsValueType && parameterTypes.Length == 0)
        {
            isPublic = true;
            newExpression = Expression.New(objectType);
        }
        else
        {
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            ConstructorInfo constructor = objectType.GetConstructor(bindingFlags, null, parameterTypes, null) ?? throw new MissingMethodException("A matching constructor was not found.");

            isPublic = constructor.IsPublic;

            if (!nonPublic && !isPublic)
                ThrowNonPublicConstructorException(objectType);

            newExpression = Expression.New(constructor, parameterExpressions);
        }

        return (Expression.Lambda<TDelegate>(newExpression, parameterExpressions).Compile(), isPublic);
    }

    private static void ThrowNonPublicConstructorException(Type objectType) => throw new MissingMethodException($"Requested constructor for type '{objectType}' is not public.");

    private static class DefaultActivatorCache<[DynamicallyAccessedMembers(DefaultConstructors)] T>
    {
        private static readonly ConcurrentDictionary<Type, (Func<T> Activator, bool IsPublic)> _typedInfoCache = new();

        private static readonly (Func<T> Activator, bool IsPublic) _info = InitInfo();

        public static (Func<T> Activator, bool IsPublic) Info => _info.Activator != null ? _info : throw new MissingMethodException("A default constructor was not found.");

        private static (Func<T> Activator, bool IsPublic) InitInfo()
        {
            try
            {
#pragma warning disable IL2087 // Justification: Only default ctors needed.
                return CreateActivator<Func<T>>(typeof(T), true);
#pragma warning restore IL2087
            }
            catch
            {
                return default;
            }
        }

        public static (Func<T> Activator, bool IsPublic) GetTypedInfo([DynamicallyAccessedMembers(DefaultConstructors)] Type objectType, bool nonPublic)
        {
            if (!_typedInfoCache.TryGetValue(objectType, out var info))
            {
#pragma warning disable IL2067 // Justification: Only default ctors needed.
                info = _typedInfoCache.GetOrAdd(objectType, _ => CreateActivator<Func<T>>(objectType, nonPublic));
#pragma warning restore IL2067
            }

            return info;
        }
    }
}