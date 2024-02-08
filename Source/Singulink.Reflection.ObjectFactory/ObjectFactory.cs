using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Singulink.Reflection;

/// <summary>
/// Provides methods to create objects and delegates that create instances based on specified constructor signatures.
/// </summary>
public static class ObjectFactory
{
    private const DynamicallyAccessedMemberTypes AllConstructors = DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors;
    private const DynamicallyAccessedMemberTypes PublicDefaultConstructor = DynamicallyAccessedMemberTypes.PublicParameterlessConstructor;
    private const DynamicallyAccessedMemberTypes PublicConstructors = DynamicallyAccessedMemberTypes.PublicConstructors;
    private const DynamicallyAccessedMemberTypes DefaultConstructors = DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.NonPublicConstructors;

    private static readonly ConcurrentDictionary<(Type ObjectType, Type DelegateType), (Delegate Activator, bool IsPublic)> _delegateActivatorCache = new();

    private static class DefaultActivatorCache<[DynamicallyAccessedMembers(DefaultConstructors)] T>
    {
        public static readonly bool IsPublic = typeof(T).IsValueType || typeof(T).GetConstructor(Type.EmptyTypes) is not null;
        public static readonly DefaultActivator<T> Instance = GetActivator<T>(true);
    }

    /// <summary>
    /// Creates an object of the specified type using the default constructor, optionally calling a non-public constructor as well.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T CreateInstance<[DynamicallyAccessedMembers(DefaultConstructors)] T>(bool nonPublic = false)
    {
#if !NETSTANDARD
        if (!RuntimeFeature.IsDynamicCodeCompiled && !nonPublic)
            return Activator.CreateInstance<T>();

        if (typeof(T).IsValueType)
            return Activator.CreateInstance<T>();
#endif

        if (!DefaultActivatorCache<T>.IsPublic && !nonPublic)
            ThrowNonPublicConstructorException(typeof(T));

        return DefaultActivatorCache<T>.Instance.Invoke();
    }

