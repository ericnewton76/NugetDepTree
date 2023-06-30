using NuGet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetDepTree
{

    internal class PackageComparer : IEqualityComparer<IPackage>
    {

        public bool Equals(IPackage x, IPackage y)
        {
            if (x == null && y == null) return true;

            if (x == null) return false;
            if (y == null) return false;

            if (x.Id != y.Id) return false;
            if (x.Version != y.Version) return false;

            return true;
        }

        public int GetHashCode(IPackage obj)
        {
            int hash = 17;
            hash *= 23 + obj.Id.GetHashCode();
            hash *= 23 + obj.Version.Version.GetHashCode();
            return hash;
        }
    }

}
