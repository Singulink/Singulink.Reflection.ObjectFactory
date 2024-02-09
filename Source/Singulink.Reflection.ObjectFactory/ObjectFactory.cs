using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Singulink.Reflection;

#pragma warning disable SA1010 // Opening square brackets should be spaced correctly triggers on collection literals.

/// <summary>
/// Provides methods to create objects and delegates that create instances based on specified constructor signatures.
/// </summary>
public static class ObjectFactory
{
    private const DynamicallyAccessedMemberTypes AllConstructors = DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors;
    private const DynamicallyAccessedMemberTypes PublicDefaultConstructor = DynamicallyAccessedMemberTypes.PublicParameterlessConstructor;
    private const DynamicallyAccessedMemberTypes PublicConstructors = DynamicallyAccessedMemberTypes.PublicConstructors;
    private const DynamicallyAccessedMemberTypes DefaultConstructors = DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.NonPublicConstructors;

    private record struct Key(
        [property: DynamicallyAccessedMembers(AllConstructors)]
        [param: DynamicallyAccessedMembers(AllConstructors)]
        Type ObjectType,
        Type DelegateType);

    private record struct DelegateInfo(Delegate Activator, bool IsPublic);

    private static readonly ConcurrentDictionary<Key, DelegateInfo> s_delegateActivatorCache = new();

    /// <summary>
    /// Creates an object of the specified type using the public default constructor.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T CreateInstance<[DynamicallyAccessedMembers(PublicDefaultConstructor)] T>()
    {
#pragma warning disable IL2091 // Only public default ctor needed.
        return CreateInstance<T>(false);
#pragma warning restore IL2091
    }

