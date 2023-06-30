using CommandLine;
using NuGet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NugetDepTree
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<ProgramOptions>(args)
                .WithParsed((options) => Run(options))
                .WithNotParsed((errors) =>
                {
                    foreach (var error in errors)
                        Console.WriteLine("Error: {0} {1}", error.Tag, error.StopsProcessing);
                });

            if (Debugger.IsAttached) { Console.Write("Program ended."); Console.ReadLine(); }
         }

        static void Run(ProgramOptions options)
        {
            Program.options = options;
            ValidateOptions(options);

            string repoFolder = GetValidPackagesPath(options.RepoPath);

            tryagain:
            var repo = new LocalPackageRepository(repoFolder);
            IQueryable<IPackage> packages = repo.GetPackages();
            if (packages.Count() == 0 && repoFolder.EndsWith("packages") == false)
            {
                repoFolder = Path.Combine(repoFolder, "packages");
                goto tryagain;
            }

            OutputGraph(repo, packages, 0);
        }

        static void ValidateOptions(ProgramOptions options)
        {
            //reset depth if anything less than -1
            if (options.Depth < -1) options.Depth = -1;

            if (options.DepthSpaces < 1) options.DepthSpaces = 1;
        }

        private static ProgramOptions options;

        private static string GetValidPackagesPath(string startingPath)
        {
            string specifiedFolder = startingPath ?? "packages";

            while(Directory.Exists(specifiedFolder) == false && Directory.Exists(Path.Combine(specifiedFolder,"packages"))==false)
            {
                Console.WriteLine("Directory '{0}' doesnt exist.", Path.GetFullPath(specifiedFolder));
                Console.Write("Enter path for the 'packages' folder: ");
                specifiedFolder = Console.ReadLine();
            }

            return specifiedFolder;
        }


        private static PackageComparer __comparer = new PackageComparer();
        private static string[] _corepackages = GenerateCorePackageList();

        static void OutputGraph(LocalPackageRepository repository, IEnumerable<IPackage> packages, int depth)
        {
            var depthStrings = new Dictionary<int, string>();

            foreach (IPackage package in packages)
            {
                if (_corepackages.Contains(package.Id)) continue;

                string depthStr;
                if(depthStrings.TryGetValue(depth, out depthStr) == false)
                {
                    switch(options.DepthStyle)
                    {
                        case DepthStyle.Spaces:
                            depthStr = new string(' ', depth * options.DepthSpaces);
                            break;
                        case DepthStyle.Tabs:
                            depthStr = new string('\t', depth);
                            break;
                        case DepthStyle.Graph:
                            var sb = new StringBuilder(new string('-', depth * 2));
                            if (depth > 0) sb[0] = '\\';
                            depthStr = sb.ToString();
                            break;
                    }
                    
                    depthStrings[depth] = depthStr;
                }

                Console.WriteLine("{0}{1} v{2}", depthStr, package.Id, package.Version);

                IList<IPackage> dependentPackages = new List<IPackage>();
                foreach (var dependencySet in package.DependencySets)
                {
                    foreach (var dependency in dependencySet.Dependencies)
                    {
                        var dependentPackage = repository.FindPackage(dependency.Id, dependency.VersionSpec, true, true);
                        if (dependentPackage != null)
                        {
                            //every package depends on System.Runtime
                            if(dependentPackage.Id != "System.Runtime")
                                dependentPackages.Add(dependentPackage);
                        }
                    }
                }

                if (options.Depth == -1 || depth + 1 <= options.Depth)
                {
                    OutputGraph(repository, dependentPackages.Distinct(__comparer), depth + 1);
                }
            }
        }

        private static string[] GenerateCorePackageList()
        {
            return new string[]
            {
                "System.Runtime",
                "System.Reactive"
            };
        }
    }
}
