using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zadanie1.Tests
{
    public class SimpleContainerType
    { }

    public class SimpleContainerType2
    { }

    public interface ITested
    { }

    public class Tested : ITested
    { }

    public class Tested2 : ITested
    { }

    public class Tested3
    {
        [DependencyConstructor]
        public Tested3(int a) { }

        [DependencyConstructor]
        public Tested3(float b) { }
    }
}
