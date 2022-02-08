namespace NuGetUtility.LicenseValidator
{
    public static class UrlToLicenseMapping
    {
        private const string Apache = "Apache";
        private const string Gpl = "GPL";
        private const string Mit = "MIT";
        private const string MsPl = "MS-PL";
        private const string MsEula = "MS-EULA";
        private const string MsEulaNonRedistributable = "MS-EULA-Non-Redistributable";

        public static Dictionary<Uri, LicenseId> Default = new Dictionary<Uri, LicenseId>
        {
            {
                new Uri("http://www.apache.org/licenses/LICENSE-2.0.html"), new LicenseId(Apache, new Version(2, 0))
            },
            { new Uri("http://www.apache.org/licenses/LICENSE-2.0"), new LicenseId(Apache, new Version(2, 0)) },
            { new Uri("http://opensource.org/licenses/Apache-2.0"), new LicenseId(Apache, new Version(2, 0)) },
            { new Uri("http://aws.amazon.com/apache2.0/"), new LicenseId(Apache, new Version(2, 0)) },
            { new Uri("http://logging.apache.org/log4net/license.html"), new LicenseId(Apache, new Version(2, 0)) },
            {
                new Uri("https://github.com/owin-contrib/owin-hosting/blob/master/LICENSE.txt"),
                new LicenseId(Apache, new Version(2, 0))
            },
            {
                new Uri("https://raw.githubusercontent.com/aspnet/Home/2.0.0/LICENSE.txt"),
                new LicenseId(Apache, new Version(2, 0))
            },
            {
                new Uri("https://github.com/Microsoft/Microsoft.IO.RecyclableMemoryStream/blob/master/LICENSE"),
                new LicenseId(Mit)
            },
            { new Uri("https://github.com/AutoMapper/AutoMapper/blob/master/LICENSE.txt"), new LicenseId(Mit) },
            { new Uri("https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE"), new LicenseId(Mit) },
            { new Uri("https://raw.githubusercontent.com/hey-red/markdownsharp/master/LICENSE"), new LicenseId(Mit) },
            { new Uri("https://raw.github.com/JamesNK/Newtonsoft.Json/master/LICENSE.md"), new LicenseId(Mit) },
            { new Uri("https://licenses.nuget.org/MIT"), new LicenseId(Mit) },
            { new Uri("http://opensource.org/licenses/MIT"), new LicenseId(Mit) },
            { new Uri("http://www.opensource.org/licenses/mit-license.php"), new LicenseId(Mit) },
            { new Uri("http://max.mit-license.org/"), new LicenseId(Mit) },
            { new Uri("https://github.com/dotnet/corefx/blob/master/LICENSE.TXT"), new LicenseId(Mit) },
            { new Uri("http://www.gnu.org/licenses/old-licenses/gpl-2.0.html"), new LicenseId(Gpl, new Version(2, 0)) },
            { new Uri("http://opensource.org/licenses/MS-PL"), new LicenseId(MsPl) },
            { new Uri("http://www.opensource.org/licenses/ms-pl"), new LicenseId(MsPl) },
            { new Uri("https://www.microsoft.com/web/webpi/eula/aspnetmvc3update-eula.htm"), new LicenseId(MsEula) },
            { new Uri("http://go.microsoft.com/fwlink/?LinkID=214339"), new LicenseId(MsEula) },
            { new Uri("https://www.microsoft.com/web/webpi/eula/net_library_eula_enu.htm"), new LicenseId(MsEula) },
            { new Uri("http://go.microsoft.com/fwlink/?LinkId=329770"), new LicenseId(MsEula) },
            { new Uri("http://go.microsoft.com/fwlink/?LinkId=529443"), new LicenseId(MsEula) },
            {
                new Uri("https://www.microsoft.com/web/webpi/eula/dotnet_library_license_non_redistributable.htm"),
                new LicenseId(MsEulaNonRedistributable)
            },
            { new Uri("http://go.microsoft.com/fwlink/?LinkId=529444"), new LicenseId(MsEulaNonRedistributable) }
        };
    }
}
