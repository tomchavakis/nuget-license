using NuGet.Protocol.Core.Types;
using NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types;
using System.Runtime.CompilerServices;

namespace NuGetUtility.PackageInformationReader
{
    public class PackageInformationReader
    {
        private readonly IEnumerable<CustomPackageInformation> _customPackageInformation;
        private readonly ISourceRepository[] _localRepositories;
        private readonly ISourceRepository[] _remoteRepositories;

        public PackageInformationReader(IWrappedSourceRepositoryProvider sourceRepositoryProvider,
            IEnumerable<CustomPackageInformation> customPackageInformation)
        {
            _customPackageInformation = customPackageInformation;
            _localRepositories = sourceRepositoryProvider.GetLocalRepositories();
            _remoteRepositories = sourceRepositoryProvider.GetRemoteRepositories();
        }

        public async IAsyncEnumerable<IPackageSearchMetadata> GetPackageInfo(
            IEnumerable<IPackageSearchMetadata> packageMetadata,
            [EnumeratorCancellation] CancellationToken cancellation)
        {
            foreach (var package in packageMetadata)
            {
                var result = TryGetPackageInfoFromCustomInformation(package);
                if (result.Success)
                {
                    yield return result.Metadata!;
                    continue;
                }
                result = await TryGetPackageInformationFromRepositories(_localRepositories, package, cancellation);
                if (result.Success)
                {
                    yield return result.Metadata!;
                    continue;
                }
                result = await TryGetPackageInformationFromRepositories(_remoteRepositories, package, cancellation);
                if (result.Success)
                {
                    yield return result.Metadata!;
                    continue;
                }
                // simply return input - validation will fail later, as the required fields are missing
                yield return package;
            }
        }

        private async Task<PackageSearchResult> TryGetPackageInformationFromRepositories(
            ISourceRepository[] cachedRepositories,
            IPackageSearchMetadata package,
            CancellationToken cancellation)
        {
            foreach (var repository in cachedRepositories)
            {
                var resource = await TryGetPackageMetadataResource(repository);
                if (resource == null)
                {
                    continue;
                }

                var updatedPackageMetadata = await resource.TryGetMetadataAsync(package.Identity, cancellation);

                if (updatedPackageMetadata != null)
                {
                    return new PackageSearchResult(updatedPackageMetadata);
                }
            }

            return new PackageSearchResult();
        }

        private PackageSearchResult TryGetPackageInfoFromCustomInformation(IPackageSearchMetadata package)
        {
            var resolvedCustomInformation = _customPackageInformation.FirstOrDefault(info =>
                info.Id.Equals(package.Identity.Id) && info.Version.Equals(package.Identity.Version));
            if (resolvedCustomInformation == default)
            {
                return new PackageSearchResult();
            }

            return new PackageSearchResult(new PackageMetadataWithLicenseInformation(package, resolvedCustomInformation.License));
        }

        private static async Task<IPackageMetadataResource?> TryGetPackageMetadataResource(ISourceRepository repository)
        {
            try
            {
                return await repository.GetPackageMetadataResourceAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private record PackageSearchResult
        {
            public bool Success { get; }
            public IPackageSearchMetadata? Metadata { get; }

            public PackageSearchResult(IPackageSearchMetadata metadata)
            {
                Success = true;
                Metadata = metadata;
            }

            public PackageSearchResult()
            {
                Success = false;
            }
        }
    }
}
