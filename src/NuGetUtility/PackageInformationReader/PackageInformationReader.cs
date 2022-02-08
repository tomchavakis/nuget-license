using NuGet.Protocol.Core.Types;
using NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types;
using System.Runtime.CompilerServices;

namespace NuGetUtility.PackageInformationReader
{
    public class PackageInformationReader : IDisposable
    {
        private readonly IEnumerable<CustomPackageInformation> _customPackageInformation;
        private readonly IDisposableSourceRepository[] _sourceRepositories;

        public PackageInformationReader(IWrappedSourceRepositoryProvider sourceRepositoryProvider,
            IEnumerable<CustomPackageInformation> customPackageInformation)
        {
            _customPackageInformation = customPackageInformation;
            _sourceRepositories = sourceRepositoryProvider.GetRepositories().ToArray();
        }

        public void Dispose()
        {
            foreach (var repo in _sourceRepositories)
            {
                repo.Dispose();
            }
        }

        public async IAsyncEnumerable<IPackageSearchMetadata> GetPackageInfo(
            IEnumerable<IPackageSearchMetadata> packageMetadata,
            [EnumeratorCancellation] CancellationToken cancellation)
        {
            foreach (var package in packageMetadata)
            {
                if (TryGetPackageInfoFromCustomInformation(package, out var info))
                {
                    yield return info!;
                }
                else
                {
                    yield return await TryGetPackageInformationFromRepositoriesOrReturnInput(_sourceRepositories,
                        package, cancellation);
                }
            }
        }

        private async Task<IPackageSearchMetadata> TryGetPackageInformationFromRepositoriesOrReturnInput(
            IDisposableSourceRepository[] cachedRepositories, IPackageSearchMetadata package,
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
                    return updatedPackageMetadata;
                }
            }

            // simply return input - validation will fail later, as the required fields are missing
            return package;
        }

        private bool TryGetPackageInfoFromCustomInformation(IPackageSearchMetadata package,
            out IPackageSearchMetadata? resolved)
        {
            resolved = default;
            var resolvedCustomInformation = _customPackageInformation.FirstOrDefault(info =>
                info.Id.Equals(package.Identity.Id) && info.Version.Equals(package.Identity.Version));
            if (resolvedCustomInformation == default)
            {
                return false;
            }

            resolved = new PackageMetadataWithLicenseInformation(package, resolvedCustomInformation.License);
            return true;
        }

        private static async Task<IPackageMetadataResource?> TryGetPackageMetadataResource(
            IDisposableSourceRepository repository)
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
    }
}
