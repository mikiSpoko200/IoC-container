

internal class DAG<N> where N : notnull
{
    private Dictionary<N, List<N>> _adjacency_list = new Dictionary<N, List<N>>();

    public DAG() {
        
    }

    public void AddNode(N node)
    {
        if (!this._adjacency_list.ContainsKey(node))
        {
            this._adjacency_list.Add(node, new List<N>());
        }
    }

    public void AddEdge(N source, N destination)
    {
        if (!this._adjacency_list.ContainsKey(source))
        {
            throw new ArgumentException("source node is not in the graph.");
        }
        if (!this._adjacency_list.ContainsKey(destination))
        {
            throw new ArgumentException("destincation node is not in the graph.");
        }
        this._adjacency_list[source].Add(destination);
        if (this.ContainsCycle())
        {
            this._adjacency_list[source].Remove(destination);
            throw new ArgumentException("specified edge violates DAG invariant by creating a cycle.");
        }
    }

    private bool ContainsCycle()
    {
        // dfs (from the root?)
        throw new NotImplementedException();
    }
}


public class SimpleContainer
{
    private static List<Type> _singletons = new List<Type>();
    private Dictionary<Type, Type> _specification = new Dictionary<Type, Type>();

    private DAG<Type> _dependency_graph = new DAG<Type>();

    public void RegisterType<T>(bool Singleton) where T : class
    {
        if (Singleton)
        {

        }
    }

    public void RegisterType<From, To>(bool Singleton) where To : From
    {
        
    }

    private void AddTypeDependency<T>() where T : class
    {
        try
        {

        }
        catch(ArgumentException err)
        {
            // we're only interested in cycle prevention case if node is not added just add it.
        }
    }

    /* 
     * Error conditions:
     *  - no zero argument contructor
     *  - 
     * */
    public T Resolve<T>()
    {
        /* bottom up traversal of _dependency_graph for T */
    }
}
