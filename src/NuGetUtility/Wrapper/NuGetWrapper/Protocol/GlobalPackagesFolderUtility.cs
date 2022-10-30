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
            var cachedPackage = NuGet.Protocol.GlobalPackagesFolderUtility.GetPackage(new OriginalPackageIdentity(identity.Id, new NuGetVersion(identity.Version.ToString())), _globalPackagesFolder);
            using var pkgStream = cachedPackage.PackageReader;
            var manifest = Manifest.ReadFrom(pkgStream.GetNuspec(), true);
            return new WrappedPackageMetadata(manifest.Metadata);
        }
    }
}
