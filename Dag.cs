using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zadanie1
{
    public class InvalidEdgeException<N> : Exception
    {
        N vertex;

        public InvalidEdgeException(N vertex) : base($"Vertex {vertex} is not present in the graph.")
        {
            this.vertex = vertex;
        }
    }

    public class CycleDetectedException<N> : Exception
    {
        N source;
        N destination;
        public CycleDetectedException(N source, N destination) 
            : base($"Dag invariant was violated, addition of edge ({source}, {destination}) would create a cycle.") 
        { 
            this.source = source;
            this.destination = destination;
        }
    }


    public class Dag<N> where N : notnull
    {
        private Dictionary<N, List<N>> _adjacencyList = new Dictionary<N, List<N>>();

        public void AddVertex(N vertex)
        {
            if (!this._adjacencyList.ContainsKey(vertex))
            {
                this._adjacencyList.Add(vertex, new List<N>());
            }
        }

        public IReadOnlyCollection<N> Children(N parent)
        {
            return this._adjacencyList[parent].AsReadOnly();
        }

        public void AddEdge(N source, N destination)
        {
            if (!this._adjacencyList.ContainsKey(source))
            {
                throw new InvalidEdgeException<N>(source);
            }
            if (!this._adjacencyList.ContainsKey(destination))
            {
                throw new InvalidEdgeException<N>(destination);
            }
            this._adjacencyList[source].Add(destination);
            if (this.ContainsCycle())
            {
                this._adjacencyList[source].Remove(destination);
                throw new CycleDetectedException<N>(source, destination);
            }
        }
        enum Color
        {
            White,
            Grey,
            Black,
        }

        /// <summary>
        /// Recursive helper function for ContainsCycle().
        /// </summary>
        /// <param name="node"> A root node for DFS traversal. It must have a White Color associated with it in coloring. </param>
        /// <param name="coloring"> Association between Nodes of the Dag and their Color in Three Color DFS algorithm. </param>
        /// <returns> Determines if the Dag contains any cycles </returns>
        private bool ColoringDfsHelper(N node, Dictionary<N, Color> coloring)
        {
            coloring[node] = Color.Grey;
            foreach (N child in Children(node))
            {
                if (coloring[child] == Color.Grey)
                {
                    return true;
                }
                if (coloring[child] == Color.White)
                {
                    bool backedgeExists = ColoringDfsHelper(child, coloring);
                    // If back-edge was found bubble up result else continue traversal
                    if (backedgeExists)
                    {
                        return true;
                    }
                }
            }
            // If all descending paths were explored and no back-egdes were found then mark node as black
            coloring[node] = Color.Black;
            return false;
        }

        /// <summary>
        /// Checks if any cycles are present in current configuration of the graph.
        /// </summary>
        /// <returns> Boolean value determining if any cycles are present in current configuration of the graph. </returns>
        private bool ContainsCycle()
        {
            var coloring = new Dictionary<N, Color>();
            foreach (N node in this._adjacencyList.Keys)
            {
                coloring.Add(node, Color.White);
            }
            foreach (N node in this._adjacencyList.Keys)
            {
                bool backedgeExists = ColoringDfsHelper(node, coloring);
                if (backedgeExists)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
