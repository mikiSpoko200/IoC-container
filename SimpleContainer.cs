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
                .Where(constructor => !(constructor.GetCustomAttribute(typeof(DependencyConstructor)) is null))
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

        private bool IsRegistered(Type type) {
            return this._constructorCache.ContainsKey(type);
        }

        private class ResolverTemporaryStubType { }

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
            if(!(typeof(T).IsInterface || typeof(T).IsAbstract))
            {
                this.RegisterTypeDependencies(typeof(T));
            }
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

        public void RegisterInstance<T>(T instance) where T : class
        { //!
            this._instances[typeof(T)] = instance;
        }

        private object ResolveType(Type type)
        {
            var result = typeof(SimpleContainer)?
                .GetMethod("Resolve")?
                .MakeGenericMethod(type)
                .Invoke(this, null);
            return result is not null ? result : throw new Exception("invalid dependency resolution method name");
        }

        public T Resolve<T>() where T : class
        {   
            Type type = typeof(T);
            
            #region Special cases
            if (this._singletons.ContainsKey(type))
            {
                return (T)this._singletons[type];
            }
            if (this._instances.ContainsKey(type))
            {
                return (T)this._instances[type];
            }
            if (type.IsInterface || type.IsAbstract)
            {
                if (this._specification.ContainsKey(type))
                {
                    return (T) this.ResolveType(this._specification[type]);
                }
                else
                {
                    throw new ArgumentException("specified abstract class/interface does not have a concrete type associated with it.");
                }
            }
            #endregion
            else {
                if (!this.IsRegistered(type))
                {
                    // at this point type is definitely not a singleton
                    this.RegisterTypeDependencies(type);
                }
                
                ConstructorInfo constructor = this._constructorCache[type];
                List<object> args = new List<object>();
                foreach (Type childType in this._dependencyResolutionGraph.Children(type))
                {
                    args.Add((T)this.ResolveType(childType));
                }
                var result = constructor.Invoke(args.ToArray());
                if (result is null)
                {
                    throw new Exception("implementation is invalid.");
                }
                return (T)result;
            }
        }
    }
}
