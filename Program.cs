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
        static void Main()
        {
            string repoFolder = "packages";
            if (Directory.Exists(repoFolder) == false)
            {
                Console.Write("Enter the local repo folder: ");
                repoFolder = Console.ReadLine();
            }

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
