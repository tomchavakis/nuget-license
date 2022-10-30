using NuGetUtility.Wrapper.NuGetWrapper.Packaging;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;
using NuGetUtility.Wrapper.NuGetWrapper.Protocol;
using NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types;
using System.Runtime.CompilerServices;

namespace NuGetUtility.PackageInformationReader
{
    public class PackageInformationReader
    {
        private readonly IGlobalPackagesFolderUtility _globalPackagesFolderUtility;
        private readonly IEnumerable<CustomPackageInformation> _customPackageInformation;
        private readonly ISourceRepository[] _repositories;

        public PackageInformationReader(IWrappedSourceRepositoryProvider sourceRepositoryProvider,
            IGlobalPackagesFolderUtility globalPackagesFolderUtility,
            IEnumerable<CustomPackageInformation> customPackageInformation)
        {
            _globalPackagesFolderUtility = globalPackagesFolderUtility;
            _customPackageInformation = customPackageInformation;
            _repositories = sourceRepositoryProvider.GetRepositories();
        }

        public async IAsyncEnumerable<IPackageMetadata> GetPackageInfo(
            IEnumerable<PackageIdentity> packages,
            [EnumeratorCancellation] CancellationToken cancellation)
        {
            foreach (var package in packages)
            {
                var result = TryGetPackageInfoFromCustomInformation(package);
                if (result.Success)
                {
                    yield return result.Metadata!;
                    continue;
                }
                result = TryGetPackageInformationFromGlobalPackageFolder(package);
                if (result.Success)
                {
                    yield return result.Metadata!;
                    continue;
                }
                result = await TryGetPackageInformationFromRepositories(_repositories, package, cancellation);
                if (result.Success)
                {
                    yield return result.Metadata!;
                    continue;
                }
                // simply return input - validation will fail later, as the required fields are missing
                yield return new PackageMetadata(package);
            }
        }
        private PackageSearchResult TryGetPackageInformationFromGlobalPackageFolder(PackageIdentity package)
        {
            var metadata = _globalPackagesFolderUtility.GetPackage(package);
            if (metadata != null)
            {
                return new PackageSearchResult(metadata);
            }
            return new PackageSearchResult();
        }

        private async Task<PackageSearchResult> TryGetPackageInformationFromRepositories(
            ISourceRepository[] cachedRepositories,
            PackageIdentity package,
            CancellationToken cancellation)
        {
            foreach (var repository in cachedRepositories)
            {
                var resource = await TryGetPackageMetadataResource(repository);
                if (resource == null)
                {
                    continue;
                }

                var updatedPackageMetadata = await resource.TryGetMetadataAsync(package, cancellation);

                if (updatedPackageMetadata != null)
                {
                    return new PackageSearchResult(updatedPackageMetadata);
                }
            }

            return new PackageSearchResult();
        }

        private PackageSearchResult TryGetPackageInfoFromCustomInformation(PackageIdentity package)
        {
            var resolvedCustomInformation = _customPackageInformation.FirstOrDefault(info =>
                info.Id.Equals(package.Id) && info.Version.Equals(package.Version));
            if (resolvedCustomInformation == default)
            {
                return new PackageSearchResult();
            }

            return new PackageSearchResult(new PackageMetadata(package, resolvedCustomInformation.License));
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
            public IPackageMetadata? Metadata { get; }

            public PackageSearchResult(IPackageMetadata metadata)
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
