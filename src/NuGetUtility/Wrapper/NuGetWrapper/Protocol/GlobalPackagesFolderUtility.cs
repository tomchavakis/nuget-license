using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Versioning;
using IWrappedPackageMetadata = NuGetUtility.Wrapper.NuGetWrapper.Packaging.IPackageMetadata;
using OriginalPackageIdentity = NuGet.Packaging.Core.PackageIdentity;
using PackageIdentity = NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core.PackageIdentity;

namespace NuGetUtility.Wrapper.NuGetWrapper.Protocol
{
    internal class GlobalPackagesFolderUtility : IGlobalPackagesFolderUtility
    {
        private readonly string _globalPackagesFolder;

        public GlobalPackagesFolderUtility(ISettings settings)
        {
            _globalPackagesFolder = SettingsUtility.GetGlobalPackagesFolder(settings);
        }

        public IWrappedPackageMetadata? GetPackage(PackageIdentity identity)
        {
            NuGet.Protocol.Core.Types.DownloadResourceResult cachedPackage = NuGet.Protocol.GlobalPackagesFolderUtility.GetPackage(new OriginalPackageIdentity(identity.Id, new NuGetVersion(identity.Version.ToString())), _globalPackagesFolder);
            if (cachedPackage == null)
            {
                return null;
            }

            using PackageReaderBase pkgStream = cachedPackage.PackageReader;
            var manifest = Manifest.ReadFrom(pkgStream.GetNuspec(), true);

            if (manifest.Metadata.Version.ToString() != identity.Version.ToString())
            {
                return null;
            }

            return new WrappedPackageMetadata(manifest.Metadata);
        }
    }
}