    /// <summary>
    /// Creates an uninitialized object of the specified type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T CreateUninitializedInstance<[DynamicallyAccessedMembers(AllConstructors)] T>()
    {
#if NETSTANDARD
        return (T)FormatterServices.GetUninitializedObject(typeof(T));
#else
        return (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
#endif
    }

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the public default constructor.
    /// </summary>
    public static DefaultActivator<T> GetActivator<[DynamicallyAccessedMembers(PublicDefaultConstructor)] T>()
    {
#pragma warning disable IL2091 // Justification: Only public default ctor needed.
        return GetActivator<T>(false);
#pragma warning restore IL2091
    }

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the default constructor, optionally calling a non-public
    /// constructor as well.
    /// </summary>
    public static DefaultActivator<T> GetActivator<[DynamicallyAccessedMembers(DefaultConstructors)] T>(bool nonPublic)
    {
#if !NETSTANDARD
        if (typeof(T).IsValueType)
            return default;
#endif

#pragma warning disable IL2087 // Justification: Only default ctors needed.
        return new(GetActivatorByDelegate<Func<T>>(typeof(T), nonPublic));
#pragma warning restore IL2087
    }

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the public constructor that accepts the specified parameter type.
    /// </summary>
    public static Func<TParam, T> GetActivator<TParam, [DynamicallyAccessedMembers(PublicConstructors)] T>()
    {
#pragma warning disable IL2091 // Justification: Only public ctors needed.
        return GetActivator<TParam, T>(false);
#pragma warning restore IL2091
    }

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter type, optionally
    /// calling non-public constructors as well.
    /// </summary>
    public static Func<TParam, T> GetActivator<TParam, [DynamicallyAccessedMembers(AllConstructors)] T>(bool nonPublic)
    {
        return GetActivatorByDelegate<Func<TParam, T>>(typeof(T), nonPublic);
    }

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the public constructor that accepts the specified parameter types.
    /// </summary>
    public static Func<TParam1, TParam2, T> GetActivator<TParam1, TParam2, [DynamicallyAccessedMembers(PublicConstructors)] T>()
    {
#pragma warning disable IL2091 // Justification: Only public ctors needed.
        return GetActivator<TParam1, TParam2, T>(false);
#pragma warning restore IL2091
    }

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter types, optionally
    /// calling non-public constructors as well.
    /// </summary>
    public static Func<TParam1, TParam2, T> GetActivator<TParam1, TParam2, [DynamicallyAccessedMembers(AllConstructors)] T>(bool nonPublic)
    {
        return GetActivatorByDelegate<Func<TParam1, TParam2, T>>(typeof(T), nonPublic);
    }

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the public constructor that accepts the specified parameter types.
    /// </summary>
    public static Func<TParam1, TParam2, TParam3, T> GetActivator<TParam1, TParam2, TParam3, [DynamicallyAccessedMembers(PublicConstructors)] T>()
    {
#pragma warning disable IL2091 // Justification: Only public ctors needed.
        return GetActivator<TParam1, TParam2, TParam3, T>(false);
#pragma warning restore IL2091
    }

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter types, optionally
    /// calling non-public constructors as well.
    /// </summary>
    public static Func<TParam1, TParam2, TParam3, T> GetActivator<TParam1, TParam2, TParam3, [DynamicallyAccessedMembers(AllConstructors)] T>(bool nonPublic)
    {
        return GetActivatorByDelegate<Func<TParam1, TParam2, TParam3, T>>(typeof(T), nonPublic);
    }

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the public constructor that accepts the specified parameter types.
    /// </summary>
    public static Func<TParam1, TParam2, TParam3, TParam4, T> GetActivator<TParam1, TParam2, TParam3, TParam4, [DynamicallyAccessedMembers(PublicConstructors)] T>()
    {
#pragma warning disable IL2091 // Justification: Only public ctors needed.
        return GetActivator<TParam1, TParam2, TParam3, TParam4, T>(false);
#pragma warning restore IL2091
    }

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter types, optionally
    /// calling non-public constructors as well.
    /// </summary>
    public static Func<TParam1, TParam2, TParam3, TParam4, T> GetActivator<TParam1, TParam2, TParam3, TParam4, [DynamicallyAccessedMembers(AllConstructors)] T>(bool nonPublic)
    {
        return GetActivatorByDelegate<Func<TParam1, TParam2, TParam3, TParam4, T>>(typeof(T), nonPublic);
    }

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the public constructor that accepts the specified parameter types.
    /// </summary>
    public static Func<TParam1, TParam2, TParam3, TParam4, TParam5, T> GetActivator<TParam1, TParam2, TParam3, TParam4, TParam5, [DynamicallyAccessedMembers(PublicConstructors)] T>()
    {
#pragma warning disable IL2091 // Justification: Only public ctors needed.
        return GetActivator<TParam1, TParam2, TParam3, TParam4, TParam5, T>(false);
#pragma warning restore IL2091
    }

    /// <summary>
    /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter types, optionally
    /// calling non-public constructors as well.
    /// </summary>
    public static Func<TParam1, TParam2, TParam3, TParam4, TParam5, T> GetActivator<TParam1, TParam2, TParam3, TParam4, TParam5, [DynamicallyAccessedMembers(AllConstructors)] T>(bool nonPublic)
    {
        return GetActivatorByDelegate<Func<TParam1, TParam2, TParam3, TParam4, TParam5, T>>(typeof(T), nonPublic);
    }

    /// <summary>
    /// Gets a delegate that calls a matching signature public constructor on the given object type.
    /// </summary>
    /// <typeparam name="TDelegate">The type of delegate which will have parameters matched to the constructor signature.</typeparam>
    /// <param name="objectType">The type of object to construct.</param>
    /// <returns>A delegate that calls the object constructor.</returns>
    public static TDelegate GetActivatorDelegate<TDelegate>([DynamicallyAccessedMembers(PublicConstructors)] Type objectType)
        where TDelegate : Delegate
    {
#pragma warning disable IL2067 // Justification: Only public ctors needed.
        return GetActivatorByDelegate<TDelegate>(objectType, false);
#pragma warning restore IL2067
    }

    /// <summary>
    /// Gets a delegate that calls a matching signature constructor on the given object type, optionally matching non-public constructors.
    /// </summary>
    /// <typeparam name="TDelegate">The type of delegate which will have parameters matched to the constructor signature.</typeparam>
    /// <param name="objectType">The type of object to construct.</param>
    /// <param name="nonPublic"><see langword="true"/> to search non-public constructors as well, otherwise <see langword="false"/>.</param>
    /// <returns>A delegate that calls the object constructor.</returns>
    /// <remarks>
    /// For performance reasons, this method should not be used to create delegates that call parameterless constructors - prefer <see
    /// cref="GetActivator{T}()"/> for a more optimal approach.
    /// </remarks>
    public static TDelegate GetActivatorByDelegate<TDelegate>([DynamicallyAccessedMembers(AllConstructors)] Type objectType, bool nonPublic)
        where TDelegate : Delegate
    {
        if (!_delegateActivatorCache.TryGetValue((objectType, typeof(TDelegate)), out var info))
            info = _delegateActivatorCache.GetOrAdd((objectType, typeof(TDelegate)), _ => CreateActivator<TDelegate>(objectType, nonPublic));

        if (!nonPublic && !info.IsPublic)
            ThrowNonPublicConstructorException(objectType);

        return (TDelegate)info.Activator;
    }

    private static (TDelegate Activator, bool IsPublic) CreateActivator<TDelegate>([DynamicallyAccessedMembers(AllConstructors)] Type objectType, bool nonPublic) where TDelegate : Delegate
    {
#pragma warning disable IL2090 // Justification: Delegates always generate metadata for the Invoke method.
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

        Expression expression;
        bool isPublic;

        if (objectType.IsValueType && parameterTypes.Length == 0)
        {
            isPublic = true;
            expression = Expression.New(objectType);
        }
        else
        {
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            ConstructorInfo constructor = objectType.GetConstructor(bindingFlags, null, parameterTypes, null) ?? throw new MissingMethodException("A matching constructor was not found.");

            isPublic = constructor.IsPublic;

            if (!nonPublic && !isPublic)
                ThrowNonPublicConstructorException(objectType);

            expression = Expression.New(constructor, parameterExpressions);
        }

        if (objectType.IsValueType && returnType != objectType)
            expression = Expression.Convert(expression, returnType);

        return (Expression.Lambda<TDelegate>(expression, parameterExpressions).Compile(), isPublic);
    }

    private static void ThrowNonPublicConstructorException(Type objectType) => throw new MissingMethodException($"Requested constructor for type '{objectType}' is not public.");
}