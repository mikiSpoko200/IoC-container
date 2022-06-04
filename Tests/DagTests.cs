using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zadanie1.Tests
{
    [TestClass]
    public class DagTests
    {
        class Foo 
        {
            public int foo;
        }

        class Bar
        { 
            public int bar;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEdgeException<int>), "Edge was inappropriately added despite that one vertex was not in a graph.")]
        public void TestNoCycle()
        {
            var dag = new Dag<int>();
            dag.AddVertex(1);
            dag.AddEdge(1, 3);
        }


        [TestMethod]
        [ExpectedException(typeof(CycleDetectedException<int>), "A cycle was inappropriately allowed.")]
        public void TestCycle1()
        {
            var dag = new Dag<int>();
            dag.AddVertex(1);
            dag.AddVertex(2);
            dag.AddVertex(3);
            dag.AddEdge(1, 2);
            dag.AddEdge(2, 3);
            dag.AddEdge(3, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(CycleDetectedException<int>), "A cycle was inappropriately allowed.")]
        public void TestCycle2()
        {
            var dag = new Dag<int>();
            dag.AddVertex(1);
            dag.AddVertex(2);
            dag.AddEdge(1, 2);
            dag.AddEdge(2, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(CycleDetectedException<int>), "A cycle was inappropriately allowed.")]
        public void TestCycle3()
        {
            var dag = new Dag<int>();
            dag.AddVertex(1);
            dag.AddEdge(1, 1);
        }
    }
}
