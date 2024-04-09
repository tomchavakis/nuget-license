// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using System.Collections.Immutable;

namespace NuGetUtility.LicenseValidator
{
    public static class UrlToLicenseMapping
    {
        private const string Apache20 = "Apache-2.0";
        private const string Gpl20 = "GPL-2.0";
        private const string Mit = "MIT";
        private const string MsPl = "MS-PL";
        private const string MsEula = "MS-EULA";
        private const string MsEulaNonRedistributable = "MS-EULA-Non-Redistributable";

        public static IImmutableDictionary<Uri, string> Default { get; } = ImmutableDictionary.CreateRange(
            new[]
            {
                new KeyValuePair<Uri, string>(new Uri("http://www.apache.org/licenses/LICENSE-2.0.html"), Apache20),
                new KeyValuePair<Uri, string>(new Uri("http://www.apache.org/licenses/LICENSE-2.0"), Apache20),
                new KeyValuePair<Uri, string>(new Uri("http://aws.amazon.com/apache2.0/"), Apache20),
                new KeyValuePair<Uri, string>(new Uri("https://github.com/owin-contrib/owin-hosting/blob/master/LICENSE.txt"), Apache20),
                new KeyValuePair<Uri, string>(new Uri("https://raw.githubusercontent.com/aspnet/Home/2.0.0/LICENSE.txt"), Apache20),
                new KeyValuePair<Uri, string>(
                    new Uri("https://github.com/Microsoft/Microsoft.IO.RecyclableMemoryStream/blob/master/LICENSE"),
                    Mit
                ),
                new KeyValuePair<Uri, string>(new Uri("https://github.com/AutoMapper/AutoMapper/blob/master/LICENSE.txt"), Mit),
                new KeyValuePair<Uri, string>(new Uri("https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE"), Mit),
                new KeyValuePair<Uri, string>(new Uri("https://raw.githubusercontent.com/hey-red/markdownsharp/master/LICENSE"), Mit),
                new KeyValuePair<Uri, string>(new Uri("https://raw.github.com/JamesNK/Newtonsoft.Json/master/LICENSE.md"), Mit),
                new KeyValuePair<Uri, string>(new Uri("https://licenses.nuget.org/MIT"), Mit),
                new KeyValuePair<Uri, string>(new Uri("http://max.mit-license.org/"), Mit),
                new KeyValuePair<Uri, string>(new Uri("https://github.com/dotnet/corefx/blob/master/LICENSE.TXT"), Mit),
                new KeyValuePair<Uri, string>(new Uri("https://go.microsoft.com/fwlink/?linkid=868514"), Mit),
                new KeyValuePair<Uri, string>(new Uri("http://go.microsoft.com/fwlink/?linkid=833178"), Mit),
                new KeyValuePair<Uri, string>(new Uri("http://www.gnu.org/licenses/old-licenses/gpl-2.0.html"), Gpl20),
                new KeyValuePair<Uri, string>(new Uri("https://raw.githubusercontent.com/AArnott/Validation/8377954d86/LICENSE.txt"), MsPl),
                new KeyValuePair<Uri, string>(new Uri("https://www.microsoft.com/web/webpi/eula/aspnetmvc3update-eula.htm"), MsEula),
                new KeyValuePair<Uri, string>(new Uri("http://go.microsoft.com/fwlink/?LinkID=214339"), MsEula),
                new KeyValuePair<Uri, string>(new Uri("https://www.microsoft.com/web/webpi/eula/net_library_eula_enu.htm"), MsEula),
                new KeyValuePair<Uri, string>(new Uri("http://go.microsoft.com/fwlink/?LinkId=329770"), MsEula),
                new KeyValuePair<Uri, string>(new Uri("http://go.microsoft.com/fwlink/?LinkId=529443"), MsEula),
                new KeyValuePair<Uri, string>(
                    new Uri("https://www.microsoft.com/web/webpi/eula/dotnet_library_license_non_redistributable.htm"),
                    MsEulaNonRedistributable
                ),
                new KeyValuePair<Uri, string>(new Uri("http://go.microsoft.com/fwlink/?LinkId=529444"), MsEulaNonRedistributable),
                new KeyValuePair<Uri, string>(new Uri(" http://opensource.org/licenses/mit-license.php"), Mit),
                new KeyValuePair<Uri, string>(new Uri("https://raw.githubusercontent.com/bchavez/Bogus/master/LICENSE"), Mit),
                new KeyValuePair<Uri, string>(new Uri("https://github.com/Microsoft/dotnet/blob/master/LICENSE"), Mit)
            }
        );
    }
}
