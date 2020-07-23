using System.Collections.Generic;

namespace NugetUtility
{
    public class PackageList : Dictionary<string, Package>
    {
        public PackageList(int capacity = 50) : base(capacity) { }
    }
}