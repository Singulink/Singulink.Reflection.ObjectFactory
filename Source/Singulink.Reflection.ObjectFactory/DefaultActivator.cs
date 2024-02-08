using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Singulink.Reflection;

#pragma warning disable IL2091 // Dynamic T requirements are specified in ObjectFactory and not needed here.

/// <summary>
/// Creates instances of objects using parameterless constructors.
/// </summary>
/// <remarks>
/// Instances of <see cref="DefaultActivator{T}"/> should not be created directly and its default value should not be used. Use the <see
/// cref="ObjectFactory.GetActivator{T}()"/> method to get a properly initialized instance.
/// </remarks>
public readonly struct DefaultActivator<T>
{
#if !NETSTANDARD
    // For value types, always use Activator.CreateInstance<T>().
    // For referece types:
    // * If running on JIT: always use delegate.
    // * If running on AOT: Activator.CreateInstance<T>() is faster than the delegate so use it if there is a public ctor.
    //   The delegate is faster than CreateInstance(Type).
    private static readonly bool _useGenericActivator = typeof(T).IsValueType ||
        (!RuntimeFeature.IsDynamicCodeCompiled && typeof(T).GetConstructor(Type.EmptyTypes) != null);
#endif

    private readonly Func<T>? _activator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultActivator{T}"/> struct.
    /// Only call this constructor if the type is a reference type.
    /// </summary>
    internal DefaultActivator(Func<T> activator)
    {
#if !NETSTANDARD
        Debug.Assert(!typeof(T).IsValueType, "Do not call this constructor for value types.");
#endif
        _activator = activator;
    }

    /// <summary>
    /// Returns a delegate that creates instances of <typeparamref name="T"/> using the parameterless constructor.
    /// </summary>
    /// <exception cref="InvalidOperationException">The activator was not properly initialized.</exception>
    /// <remarks>
    /// For performance reasons, calling <see cref="Invoke"/> directly is preferred over using the delegate returned by this method.
    /// </remarks>
    public Func<T> AsDelegate()
    {
#if !NETSTANDARD
        if (_useGenericActivator)
            return static () => Activator.CreateInstance<T>();
#endif

        if (_activator == null)
            ThrowNotInitialized();

        return _activator!;
    }

    /// <summary>
    /// Creates an instance of <typeparamref name="T"/> using the parameterless constructor.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Invoke()
    {
#if !NETSTANDARD
        if (_useGenericActivator)
            return Activator.CreateInstance<T>();
#endif

        if (_activator == null)
            ThrowNotInitialized();

        return _activator!.Invoke();
    }

    private static void ThrowNotInitialized() => throw new InvalidOperationException("Activator was not properly initialized.");
}