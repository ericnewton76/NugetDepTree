using NuGet;
using System;
using System.Collections.Generic;
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
            string repoFolder = GetValidPackagesPath(args);

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

        private static string GetValidPackagesPath(string[] args)
        {
            string specifiedFolder = "packages";
            if (args.Length > 0) specifiedFolder = args[0];

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
                
                var depthStr = new string(' ', depth * 2);
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

                OutputGraph(repository, dependentPackages.Distinct(__comparer), depth+1);
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
