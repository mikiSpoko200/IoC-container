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

        /// <summary>
        /// Registry of externally created objects that should be returned when type is requested.
        /// <summary
        private Dictionary<Type, object> _instances = new Dictionary<Type, object>();
        
        /// <summary>
        /// Association of type and the corresponding constructor that should be invoked upon creation.
        /// <summary>
        private Dictionary<Type, ConstructorInfo> _constructorCache = new Dictionary<Type, ConstructorInfo>();

        /// <summary>
        /// Graph of dependencies between types.
        /// <summary>
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
            if(attributedConstructors.Length == 1)
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

        private IEnumerable<Type> GetParameterTypes(ConstructorInfo constructor)
        {
            return constructor.GetParameters()
                .AsEnumerable()
                .Select(p => p.ParameterType);
        }

        private class ResolverTemporaryStubType { }

        /// <summary>
        /// 
        ///
        ///
        ///
        ///
        private void RegisterTypeDependencies(Type type)
        {
            List<Type> addedTypes = new List<Type>();
            var unresolvedTypes = new Stack<(Type, Type)>();
            
            // temporary internal stub type removed at the end, makes code in the loop below cleaner.
            Type temp = typeof(ResolverTemporaryStubType);
            unresolvedTypes.Push((type, temp));
            this._dependencyResolutionGraph.AddVertex(temp);

            // loop invariant: parentType is in the graph
            while(unresolvedTypes.Count > 0)
            {
                var (childType, parentType) = unresolvedTypes.Pop();
                if (!this._dependencyResolutionGraph.ContainsVertex(childType))
                {
                    ConstructorInfo constructor = this.GetConstructor(childType);

                    foreach(var grandChildType in this.GetParameterTypes(constructor))
                    {
                        unresolvedTypes.Push((grandChildType, childType));
                    }

                    this._dependencyResolutionGraph.AddVertex(childType);
                    addedTypes.Add(childType);
                    try
                    {
                        this._dependencyResolutionGraph.AddEdge(parentType, childType);
                    }
                    catch (CycleDetectedException<Type>)
                    {
                        // clean up all newly added vertices.
                        addedTypes.ForEach(type => this._dependencyResolutionGraph.DeleteSource(type));
                        throw;
                    }
                    // if no cycle was created cache constructor for parent type.
                    this._constructorCache.Add(childType, constructor);
                }
            }
            this._dependencyResolutionGraph.DeleteSource(temp);
        }

        public void RegisterType<T>(bool Singleton) where T : class
        {
            this.RegisterTypeDependencies(typeof(T));
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
