using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Singulink.Reflection;

// Dynamic T requirements are specified in ObjectFactory and not needed here.
#pragma warning disable IL2091
#pragma warning disable IL2087

/// <summary>
/// Creates instances of objects using parameterless constructors.
/// </summary>
/// <remarks>
/// Instances of <see cref="DefaultActivator{T}"/> should not be created directly and its default value should not be used. Use the <see
/// cref="ObjectFactory.GetActivator{T}()"/> method to get a properly initialized instance.
/// </remarks>
public readonly struct DefaultActivator<T>
{
    internal static bool IsPublic { get; } = typeof(T).IsValueType || typeof(T).GetConstructor(Type.EmptyTypes) is not null;

    internal static Func<T>? Delegate { get; private set; } = TryGetDelegate();

#if !NETSTANDARD
    // For value types, always use Activator.CreateInstance<T>().
    // For referece types:
    // * If running on JIT: always use delegate.
    // * If running on AOT: Activator.CreateInstance<T>() is faster than the delegate so use it if there is a public ctor.
    //   The delegate is faster than CreateInstance(Type).
    internal static bool UseSystemActivator { get; } = typeof(T).IsValueType ||
        (!RuntimeFeature.IsDynamicCodeCompiled && IsPublic);
#endif

    private readonly Func<T>? _delegate;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultActivator{T}"/> struct.
    /// Only call this constructor if the type is a reference type.
    /// </summary>
    internal DefaultActivator(Func<T> activatorDelegate)
    {
#if !NETSTANDARD
        Debug.Assert(!typeof(T).IsValueType, "Do not call this constructor for value types.");
#endif
        _delegate = activatorDelegate;
    }

    /// <summary>
    /// Returns a delegate that creates instances of <typeparamref name="T"/> using the parameterless constructor.
    /// </summary>
    /// <exception cref="InvalidOperationException">The activator was not properly initialized.</exception>
    /// <remarks>
    /// For performance reasons, calling <see cref="Invoke"/> directly on the default activator is preferred over using the delegate returned by this method.
    /// </remarks>
    public Func<T> AsDelegate()
    {
#if !NETSTANDARD
        // Duplicate IsValueType check for AOT. Do not remove.
        if (typeof(T).IsValueType || UseSystemActivator)
            return Delegate ??= static () => Activator.CreateInstance<T>();
#endif

        if (_delegate == null)
            ThrowNotInitialized();

        return _delegate!;
    }

    /// <summary>
    /// Creates an instance of <typeparamref name="T"/> using the parameterless constructor.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Invoke()
    {
#if !NETSTANDARD
        // Duplicate IsValueType check for AOT. Do not remove.
        if (typeof(T).IsValueType || UseSystemActivator)
            return Activator.CreateInstance<T>();
#endif

        if (_delegate == null)
            ThrowNotInitialized();

        return _delegate!.Invoke();
    }

    private static Func<T>? TryGetDelegate()
    {
        try
        {
#if !NETSTANDARD
            if (typeof(T).IsValueType)
            {
                return null;
            }
#endif
#if NET8_0_OR_GREATER
            else if (!RuntimeFeature.IsDynamicCodeCompiled)
            {
                var constructor = ObjectFactory.GetConstructor(typeof(T), Type.EmptyTypes);
                var invoker = ConstructorInvoker.Create(constructor);

                return () => (T)invoker.Invoke();
            }
#endif
            return ObjectFactory.GetActivatorByDelegate<Func<T>>(typeof(T), true);
        }
        catch (Exception ex)
        {
            Trace.TraceWarning($"[ObjectFactory] Failed to create default activator for type {typeof(T).FullName}: {ex}");
            return null;
        }
    }

    private static void ThrowNotInitialized() => throw new InvalidOperationException("Activator was not properly initialized.");
}