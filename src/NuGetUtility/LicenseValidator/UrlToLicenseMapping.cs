using System.Collections.Immutable;

namespace NuGetUtility.LicenseValidator
{
    public static class UrlToLicenseMapping
    {
        private const string Apache20 = "Apache-2.0";
        private const string Bsd3Clause = "BSD-3-Clause";
        private const string Gpl20 = "GPL-2.0";
        private const string Mit = "MIT";
        private const string MsPl = "MS-PL";
        private const string MsEula = "MS-EULA";
        private const string MsEulaNonRedistributable = "MS-EULA-Non-Redistributable";

        public static IImmutableDictionary<Uri, string> Default { get; } = ImmutableDictionary.CreateRange(
            new[]
            {
                KeyValuePair.Create( new Uri("http://www.apache.org/licenses/LICENSE-2.0.html"), Apache20 ),
                KeyValuePair.Create( new Uri("http://www.apache.org/licenses/LICENSE-2.0"), Apache20 ),
                KeyValuePair.Create( new Uri("http://opensource.org/licenses/Apache-2.0"), Apache20 ),
                KeyValuePair.Create( new Uri("http://aws.amazon.com/apache2.0/"), Apache20 ),
                KeyValuePair.Create( new Uri("http://logging.apache.org/log4net/license.html"), Apache20 ),
                KeyValuePair.Create( new Uri("https://github.com/owin-contrib/owin-hosting/blob/master/LICENSE.txt"), Apache20 ),
                KeyValuePair.Create( new Uri("https://raw.githubusercontent.com/aspnet/Home/2.0.0/LICENSE.txt"), Apache20 ),
                KeyValuePair.Create( new Uri("http://opensource.org/licenses/BSD-3-Clause"), Bsd3Clause ),
                KeyValuePair.Create(
                    new Uri("https://github.com/Microsoft/Microsoft.IO.RecyclableMemoryStream/blob/master/LICENSE"), Mit
                ),
                KeyValuePair.Create( new Uri("https://github.com/AutoMapper/AutoMapper/blob/master/LICENSE.txt"), Mit ),
                KeyValuePair.Create( new Uri("https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE"), Mit ),
                KeyValuePair.Create( new Uri("https://raw.githubusercontent.com/hey-red/markdownsharp/master/LICENSE"), Mit ),
                KeyValuePair.Create( new Uri("https://raw.github.com/JamesNK/Newtonsoft.Json/master/LICENSE.md"), Mit ),
                KeyValuePair.Create( new Uri("https://licenses.nuget.org/MIT"), Mit ),
                KeyValuePair.Create( new Uri("http://opensource.org/licenses/MIT"), Mit ),
                KeyValuePair.Create( new Uri("http://opensource.org/licenses/mit-license.php"), Mit ),
                KeyValuePair.Create( new Uri("http://www.opensource.org/licenses/mit-license.php"), Mit ),
                KeyValuePair.Create( new Uri("http://max.mit-license.org/"), Mit ),
                KeyValuePair.Create( new Uri("https://github.com/dotnet/corefx/blob/master/LICENSE.TXT"), Mit ),
                KeyValuePair.Create( new Uri("https://go.microsoft.com/fwlink/?linkid=868514"), Mit ),
                KeyValuePair.Create( new Uri("http://go.microsoft.com/fwlink/?linkid=833178"), Mit ),
                KeyValuePair.Create( new Uri("http://sourceforge.net/directory/license:mit/"), Mit ),
                KeyValuePair.Create( new Uri("http://www.gnu.org/licenses/old-licenses/gpl-2.0.html"), Gpl20 ),
                KeyValuePair.Create( new Uri("http://opensource.org/licenses/MS-PL"), MsPl ),
                KeyValuePair.Create( new Uri("https://opensource.org/licenses/MS-PL"), MsPl ),
                KeyValuePair.Create( new Uri("http://www.opensource.org/licenses/ms-pl"), MsPl ),
                KeyValuePair.Create( new Uri("https://raw.githubusercontent.com/AArnott/Validation/8377954d86/LICENSE.txt"), MsPl ),
                KeyValuePair.Create( new Uri("https://www.microsoft.com/web/webpi/eula/aspnetmvc3update-eula.htm"), MsEula ),
                KeyValuePair.Create( new Uri("http://go.microsoft.com/fwlink/?LinkID=214339"), MsEula ),
                KeyValuePair.Create( new Uri("https://www.microsoft.com/web/webpi/eula/net_library_eula_enu.htm"), MsEula ),
                KeyValuePair.Create( new Uri("http://go.microsoft.com/fwlink/?LinkId=329770"), MsEula ),
                KeyValuePair.Create( new Uri("http://go.microsoft.com/fwlink/?LinkId=529443"), MsEula ),
                KeyValuePair.Create(
                    new Uri("https://www.microsoft.com/web/webpi/eula/dotnet_library_license_non_redistributable.htm"),
                    MsEulaNonRedistributable
                ),
                KeyValuePair.Create( new Uri("http://go.microsoft.com/fwlink/?LinkId=529444"), MsEulaNonRedistributable )
            }
        );
    }
}
