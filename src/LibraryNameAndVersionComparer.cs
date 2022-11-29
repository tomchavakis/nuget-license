using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NugetUtility
{
    public class LibraryNameAndVersionComparer : IEqualityComparer<LibraryInfo>
    {
        public static LibraryNameAndVersionComparer Default = new LibraryNameAndVersionComparer();

        public bool Equals([AllowNull] LibraryInfo x, [AllowNull] LibraryInfo y)
        {
            return x?.PackageName == y?.PackageName
                && x?.PackageVersion == y?.PackageVersion;
        }

        public int GetHashCode([DisallowNull] LibraryInfo obj)
        {
            return obj.PackageName.GetHashCode() ^ obj.PackageVersion.GetHashCode();
        }
    }
}