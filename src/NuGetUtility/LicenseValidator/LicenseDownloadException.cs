using NuGet.Packaging.Core;

namespace NuGetUtility.LicenseValidator
{
    public class LicenseDownloadException : Exception
    {
        public LicenseDownloadException(Exception inner, string context, PackageIdentity packageInfo)
            :
            base(
                $"Failed to download license for package {packageInfo.Id} ({packageInfo.Version}).\nContext: {context}",
                inner) { }
    }
}
