using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zadanie1
{
    internal class Dag<N> where N : notnull
    {
        private Dictionary<N, List<N>> _adjacency_list = new Dictionary<N, List<N>>();

        public Dag()
        {
            throw new NotImplementedException();
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

        enum Color
        {
            White,
            Grey,
            Black,
        }

        private bool ContainsCycle()
        {
            if (this._adjacency_list.Count > 0)
            {
                Dictionary<N, Color> coloring = new Dictionary<N, Color>();
                Stack<N> search_stack = new Stack<N>();
                search_stack.Push(this._adjacency_list.Keys.First());
                foreach (N node in this._adjacency_list.Keys)
                {
                    coloring.Add(node, Color.White);
                }

                while (search_stack.Count > 0)
                {
                    N current_node = search_stack.Pop();
                    coloring[current_node] = Color.Grey;
                    foreach (N child in this._adjacency_list[current_node])
                    {
                        if (coloring[child] == Color.Grey)
                        {
                            return true;
                        }
                        if (coloring[child] == Color.White)
                        {

                        }
                    }
                }

            }

            return false;
        }
    }
}
