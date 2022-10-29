using AutoFixture;
using Moq;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGetUtility.PackageInformationReader;
using NuGetUtility.Test.Helper.AsyncEnumerableExtension;
using NuGetUtility.Test.Helper.AutoFixture;
using NuGetUtility.Test.Helper.AutoFixture.NuGet.Versioning;
using NuGetUtility.Test.Helper.NuGet.Protocol.Core.Types;
using NuGetUtility.Test.Helper.ShuffelledEnumerable;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;
using NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types;
using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

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
            _localRepositories = Array.Empty<Mock<ISourceRepository>>();
            _remoteRepositories = Array.Empty<Mock<ISourceRepository>>();

            _sourceRepositoryProvider.Setup(m => m.GetLocalRepositories())
                .Returns((Delegate)(() =>
                {
                    Assert.AreEqual(Array.Empty<Mock<ISourceRepository>>(), _localRepositories);
                    _localRepositories = _fixture.CreateMany<Mock<ISourceRepository>>().ToArray<Mock<ISourceRepository>>();
                    return _localRepositories.Select<Mock<ISourceRepository>, ISourceRepository>(r => r.Object).ToArray();
                }));

            _sourceRepositoryProvider.Setup(m => m.GetRemoteRepositories())
                .Returns((Delegate)(() =>
                {
                    Assert.AreEqual(Array.Empty<Mock<ISourceRepository>>(), _remoteRepositories);
                    _remoteRepositories = _fixture.CreateMany<Mock<ISourceRepository>>().ToArray<Mock<ISourceRepository>>();
                    return _remoteRepositories.Select<Mock<ISourceRepository>, ISourceRepository>(r => r.Object).ToArray();
                }));

            SetupUut();
        }

        [TearDown]
        public void TearDown()
        {
            _localRepositories = Array.Empty<Mock<ISourceRepository>>();
            _remoteRepositories = Array.Empty<Mock<ISourceRepository>>();
            _uut = null!;
        }

        private void SetupUut()
        {
            TearDown();
            _uut = new NuGetUtility.PackageInformationReader.PackageInformationReader(_sourceRepositoryProvider.Object,
                _customPackageInformation);
        }

        private NuGetUtility.PackageInformationReader.PackageInformationReader _uut = null!;
        private Mock<IWrappedSourceRepositoryProvider> _sourceRepositoryProvider = null!;
        private List<CustomPackageInformation> _customPackageInformation = null!;
        private Fixture _fixture = null!;
        private Mock<ISourceRepository>[] _localRepositories = null!;
        private Mock<ISourceRepository>[] _remoteRepositories = null!;

        [Test]
        public async Task GetPackageInfo_Should_PreferProvidedCustomInformation()
        {
            _customPackageInformation = _fixture.CreateMany<CustomPackageInformation>().ToList();
            SetupUut();

            var searchedPackages = _customPackageInformation.Select(p =>
                new PackageSearchMetadataMock(new PackageIdentity(p.Id, CreateMockedVersion(p.Version))) as
                    IPackageSearchMetadata);

            var searchedPackageInfo =
                await _uut.GetPackageInfo(searchedPackages, CancellationToken.None).Synchronize();

            CollectionAssert.AreEquivalent(_customPackageInformation,
                searchedPackageInfo.Select(s => new CustomPackageInformation(s.Identity.Id,
                    s.Identity.Version,
                    s.LicenseMetadata.License)));
        }

        [Test]
        public async Task GetPackageInfo_Should_IterateThroughLocalRepositoriesToGetAdditionalInformation()
        {
            var sourceRepositoriesWithPackageMetadataResource = _localRepositories.Shuffle().Take(2).ToArray();
            var packageMetadataResources = sourceRepositoriesWithPackageMetadataResource.Select(r =>
                {
                    var metadataResource = new Mock<IPackageMetadataResource>();
                    r.Setup(m => m.GetPackageMetadataResourceAsync()).ReturnsAsync(metadataResource.Object);
                    return metadataResource;
                })
                .ToArray();
            var searchedPackagesAsPackageInformation = _fixture.CreateMany<CustomPackageInformation>(20).ToArray();

            foreach (var package in searchedPackagesAsPackageInformation)
            {
                var metadataReturningProperInformation = packageMetadataResources.Shuffle().First();
                metadataReturningProperInformation
                    .Setup(m => m.TryGetMetadataAsync(
                        new NuGet.Packaging.Core.PackageIdentity(package.Id, package.Version),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new PackageMetadataWithVersionInfo(package.Id,
                        package.Version,
                        package.License));
            }

            var searchedPackages = searchedPackagesAsPackageInformation.Select(i =>
                new PackageSearchMetadataMock(new PackageIdentity(i.Id, CreateMockedVersion(i.Version))) as
                    IPackageSearchMetadata);

            var searchedPackageInfo =
                await _uut.GetPackageInfo(searchedPackages, CancellationToken.None).Synchronize();

            CollectionAssert.AreEquivalent(searchedPackagesAsPackageInformation,
                searchedPackageInfo.Select(s => new CustomPackageInformation(s.Identity.Id,
                    s.Identity.Version,
                    s.LicenseMetadata.License)));
        }

        [Test]
        public async Task GetPackageInfo_Should_IterateThroughRemoteRepositoriesToGetAdditionalInformation_If_LocalRepositoriesFail()
        {
            var localRepositoriesMetadataResources = _localRepositories.Select(r =>
            {
                var metadataResource = new Mock<IPackageMetadataResource>();
                r.Setup(m => m.GetPackageMetadataResourceAsync()).ReturnsAsync(metadataResource.Object);
                return metadataResource;
            }).ToArray();
            var sourceRepositoriesWithPackageMetadataResource = _remoteRepositories.Shuffle().Take(2).ToArray();
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
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new PackageMetadataWithVersionInfo(package.Id,
                        package.Version,
                        package.License));
            }

            var searchedPackages = searchedPackagesAsPackageInformation.Select(i =>
                new PackageSearchMetadataMock(new PackageIdentity(i.Id, CreateMockedVersion(i.Version))) as
                    IPackageSearchMetadata);

            var searchedPackageInfo =
                await _uut.GetPackageInfo(searchedPackages, CancellationToken.None).Synchronize();

            CollectionAssert.AreEquivalent(searchedPackagesAsPackageInformation,
                searchedPackageInfo.Select(s => new CustomPackageInformation(s.Identity.Id,
                    s.Identity.Version,
                    s.LicenseMetadata.License)));

            foreach(var localRepoMetadataResource in localRepositoriesMetadataResources)
            {
                foreach (var package in searchedPackages) {
                    localRepoMetadataResource.Verify(m => m.TryGetMetadataAsync(package.Identity, It.IsAny<CancellationToken>()), Times.Once);
                }
            }
        }

        [Test]
        public async Task
            GetPackageInfo_Should_IgnoreFailingPackageMetadataResourceGetting_If_IterateThroughRepositoriesToGetAdditionalInformation()
        {
            var shuffledRepositories = _localRepositories.Shuffle();
            var splitRepositories = shuffledRepositories.Select((repo, index) => (Index: index, Repo: repo))
                .GroupBy(e => e.Index % 2)
                .ToArray();

            var sourceRepositoriesWithPackageMetadataResource = splitRepositories[0].Select(e => e.Repo).ToArray();
            var sourceRepositoriesWithFailingPackageMetadataResource =
                splitRepositories[1].Select(e => e.Repo).ToArray();
            var packageMetadataResources = sourceRepositoriesWithPackageMetadataResource.Select(r =>
                {
                    var metadataResource = new Mock<IPackageMetadataResource>();
                    r.Setup(m => m.GetPackageMetadataResourceAsync()).ReturnsAsync(metadataResource.Object);
                    return metadataResource;
                })
                .ToArray();
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
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new PackageMetadataWithVersionInfo(package.Id,
                        package.Version,
                        package.License));
            }

            var searchedPackages = searchedPackagesAsPackageInformation.Select(i =>
                new PackageSearchMetadataMock(new PackageIdentity(i.Id, CreateMockedVersion(i.Version))) as
                    IPackageSearchMetadata);

            var searchedPackageInfo =
                await _uut.GetPackageInfo(searchedPackages, CancellationToken.None).Synchronize();

            CollectionAssert.AreEquivalent(searchedPackagesAsPackageInformation,
                searchedPackageInfo.Select(s => new CustomPackageInformation(s.Identity.Id,
                    s.Identity.Version,
                    s.LicenseMetadata.License)));
        }

        [Test]
        public async Task GetPackageInfo_Should_ReturnInputForPackagesWithoutProperLicenseInformation()
        {
            var searchedPackagesAsPackageInformation = _fixture.CreateMany<CustomPackageInformation>().ToArray();
            var searchedPackages = searchedPackagesAsPackageInformation.Select(p =>
                    new PackageSearchMetadataMock(new PackageIdentity(p.Id, CreateMockedVersion(p.Version))) as
                        IPackageSearchMetadata)
                .ToArray();

            var searchedPackageInfo =
                await _uut.GetPackageInfo(searchedPackages, CancellationToken.None).Synchronize();

            CollectionAssert.AreEquivalent(searchedPackages, searchedPackageInfo);
        }

        private INuGetVersion CreateMockedVersion(NuGetVersion innerVersion)
        {
            var mock = new Mock<INuGetVersion>();
            mock.Setup(m => m.ToString()).Returns(innerVersion.ToString());

            return mock.Object;
        }
    }
}
