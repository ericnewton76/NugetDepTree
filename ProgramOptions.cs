using CommandLine;

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

        [Option('s', "depthStyle", Default=DepthStyle.Spaces)]
        public DepthStyle DepthStyle { get; set; }

        [Option('d', "depth", Default=-1)]
        public int Depth { get; set; }

        [Value(0)]
        public string RepoPath { get; set; }

    }
}