    /// <summary>
    /// Creates an object of the specified type using the default constructor, optionally calling a non-public constructor as well.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T CreateInstance<[DynamicallyAccessedMembers(DefaultConstructors)] T>(bool nonPublic)
    {
#if !NETSTANDARD
        // Duplicate IsValueType check for AOT. Do not remove.
        if (typeof(T).IsValueType || DefaultActivator<T>.UseSystemActivator)
            return Activator.CreateInstance<T>();
#endif

        if (!nonPublic && !DefaultActivator<T>.IsPublic)
            ThrowNonPublicCtorException(typeof(T));

        var activatorDelegate = DefaultActivator<T>.Delegate;

        if (activatorDelegate == null)
            ThrowNoParameterlessCtor(typeof(T));

        return activatorDelegate!.Invoke();
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
    /// Gets an activator that creates objects of the specified type using the public default constructor.
    /// </summary>
    /// <remarks>
    /// You can get a <see cref="Func{TResult}"/> delegate by calling <see cref="DefaultActivator{T}.AsDelegate"/> on the returned activator.
    /// </remarks>
    public static DefaultActivator<T> GetActivator<[DynamicallyAccessedMembers(PublicDefaultConstructor)] T>()
    {
#pragma warning disable IL2091 // Only public default ctor needed.
        return GetActivator<T>(false);
#pragma warning restore IL2091
    }

    /// <summary>
    /// Gets an activator that creates objects of the specified type using the default constructor, optionally calling a non-public constructor as well.
    /// </summary>
    /// <remarks>
    /// You can get a <see cref="Func{TResult}"/> delegate with the <see cref="DefaultActivator{T}.AsDelegate"/> method on the returned activator.
    /// </remarks>
    public static DefaultActivator<T> GetActivator<[DynamicallyAccessedMembers(DefaultConstructors)] T>(bool nonPublic)
    {
#if !NETSTANDARD
        if (typeof(T).IsValueType)
            return default;
#endif
        var activatorDelegate = DefaultActivator<T>.Delegate;

        if (activatorDelegate == null)
            ThrowNoParameterlessCtor(typeof(T));

        if (!nonPublic && !DefaultActivator<T>.IsPublic)
            ThrowNonPublicCtorException(typeof(T));

        return new(activatorDelegate!);
    }

    /// <summary>
    /// Gets an activator delegate that creates objects of the specified type using the public constructor that accepts the specified parameter type.
    /// </summary>
    public static Func<TParam, T> GetActivator<TParam, [DynamicallyAccessedMembers(PublicConstructors)] T>()
    {
#pragma warning disable IL2091 // Only public ctors needed.
        return GetActivator<TParam, T>(false);
#pragma warning restore IL2091
    }

    /// <summary>
    /// Gets an activator delegate that creates objects of the specified type using the constructor that accepts the specified parameter type, optionally
    /// calling non-public constructors as well.
    /// </summary>
    public static Func<TParam, T> GetActivator<TParam, [DynamicallyAccessedMembers(AllConstructors)] T>(bool nonPublic)
    {
#if NET8_0_OR_GREATER
        if (!RuntimeFeature.IsDynamicCodeCompiled)
        {
            return GetOrCreateActivatorByDelegate<Func<TParam, T>>(typeof(T), nonPublic, static _ => {
                var constructor = GetConstructor(typeof(T), [typeof(TParam)]);
                var invoker = ConstructorInvoker.Create(constructor);
                return new((TParam p) => (T)invoker.Invoke(p), constructor.IsPublic);
            });
        }
#endif

        return GetActivatorByDelegate<Func<TParam, T>>(typeof(T), nonPublic);
    }

    /// <summary>
    /// Gets an activator delegate that creates objects of the specified type using the public constructor that accepts the specified parameter types.
    /// </summary>
    public static Func<TParam1, TParam2, T> GetActivator<TParam1, TParam2, [DynamicallyAccessedMembers(PublicConstructors)] T>()
    {
#pragma warning disable IL2091 // Only public ctors needed.
        return GetActivator<TParam1, TParam2, T>(false);
#pragma warning restore IL2091
    }

    /// <summary>
    /// Gets an activator delegate that creates objects of the specified type using the constructor that accepts the specified parameter types, optionally
    /// calling non-public constructors as well.
    /// </summary>
    public static Func<TParam1, TParam2, T> GetActivator<TParam1, TParam2, [DynamicallyAccessedMembers(AllConstructors)] T>(bool nonPublic)
    {
#if NET8_0_OR_GREATER
        if (!RuntimeFeature.IsDynamicCodeCompiled)
        {
            return GetOrCreateActivatorByDelegate<Func<TParam1, TParam2, T>>(typeof(T), nonPublic, static _ => {
                var constructor = GetConstructor(typeof(T), [typeof(TParam1), typeof(TParam2)]);
                var invoker = ConstructorInvoker.Create(constructor);
                return new((TParam1 p1, TParam2 p2) => (T)invoker.Invoke(p1, p2), constructor.IsPublic);
            });
        }
#endif

        return GetActivatorByDelegate<Func<TParam1, TParam2, T>>(typeof(T), nonPublic);
    }

    /// <summary>
    /// Gets an activator delegate that creates objects of the specified type using the public constructor that accepts the specified parameter types.
    /// </summary>
    public static Func<TParam1, TParam2, TParam3, T> GetActivator<TParam1, TParam2, TParam3, [DynamicallyAccessedMembers(PublicConstructors)] T>()
    {
#pragma warning disable IL2091 // Only public ctors needed.
        return GetActivator<TParam1, TParam2, TParam3, T>(false);
#pragma warning restore IL2091
    }

    /// <summary>
    /// Gets an activator delegate that creates objects of the specified type using the constructor that accepts the specified parameter types, optionally
    /// calling non-public constructors as well.
    /// </summary>
    public static Func<TParam1, TParam2, TParam3, T> GetActivator<TParam1, TParam2, TParam3, [DynamicallyAccessedMembers(AllConstructors)] T>(bool nonPublic)
    {
#if NET8_0_OR_GREATER
        if (!RuntimeFeature.IsDynamicCodeCompiled)
        {
            return GetOrCreateActivatorByDelegate<Func<TParam1, TParam2, TParam3, T>>(typeof(T), nonPublic, static _ => {
                var constructor = GetConstructor(typeof(T), [typeof(TParam1), typeof(TParam2), typeof(TParam3)]);
                var invoker = ConstructorInvoker.Create(constructor);
                return new((TParam1 p1, TParam2 p2, TParam3 p3) => (T)invoker.Invoke(p1, p2, p3), constructor.IsPublic);
            });
        }
#endif

        return GetActivatorByDelegate<Func<TParam1, TParam2, TParam3, T>>(typeof(T), nonPublic);
    }

    /// <summary>
    /// Gets an activator delegate that creates objects of the specified type using the public constructor that accepts the specified parameter types.
    /// </summary>
    public static Func<TParam1, TParam2, TParam3, TParam4, T> GetActivator<TParam1, TParam2, TParam3, TParam4, [DynamicallyAccessedMembers(PublicConstructors)] T>()
    {
#pragma warning disable IL2091 // Only public ctors needed.
        return GetActivator<TParam1, TParam2, TParam3, TParam4, T>(false);
#pragma warning restore IL2091
    }

    /// <summary>
    /// Gets an activator delegate that creates objects of the specified type using the constructor that accepts the specified parameter types, optionally
    /// calling non-public constructors as well.
    /// </summary>
    public static Func<TParam1, TParam2, TParam3, TParam4, T> GetActivator<TParam1, TParam2, TParam3, TParam4, [DynamicallyAccessedMembers(AllConstructors)] T>(bool nonPublic)
    {
#if NET8_0_OR_GREATER
        if (!RuntimeFeature.IsDynamicCodeCompiled)
        {
            return GetOrCreateActivatorByDelegate<Func<TParam1, TParam2, TParam3, TParam4, T>>(typeof(T), nonPublic, static _ => {
                var constructor = GetConstructor(typeof(T), [typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4)]);
                var invoker = ConstructorInvoker.Create(constructor);
                return new((TParam1 p1, TParam2 p2, TParam3 p3, TParam4 p4) => (T)invoker.Invoke(p1, p2, p3, p4), constructor.IsPublic);
            });
        }
#endif

        return GetActivatorByDelegate<Func<TParam1, TParam2, TParam3, TParam4, T>>(typeof(T), nonPublic);
    }

