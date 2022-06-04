using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace zadanie1
{
    public class SimpleContainer
    {
        /// <summary>
        /// Registry of Types that should have a static lifetime policy.
        /// </summary>
        private Dictionary<Type, object> _singletons = new Dictionary<Type, object>();

        /// <summary>
        /// Registry of Types that should be returned when a given Abstact class or Interface instance is requested.
        /// </summary>
        private Dictionary<Type, Type> _specification = new Dictionary<Type, Type>();

        private Dictionary<Type, object> _instances = new Dictionary<Type, object>();

        private Dictionary<Type, ConstructorInfo> _constructorCache = new Dictionary<Type, ConstructorInfo>();

        private Dag<Type> _dependencyResolutionGraph = new Dag<Type> { };

        private ConstructorInfo GetConstructor(Type type)
        {
            ConstructorInfo[] constructors = type.GetConstructors();
            ConstructorInfo[] attributedConstructors = 
                constructors.AsEnumerable()
                .Where(constructor => !(constructor.GetCustomAttribute(typeof(DependencyConstrutor)) is null))
                .ToArray();

            if(attributedConstructors.Length > 1)
            {
                throw new Exception("There is more than one constructor marked with DependencyConstructor");
            }
            if(attributedConstructors.Length == 0)
            {
                return attributedConstructors.First();
            }

            ConstructorInfo maxParamsConstructor =
                constructors.AsEnumerable().OrderBy(constructors => constructors.GetParameters().Length).Last();

            IEnumerable<ConstructorInfo> maxParamsLenConstructors =
                constructors.Where(constructor => constructor.GetParameters().Length ==
                                                  maxParamsConstructor.GetParameters().Length);

            if(maxParamsLenConstructors.Count() > 1)
            {
                throw new Exception("Type doesn't have only one constructor with maximum parameters number");
            }
            return maxParamsConstructor;
        }

        public void RegisterType<T>(bool Singleton) where T : class
        {
            /*
             * 1. get type of T
             * 2. Consider special cases
             * 
             * GetConstructor(type t):
             *      var constructor = ... GetMarkedConst ...
             *      if marked_const is null:
             *          constructor = ... GetConstructorFallback ...
             *      return constructor
             *      
             *      
             *     
             * RegisterType(type t
             *      
             *      this._dependencyResolutionGraph.AddVertex(t)
             *      var tConstructor = GetConstructor(t);
             *      for argType in .ArgsList():
             *          args.Push(RegisterType<argType>( ??? ))
             *          this._dependencyResolutionGraph.AddEdge(t, argType)
             *      this._constructorCache.Add(t, tConstructor
             *      
             * RegisterInstance<Foo>
             * Resolve<Bar> where Bar(Foo)?
             */
            if (Singleton)
            {
                if (!this._singletons.ContainsKey(typeof(T)))
                {
                    this._singletons[typeof(T)] = this.Resolve<T>();
                    this._instances[typeof(T)] = this._singletons[typeof(T)];
                }
            }
        }

        public void RegisterType<From, To>(bool Singleton) where To : class, From
        {
            this._specification[typeof(From)] = typeof(To);
            this.RegisterType<To>(Singleton);
        }

        public void RegisterInstance<T>(T instance)
        {
            this._instances[typeof(T)] = instance;
        }

        public T Resolve<T>() where T : class
        {
            /*
             * if (IsSpecialCase(t)):
             *      this._specialCaseHandlers(t).Handle(t)
             *      for now just make HandleSingleton, HandleSpecification, HandleInstance
             * else:
             *      if (!IsRegistered(t)):
             *          # problem 
             *          # RegisterType<Foo>(true) <- singleton
             *          # RegisterType<Bar>(false) where Bar(Foo)
             *          #
             *          Register(t)
             *      At This point ALL types that t possibly depends on are registered.
             *      ... construct from dependency graph ...
             */
            object? obj = null;
            Type type = typeof(T);
            if (this._singletons.ContainsKey(type))
            {
                return (T)this._singletons[type];
            }
            if (this._instances.ContainsKey(type))
            {
                return (T)this._instances[type];
            }
            // if (this._instances.ContainsKet(type))
            if (type.IsInterface || type.IsAbstract)
            {
                if (this._specification.ContainsKey(type))
                {
                    var result = typeof(SimpleContainer)?
                        .GetMethod("Resolve")?
                        .MakeGenericMethod(this._specification[type])
                        .Invoke(this, null);
                    return result is not null ? (T)result : throw new Exception("invalid method configuration.");
                }
                else
                {
                    throw new ArgumentException("specified abstract class/interface does not have a concrete type associated with it.");
                }
            }
            else
            {
                // if (!IsRegistered()) register
                // Recursive Resolve
                ConstructorInfo constructor = this._constructorCache[type];
                List<object> args = new List<object>();
                foreach (Type dependecy in this._dependencyResolutionGraph.Children(type))
                {
                    // invoke Resolve for typeof(dependecy) and add result to args
                }
                obj = constructor.Invoke(null, args.ToArray());
            }
            if (obj is null)
            {
                // throw implementation incorrect exception
            }
            return (T)obj;
        }
    }
}
