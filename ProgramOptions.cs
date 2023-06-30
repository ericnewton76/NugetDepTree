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

		[Option('s', "depthStyle", Default = DepthStyle.Spaces, HelpText = "style of output when writing level of depth of the depencies.  Options are Spaces, Tabs, Graph")]
		public DepthStyle DepthStyle { get; set; }

		[Option("spaces", Default = 2, HelpText = "Number of spaces to output when DepthStyle=Spaces.  Minimum is 1.")]
		public int DepthSpaces { get; set; }

		[Option('d', "max-depth", Default = -1, HelpText = "print package dependencies up to a specified depth. N=0 shows only direct dependencies.  default is -1 (full depth)")]
		public int Depth { get; set; }

		[Option("system", Default = false, HelpText = "Dont exclude System.* dependencies.  Default is false (ie doesnt follow System.* dependencies)")]
		public bool SystemDependencies { get; set; }

		[Value(0)]
		public string RepoPath { get; set; }

		[Option("hide-versions", Default = false, HelpText = "hides the package version information")]
		public bool HideVersions { get; set; }
	}
}