    /// <summary>
    /// Gets an activator delegate that creates objects of the specified type using the public constructor that accepts the specified parameter types.
    /// </summary>
    public static Func<TParam1, TParam2, TParam3, TParam4, TParam5, T> GetActivator<TParam1, TParam2, TParam3, TParam4, TParam5, [DynamicallyAccessedMembers(PublicConstructors)] T>()
    {
#pragma warning disable IL2091 // Only public ctors needed.
        return GetActivator<TParam1, TParam2, TParam3, TParam4, TParam5, T>(false);
#pragma warning restore IL2091
    }

    /// <summary>
    /// Gets an activator delegate that creates objects of the specified type using the constructor that accepts the specified parameter types, optionally
    /// calling non-public constructors as well.
    /// </summary>
    public static Func<TParam1, TParam2, TParam3, TParam4, TParam5, T> GetActivator<TParam1, TParam2, TParam3, TParam4, TParam5, [DynamicallyAccessedMembers(AllConstructors)] T>(bool nonPublic)
    {
#if NET8_0_OR_GREATER
        if (!RuntimeFeature.IsDynamicCodeCompiled)
        {
            return GetOrCreateActivatorByDelegate<Func<TParam1, TParam2, TParam3, TParam4, TParam5, T>>(typeof(T), false, static _ => {
                var constructor = GetConstructor(typeof(T), [typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5)]);
                var invoker = ConstructorInvoker.Create(constructor);

                return new DelegateInfo(
                    (TParam1 p1, TParam2 p2, TParam3 p3, TParam4 p4, TParam5 p5) => {
                        Span<object?> args = [p1, p2, p3, p4, p5];
                        return (T)invoker.Invoke(args);
                    },
                    constructor.IsPublic);
            });
        }
#endif

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
#pragma warning disable IL2067 // Only public ctors needed.
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
        return GetOrCreateActivatorByDelegate<TDelegate>(objectType, nonPublic, static key => CreateActivator<TDelegate>(key.ObjectType, true));
    }

    internal static ConstructorInfo GetConstructor([DynamicallyAccessedMembers(AllConstructors)] Type objectType, Type[] parameterTypes)
    {
        const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        return objectType.GetConstructor(bindingFlags, null, parameterTypes, null) ?? throw new MissingMethodException("A matching constructor was not found.");
    }

    private static TDelegate GetOrCreateActivatorByDelegate<TDelegate>([DynamicallyAccessedMembers(AllConstructors)] Type objectType, bool nonPublic, Func<Key, DelegateInfo> infoFactory)
        where TDelegate : Delegate
    {
        var key = new Key(objectType, typeof(TDelegate));
        var info = s_delegateActivatorCache.GetOrAdd(key, infoFactory);

        if (!nonPublic && !info.IsPublic)
            ThrowNonPublicCtorException(objectType);

        return (TDelegate)info.Activator;
    }

    private static DelegateInfo CreateActivator<TDelegate>([DynamicallyAccessedMembers(AllConstructors)] Type objectType, bool nonPublic)
        where TDelegate : Delegate
    {
#pragma warning disable IL2090 // Delegates always generate metadata for the Invoke method.
        const BindingFlags invokeBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        var delegateInfo = typeof(TDelegate).GetMethod("Invoke", invokeBindingFlags);
#pragma warning restore IL2090

        if (delegateInfo == null)
            throw new MissingMethodException("Missing metadata for delegate 'Invoke' method.");

        var returnType = delegateInfo.ReturnType;

        if (returnType.IsByRef)
            throw new NotSupportedException("The constructor cannot have a ref return type.");

        if (!returnType.IsAssignableFrom(objectType))
            throw new InvalidCastException("The object type must be assignable to the delegate return type.");

        var parameters = delegateInfo.GetParameters();
        var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();

        if (parameters.Any(p => p.IsOut) || parameterTypes.Any(p => p.IsByRef))
            throw new NotSupportedException("The constructor cannot have ref or out parameters.");

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
            var constructor = GetConstructor(objectType, parameterTypes);

            isPublic = constructor.IsPublic;

            if (!nonPublic && !isPublic)
                ThrowNonPublicCtorException(objectType);

            expression = Expression.New(constructor, parameterExpressions);
        }

        if (objectType.IsValueType && returnType != objectType)
            expression = Expression.Convert(expression, returnType);

        return new(Expression.Lambda<TDelegate>(expression, parameterExpressions).Compile(), isPublic);
    }

    private static void ThrowNoParameterlessCtor(Type type) => throw new MissingMethodException($"No parameterless constructor defined for type '{type}'.");

    private static void ThrowNonPublicCtorException(Type type) => throw new MissingMethodException($"Requested constructor for type '{type}' is not public.");
}