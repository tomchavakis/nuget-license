using System;
using System.Collections.Generic;
using System.Text;

namespace NugetUtility
{
    public class Package
    {
        public List<PackageData> data { get; set; }
    }

    public class PackageData
    {
        public string version { get; set; }
        public string projectUrl { get; set; }
        public string licenseUrl { get; set; }
    }
}
