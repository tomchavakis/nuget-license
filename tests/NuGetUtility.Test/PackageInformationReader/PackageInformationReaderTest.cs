using AutoFixture;
using Moq;
using NuGet.Protocol.Core.Types;
using NuGetUtility.PackageInformationReader;
using NuGetUtility.Test.Helper.AsyncEnumerableExtension;
using NuGetUtility.Test.Helper.AutoFixture;
using NuGetUtility.Test.Helper.AutoFixture.NuGet.Versioning;
using NuGetUtility.Test.Helper.NuGet.Protocol.Core.Types;
using NuGetUtility.Test.Helper.NuGet.Versioning;
using NuGetUtility.Test.Helper.ShuffelledEnumerable;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;
using NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types;
using NUnit.Framework;

namespace NuGetUtility.Test.PackageInformationReader
{
    [TestFixture]
    internal class PackageInformationReaderTest
    {
        [SetUp]
        public void SetUp()
        {
            _sourceRepositoryProvider = new Mock<IWrappedSourceRepositoryProvider>();
            _customPackageInformation = Enumerable.Empty<CustomPackageInformation>().ToList();
            _fixture = new Fixture();
            _fixture.Customizations.Add(new NuGetVersionBuilder());
            _fixture.Customizations.Add(new MockBuilder());
            _sourceRepositories = new List<Mock<IDisposableSourceRepository>>();

            _sourceRepositoryProvider.Setup(m => m.GetRepositories()).Returns(() =>
            {
                foreach (var repository in _sourceRepositories ?? Enumerable.Empty<Mock<IDisposableSourceRepository>>())
                {
                    repository.Verify(m => m.Dispose(), Times.Once);
                }

                _sourceRepositories = _fixture.CreateMany<Mock<IDisposableSourceRepository>>().ToList();
                return _sourceRepositories.Select(r => r.Object);
            });

            SetupUut();
        }

        [TearDown]
        public void TearDown()
        {
            _uut?.Dispose();
            foreach (var repository in _sourceRepositories ?? Enumerable.Empty<Mock<IDisposableSourceRepository>>())
            {
                repository.Verify(m => m.Dispose(), Times.Once);
            }

            _sourceRepositories?.Clear();
            _uut = null;
        }

        private void SetupUut()
        {
            TearDown();
            _uut = new NuGetUtility.PackageInformationReader.PackageInformationReader(_sourceRepositoryProvider!.Object,
                _customPackageInformation!);
        }

        private NuGetUtility.PackageInformationReader.PackageInformationReader? _uut;
        private Mock<IWrappedSourceRepositoryProvider>? _sourceRepositoryProvider;
        private List<CustomPackageInformation>? _customPackageInformation;
        private Fixture? _fixture;
        private List<Mock<IDisposableSourceRepository>>? _sourceRepositories;

        [Test]
        public async Task GetPackageInfo_Should_PreferProvidedCustomInformation()
        {
            _customPackageInformation = _fixture.CreateMany<CustomPackageInformation>().ToList();
            SetupUut();

            var searchedPackages = _customPackageInformation.Select(p =>
                new PackageSearchMetadataMock(new PackageIdentity(p.Id, new WrappedNuGetVersion(p.Version))) as
                    IPackageSearchMetadata);

            var searchedPackageInfo =
                await _uut!.GetPackageInfo(searchedPackages, CancellationToken.None).Synchronize();

            CollectionAssert.AreEquivalent(_customPackageInformation!,
                searchedPackageInfo.Select(s => new CustomPackageInformation(s.Identity.Id, s.Identity.Version,
                    s.LicenseMetadata.License)));
        }

