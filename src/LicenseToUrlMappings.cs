using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NugetUtility
{
    /// <summary>
    /// More information at https://spdx.org/licenses/
    /// </summary>
    public class LicenseToUrlMappings : Dictionary<string, string>
    {
        private static readonly string Apache2_0 = "Apache-2.0";
        private static readonly string GPL2_0 = "GPL-2.0";
        private static readonly string MIT = "MIT";
        private static readonly string MS_PL = "MS-PL";
        private static readonly string MS_EULA = "MS-EULA";
        private static readonly string MS_EULA_Non_Redistributable = "MS-EULA-Non-Redistributable";

        public LicenseToUrlMappings() : base(ProtocolIgnorantOrdinalIgnoreCase.Default) { }

        public LicenseToUrlMappings(IDictionary<string, string> dictionary) : base(dictionary, ProtocolIgnorantOrdinalIgnoreCase.Default)
        {
        }

        private class ProtocolIgnorantOrdinalIgnoreCase : IEqualityComparer<string>
        {
            public static ProtocolIgnorantOrdinalIgnoreCase Default = new ProtocolIgnorantOrdinalIgnoreCase();

            public bool Equals([AllowNull] string x, [AllowNull] string y)
            {
                if (x is null || y is null) { return false; }

                return string.Compare(GetProtocolLessUrl(x), GetProtocolLessUrl(y), StringComparison.OrdinalIgnoreCase) == 0;
            }

            public int GetHashCode([DisallowNull] string obj) => GetProtocolLessUrl(obj).ToLower().GetHashCode();

            private static string GetProtocolLessUrl(string url)
            {
                if (string.IsNullOrEmpty(url)) return string.Empty;
                if (!url.Contains(":")) return string.Empty;
                return url[url.IndexOf(':')..];
            }
        }

        public static LicenseToUrlMappings Default { get; } = new LicenseToUrlMappings
        {
            { "http://www.apache.org/licenses/LICENSE-2.0.html", Apache2_0 },
            { "http://www.apache.org/licenses/LICENSE-2.0", Apache2_0 },
            { "http://opensource.org/licenses/Apache-2.0", Apache2_0 },
            { "http://aws.amazon.com/apache2.0/", Apache2_0 },
            { "http://logging.apache.org/log4net/license.html", Apache2_0 },
            { "https://github.com/owin-contrib/owin-hosting/blob/master/LICENSE.txt", Apache2_0 },
            { "https://raw.githubusercontent.com/aspnet/Home/2.0.0/LICENSE.txt", Apache2_0 },

            { "https://github.com/Microsoft/Microsoft.IO.RecyclableMemoryStream/blob/master/LICENSE", MIT },
            { "https://github.com/AutoMapper/AutoMapper/blob/master/LICENSE.txt", MIT },
            { "https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE", MIT },
            { "https://raw.githubusercontent.com/hey-red/markdownsharp/master/LICENSE", MIT },
            { "https://raw.github.com/JamesNK/Newtonsoft.Json/master/LICENSE.md", MIT },
            { "https://licenses.nuget.org/MIT", MIT },
            { "http://opensource.org/licenses/MIT", MIT },
            { "http://www.opensource.org/licenses/mit-license.php", MIT },
            { "http://max.mit-license.org/", MIT },
            { "https://github.com/dotnet/corefx/blob/master/LICENSE.TXT", MIT},

            { "http://www.gnu.org/licenses/old-licenses/gpl-2.0.html", GPL2_0 },

            { "http://opensource.org/licenses/MS-PL", MS_PL },
            { "http://www.opensource.org/licenses/ms-pl", MS_PL },

            { "https://www.microsoft.com/web/webpi/eula/aspnetmvc3update-eula.htm", MS_EULA },
            { "http://go.microsoft.com/fwlink/?LinkID=214339", MS_EULA },
            { "https://www.microsoft.com/web/webpi/eula/net_library_eula_enu.htm", MS_EULA },
            { "http://go.microsoft.com/fwlink/?LinkId=329770", MS_EULA },
            { "http://go.microsoft.com/fwlink/?LinkId=529443", MS_EULA },

            {"https://www.microsoft.com/web/webpi/eula/dotnet_library_license_non_redistributable.htm", MS_EULA_Non_Redistributable  },
            {"http://go.microsoft.com/fwlink/?LinkId=529444", MS_EULA_Non_Redistributable  }
        };
    }
}
