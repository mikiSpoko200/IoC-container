using System.Reflection;

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

    private Dictionary<Type, ConstructorInfo> _constructor_cache = new Dictionary<Type, ConstructorInfo>();

    public void RegisterType<T>(bool Singleton) where T : class
    {
        if (Singleton)
        {
            this._singletons[typeof(T)] = this.Resolve<T>();
        }
    }

    public void RegisterType<From, To>(bool Singleton) where To : From
    {
        this._specification[typeof(From)] = typeof(To);
    }

    public T Resolve<T>() where T: class
    {
        Type type = typeof(T);

        if (this._constructor_cache.ContainsKey(type))
        {
            return (T)this._constructor_cache[type].Invoke(null);
        }
        if (type.IsInterface || type.IsAbstract)
        {
            if (this._specification.ContainsKey(type))
            {
                var result = typeof(SimpleContainer)?
                    .GetMethod("Resolve")?
                    .MakeGenericMethod(this._specification[type])
                    .Invoke(null, null);
                return result is not null ? (T)result : throw new Exception("invalid method configuration.");
            }
            else
            {
                throw new ArgumentException("specified abstract class/interface does not have a concrete type associated with it.");
            }   
        }    
        else if (this._singletons.ContainsKey(type))
        {
            return (T)this._singletons[type];
        } 
        else
        {
            ConstructorInfo? constructor = type.GetConstructor(new Type[] { });
            if (constructor is not null)
            {
                this._constructor_cache.Add(type, constructor);
                return (T)this._constructor_cache[type].Invoke(null);
            }
            else
            {
                throw new ArgumentException("type specified does not have a default constructor.");
            }
        }
    }
}
