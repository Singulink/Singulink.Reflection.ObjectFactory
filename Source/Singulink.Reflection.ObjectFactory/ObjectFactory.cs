using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace Singulink.Reflection
{
    /// <summary>
    /// Provides methods to create objects and delegates that create objects based on specified constructor signatures.
    /// </summary>
    public static class ObjectFactory
    {
        #region Delegate Type Activators

        private static readonly ConcurrentDictionary<(Type ObjectType, Type DelegateType), (object Activator, bool IsPublic)> _delegateActivatorCache = new();

        /// <summary>
        /// Gets a delegate that calls a matching signature constructor on the given object type.
        /// </summary>
        /// <typeparam name="TDelegate">The type of delegate which parameters will be matched to the constructor signature.</typeparam>
        /// <param name="objectType">The type of object to construct.</param>
        /// <param name="nonPublic"><see langword="true"/> to get a matching non-public constructor, otherwise false.</param>
        /// <returns>A delegate that calls the object constructor.</returns>
        public static TDelegate GetActivatorDelegate<TDelegate>(Type objectType, bool nonPublic = false) where TDelegate : Delegate
        {
            if (!_delegateActivatorCache.TryGetValue((objectType, typeof(TDelegate)), out var info))
                info = _delegateActivatorCache.GetOrAdd((objectType, typeof(TDelegate)), k => CreateActivator<TDelegate>(k.ObjectType, nonPublic));

            if (!nonPublic && !info.IsPublic)
                ThrowNonPublicConstructorException(objectType);

            return (TDelegate)info.Activator;
        }

        #endregion

        #region Create Instance

        /// <summary>
        /// Creates an object of the specified type using the default constructor, optionally calling non-public constructors as well.
        /// </summary>
        public static object CreateInstance(Type type, bool nonPublic = false) => GetActivator(type, nonPublic).Invoke();

        /// <summary>
        /// Creates an object of the specified type using the default constructor, optionally calling non-public constructors as well.
        /// </summary>
        public static TObject CreateInstance<TObject>(bool nonPublic = false) => GetActivator<TObject>(nonPublic).Invoke();

        /// <summary>
        /// Creates an object of the specified type using the default constructor, optionally calling non-public constructors as well.
        /// </summary>
        public static TResult CreateInstance<TResult>(Type objectType, bool nonPublic = false) => GetActivator<TResult>(objectType, nonPublic).Invoke();

        #endregion

        #region Activator Delegate

        /// <summary>
        /// Gets an activator delegate that creates an object of the specified type using the default constructor, optionally calling non-public constructors
        /// as well.
        /// </summary>
        public static Func<object> GetActivator(Type objectType, bool nonPublic = false) => GetActivator<object>(objectType, nonPublic);

        /// <summary>
        /// Gets an activator delegate that creates an object of the specified type using the default constructor, optionally calling non-public constructors
        /// as well.
        /// </summary>
        public static Func<TObject> GetActivator<TObject>(bool nonPublic = false)
        {
            var info = DefaultActivatorCache<TObject>.Info;

            if (info.Activator == null)
                throw new MissingMethodException($"Type '{typeof(TObject)}' does not have a default constructor.");

            if (!nonPublic && !info.IsPublic)
                ThrowNonPublicConstructorException(typeof(TObject));

            return info.Activator;
        }

        /// <summary>
        /// Gets an activator delegate that creates an object of the specified type using the default constructor, optionally calling non-public constructors
        /// as well.
        /// </summary>
        public static Func<TResult> GetActivator<TResult>(Type objectType, bool nonPublic = false)
        {
            var info = DefaultActivatorCache<TResult>.GetTypedInfo(objectType, nonPublic);

            if (!nonPublic && !info.IsPublic)
                ThrowNonPublicConstructorException(objectType);

            return info.Activator;
        }

        /// <summary>
        /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter type, optionally
        /// calling non-public constructors as well.
        /// </summary>
        public static Func<TParam, TObject> GetActivator<TParam, TObject>(bool nonPublic = false)
            => GetActivatorDelegate<Func<TParam, TObject>>(typeof(TObject), nonPublic);

        /// <summary>
        /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter types, optionally
        /// calling non-public constructors as well.
        /// </summary>
        public static Func<TParam1, TParam2, TObject> GetActivator<TParam1, TParam2, TObject>(bool nonPublic = false)
            => GetActivatorDelegate<Func<TParam1, TParam2, TObject>>(typeof(TObject), nonPublic);

        /// <summary>
        /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter types, optionally
        /// calling non-public constructors as well.
        /// </summary>
        public static Func<TParam1, TParam2, TParam3, TObject> GetActivator<TParam1, TParam2, TParam3, TObject>(bool nonPublic = false)
            => GetActivatorDelegate<Func<TParam1, TParam2, TParam3, TObject>>(typeof(TObject), nonPublic);

        /// <summary>
        /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter types, optionally
        /// calling non-public constructors as well.
        /// </summary>
        public static Func<TParam1, TParam2, TParam3, TParam4, TObject> GetActivator<TParam1, TParam2, TParam3, TParam4, TObject>(bool nonPublic = false)
            => GetActivatorDelegate<Func<TParam1, TParam2, TParam3, TParam4, TObject>>(typeof(TObject), nonPublic);

        /// <summary>
        /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter types, optionally
        /// calling non-public constructors as well.
        /// </summary>
        public static Func<TParam, TResult> GetActivator<TParam, TResult>(Type objectType, bool nonPublic = false)
            => GetActivatorDelegate<Func<TParam, TResult>>(objectType, nonPublic);

        /// <summary>
        /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter types, optionally
        /// calling non-public constructors as well.
        /// </summary>
        public static Func<TParam1, TParam2, TResult> GetActivator<TParam1, TParam2, TResult>(Type objectType, bool nonPublic = false)
            => GetActivatorDelegate<Func<TParam1, TParam2, TResult>>(objectType, nonPublic);

        /// <summary>
        /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter types, optionally
        /// calling non-public constructors as well.
        /// </summary>
        public static Func<TParam1, TParam2, TParam3, TResult> GetActivator<TParam1, TParam2, TParam3, TResult>(Type objectType, bool nonPublic = false)
            => GetActivatorDelegate<Func<TParam1, TParam2, TParam3, TResult>>(objectType, nonPublic);

        /// <summary>
        /// Gets an activator delegate that creates an object of the specified type using the constructor that accepts the specified parameter types, optionally
        /// calling non-public constructors as well.
        /// </summary>
        public static Func<TParam1, TParam2, TParam3, TParam4, TResult> GetActivator<TParam1, TParam2, TParam3, TParam4, TResult>(Type objectType, bool nonPublic = false)
            => GetActivatorDelegate<Func<TParam1, TParam2, TParam3, TParam4, TResult>>(objectType, nonPublic);

        #endregion

        #region Formattable Object Factory Delegate

        /// <summary>
        /// Gets a delegate that creates an object of the specified type either by calling the default constructor (including non-public constructors) if it's
        /// available or by returning an uninitialized object.
        /// </summary>
        public static Func<object> GetFormattableObjectFactory(Type objectType)
        {
            try {
                return GetActivator(objectType, true);
            }
            catch (MissingMethodException) {
                return () => FormatterServices.GetUninitializedObject(objectType);
            }
        }

        /// <summary>
        /// Gets a delegate that creates an object of the specified type either by calling the default constructor (including non-public constructors) if it's
        /// available or by returning an uninitialized object.
        /// </summary>
        public static Func<TObject> GetFormattableObjectFactory<TObject>()
        {
            try {
                return GetActivator<TObject>(true);
            }
            catch (MissingMethodException) {
                return () => (TObject)FormatterServices.GetUninitializedObject(typeof(TObject));
            }
        }

        /// <summary>
        /// Gets a delegate that creates an object of the specified type either by calling the default constructor (including non-public constructors) if it's
        /// available or by returning an uninitialized object.
        /// </summary>
        public static Func<TResult> GetFormattableObjectFactory<TResult>(Type objectType)
        {
            try {
                return GetActivator<TResult>(objectType, true);
            }
            catch (MissingMethodException) {
                return () => (TResult)FormatterServices.GetUninitializedObject(typeof(TResult));
            }
        }

        #endregion

        private static (TDelegate Activator, bool IsPublic) CreateActivator<TDelegate>(Type objectType, bool nonPublic) where TDelegate : Delegate
        {
            MethodInfo? delegateInfo = typeof(TDelegate).GetMethod("Invoke");

            if (delegateInfo == null)
                throw new ArgumentException("Invalid delegate type: no invoke method found.");

            var returnType = delegateInfo.ReturnType;

            if (!returnType.IsAssignableFrom(objectType))
                throw new InvalidCastException("The delegate return type must be assignable from the object type.");

            var parameters = delegateInfo.GetParameters();
            var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();

            if (parameters.Any(p => p.IsOut) || parameterTypes.Any(p => p.IsByRef))
                throw new NotSupportedException("The delegate cannot have ref or out parameters.");

            var parameterExpressions = parameterTypes.Select((p, i) => Expression.Parameter(p, "p" + i)).ToArray();
            NewExpression newExpression;

            bool isPublic;

            if (objectType.IsValueType && parameterTypes.Length == 0) {
                isPublic = true;
                newExpression = Expression.New(objectType);
            }
            else {
                const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                ConstructorInfo? constructor = objectType.GetConstructor(bindingFlags, null, parameterTypes, null);

                if (constructor == null)
                    throw new MissingMethodException("A matching constructor was not found.");

                isPublic = constructor.IsPublic;

                if (!nonPublic && !isPublic)
                    ThrowNonPublicConstructorException(objectType);

                newExpression = Expression.New(constructor, parameterExpressions);
            }

            return (Expression.Lambda<TDelegate>(newExpression, parameterExpressions).Compile(), isPublic);
        }

        private static void ThrowNonPublicConstructorException(Type objectType)
        {
            throw new MissingMethodException($"Requested constructor for type '{objectType}' is not public.");
        }

        private static class DefaultActivatorCache<T>
        {
            private static readonly ConcurrentDictionary<Type, (Func<T> Activator, bool IsPublic)> _typedInfoCache = new();

            private static readonly (Func<T> Activator, bool IsPublic) _info = InitInfo();

            public static (Func<T> Activator, bool IsPublic) Info {
                get => _info.Activator != null ? _info : throw new MissingMethodException("A default constructor was not found.");
            }

            private static (Func<T> Activator, bool IsPublic) InitInfo()
            {
                try {
                    return CreateActivator<Func<T>>(typeof(T), true);
                }
                catch {
                    return default;
                }
            }

            public static (Func<T> Activator, bool IsPublic) GetTypedInfo(Type objectType, bool nonPublic)
            {
                if (!_typedInfoCache.TryGetValue(objectType, out var info))
                    info = _typedInfoCache.GetOrAdd(objectType, k => CreateActivator<Func<T>>(k, nonPublic));

                return info;
            }
        }
    }
}
