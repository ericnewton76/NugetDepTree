using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetDepTree
{
    internal enum DepthStyle
    {
        Unspecified = 0,
        Spaces = 1,
        Tabs = 2,
        Graph = 3
    }

    internal class ProgramOptions
    {

        public DepthStyle DepthStyle { get; set; }

    }
}
