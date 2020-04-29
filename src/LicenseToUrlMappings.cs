using System.Collections.Generic;

namespace NugetUtility
{
    public class LicenseToUrlMappings : Dictionary<string, string>
    {
        public LicenseToUrlMappings() { }

        public LicenseToUrlMappings(IDictionary<string, string> dictionary) : base(dictionary)
        {
        }

        public static LicenseToUrlMappings Default { get; } = new LicenseToUrlMappings
        {
            { "http://www.apache.org/licenses/LICENSE-2.0.html", "Apache2.0" },
            { "http://www.apache.org/licenses/LICENSE-2.0", "Apache2.0" },
            { "http://aws.amazon.com/apache2.0/", "Apache2.0" },
            { "https://opensource.org/licenses/MS-PL", "MS-PL" }
        };
    }
}
