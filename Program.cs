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
            var parser = new CommandLine.Parser((settings) =>
            {
                settings.IgnoreUnknownArguments = true;
            });

            parser.ParseArguments<ProgramOptions>(args)
                .WithParsed((options) => Run(options))
                .WithNotParsed((errors) =>
                {
                    foreach (var error in errors)
                        Console.WriteLine("Error: {0} {1}", error.Tag, error.StopsProcessing);
                });

            ProgramEnd.Wait(10);
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
            foreach (IPackage package in packages)
            {
                if (_corepackages.Contains(package.Id)) continue;

                //write package info to output
                WritePackageInfo(depth, package);

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

                bool godeeper = true;

                if (package.Id.StartsWith("System.") == true && options.SystemDependencies == false)
                {
                    godeeper = false;
                }

                if (depth + 1 <= options.Depth)
                {
                    godeeper = false;
                }
                
                if(godeeper)
                {
                    OutputGraph(repository, dependentPackages.Distinct(__comparer), depth + 1);
                }
            }
        }


        private static Dictionary<int,string> S_depthStrings = new Dictionary<int, string>();

        /// <summary>
        /// Writes the package information to output
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="depthStrings"></param>
        /// <param name="package"></param>
        private static void WritePackageInfo(int depth, IPackage package)
        {
            string depthStr;
            if (S_depthStrings.TryGetValue(depth, out depthStr) == false)
            {
                switch (options.DepthStyle)
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

                S_depthStrings[depth] = depthStr;
            }

            if (options.HideVersions)
            {
                Console.WriteLine("{0}{1}", depthStr, package.Id, package.Version);
                return;
            }
            
            Console.Write("{0}{1} ", depthStr, package.Id, package.Version);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("{0}", package.Version);
            Console.ResetColor();

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