        [Test]
        public async Task GetPackageInfo_Should_IterateThroughRepositoriesToGetAdditionalInformation()
        {
            var sourceRepositoriesWithPackageMetadataResource = _sourceRepositories!.Shuffle().Take(2).ToArray();
            var packageMetadataResources = sourceRepositoriesWithPackageMetadataResource.Select(r =>
            {
                var metadataResource = new Mock<IPackageMetadataResource>();
                r.Setup(m => m.GetPackageMetadataResourceAsync()).ReturnsAsync(metadataResource.Object);
                return metadataResource;
            }).ToArray();
            var searchedPackagesAsPackageInformation = _fixture.CreateMany<CustomPackageInformation>(20).ToArray();

            foreach (var package in searchedPackagesAsPackageInformation)
            {
                var metadataReturningProperInformation = packageMetadataResources.Shuffle().First();
                metadataReturningProperInformation
                    .Setup(m => m.TryGetMetadataAsync(
                        new NuGet.Packaging.Core.PackageIdentity(package.Id, package.Version),
                        It.IsAny<CancellationToken>())).ReturnsAsync(new PackageMetadataWithVersionInfo(package.Id,
                        package.Version, package.License));
            }

            var searchedPackages = searchedPackagesAsPackageInformation.Select(i =>
                new PackageSearchMetadataMock(new PackageIdentity(i.Id, new WrappedNuGetVersion(i.Version))) as
                    IPackageSearchMetadata);

            var searchedPackageInfo =
                await _uut!.GetPackageInfo(searchedPackages, CancellationToken.None).Synchronize();

            CollectionAssert.AreEquivalent(searchedPackagesAsPackageInformation,
                searchedPackageInfo.Select(s => new CustomPackageInformation(s.Identity.Id, s.Identity.Version,
                    s.LicenseMetadata.License)));
        }

        [Test]
        public async Task
            GetPackageInfo_Should_IgnoreFailingPackageMetadataResourceGetting_if_IterateThroughRepositoriesToGetAdditionalInformation()
        {
            var shuffledRepositories = _sourceRepositories!.Shuffle();
            var splitRepositories = shuffledRepositories.Select((repo, index) => (Index: index, Repo: repo))
                .GroupBy(e => e.Index % 2).ToArray();

            var sourceRepositoriesWithPackageMetadataResource = splitRepositories[0].Select(e => e.Repo).ToArray();
            var sourceRepositoriesWithFailingPackageMetadataResource =
                splitRepositories[1].Select(e => e.Repo).ToArray();
            var packageMetadataResources = sourceRepositoriesWithPackageMetadataResource.Select(r =>
            {
                var metadataResource = new Mock<IPackageMetadataResource>();
                r.Setup(m => m.GetPackageMetadataResourceAsync()).ReturnsAsync(metadataResource.Object);
                return metadataResource;
            }).ToArray();
            foreach (var repo in sourceRepositoriesWithFailingPackageMetadataResource)
            {
                repo.Setup(m => m.GetPackageMetadataResourceAsync()).Callback(() => throw new Exception());
            }

            var searchedPackagesAsPackageInformation = _fixture.CreateMany<CustomPackageInformation>(20).ToArray();

            foreach (var package in searchedPackagesAsPackageInformation)
            {
                var metadataReturningProperInformation = packageMetadataResources.Shuffle().First();
                metadataReturningProperInformation
                    .Setup(m => m.TryGetMetadataAsync(
                        new NuGet.Packaging.Core.PackageIdentity(package.Id, package.Version),
                        It.IsAny<CancellationToken>())).ReturnsAsync(new PackageMetadataWithVersionInfo(package.Id,
                        package.Version, package.License));
            }

            var searchedPackages = searchedPackagesAsPackageInformation.Select(i =>
                new PackageSearchMetadataMock(new PackageIdentity(i.Id, new WrappedNuGetVersion(i.Version))) as
                    IPackageSearchMetadata);

            var searchedPackageInfo =
                await _uut!.GetPackageInfo(searchedPackages, CancellationToken.None).Synchronize();

            CollectionAssert.AreEquivalent(searchedPackagesAsPackageInformation,
                searchedPackageInfo.Select(s => new CustomPackageInformation(s.Identity.Id, s.Identity.Version,
                    s.LicenseMetadata.License)));
        }

        [Test]
        public async Task GetPackageInfo_Should_ReturnInputForPackagesWithoutProperLicenseInformation()
        {
            var searchedPackagesAsPackageInformation = _fixture.CreateMany<CustomPackageInformation>().ToArray();
            var searchedPackages = searchedPackagesAsPackageInformation.Select(p =>
                new PackageSearchMetadataMock(new PackageIdentity(p.Id, new WrappedNuGetVersion(p.Version))) as
                    IPackageSearchMetadata).ToArray();

            var searchedPackageInfo =
                await _uut!.GetPackageInfo(searchedPackages, CancellationToken.None).Synchronize();

            CollectionAssert.AreEquivalent(searchedPackages, searchedPackageInfo);
        }
    }
}
