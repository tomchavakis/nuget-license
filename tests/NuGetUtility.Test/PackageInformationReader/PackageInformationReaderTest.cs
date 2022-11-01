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
            _sourceRepositories = new List<Mock<IDisposableSourceRepository>>();

            _sourceRepositoryProvider.Setup(m => m.GetRepositories())
                .Returns(() =>
                {
                    foreach (var repository in _sourceRepositories ??
                                               Enumerable.Empty<Mock<IDisposableSourceRepository>>())
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
                new PackageSearchMetadataMock(new PackageIdentity(p.Id, CreateMockedVersion(p.Version))) as
                    IPackageSearchMetadata);

            var (project, result) = await PerformSearch(searchedPackages);
            CheckResult(result, project, _customPackageInformation);
        }
        private async Task<(string Project, ReferencedPackageWithContext[] Result)> PerformSearch(
            IEnumerable<IPackageSearchMetadata> searchedPackages)
        {
            var project = _fixture.Create<string>();
            var packageSearchRequest = new ProjectWithReferencedPackages(project, searchedPackages);
            var result = (await _uut!.GetPackageInfo(packageSearchRequest, CancellationToken.None).Synchronize())
                .ToArray();
            return (project, result);
        }

        private static void CheckResult(ReferencedPackageWithContext[] result,
            string project,
            IEnumerable<CustomPackageInformation> packages)
        {
            CollectionAssert.AreEquivalent(packages,
                result.Select(s => new CustomPackageInformation(s.PackageInfo.Identity.Id,
                    s.PackageInfo.Identity.Version,
                    s.PackageInfo.LicenseMetadata.License)));
            foreach (var r in result)
            {
                Assert.AreEqual(project, r.Context);
            }
        }

        [Test]
        public async Task GetPackageInfo_Should_IterateThroughRepositoriesToGetAdditionalInformation()
        {
            var sourceRepositoriesWithPackageMetadataResource = _sourceRepositories!.Shuffle(2345).Take(2).ToArray();
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
                var metadataReturningProperInformation = packageMetadataResources.Shuffle(6435).First();
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

            var (project, result) = await PerformSearch(searchedPackages);
            CheckResult(result, project, searchedPackagesAsPackageInformation);
        }

        [Test]
        public async Task
            GetPackageInfo_Should_IgnoreFailingPackageMetadataResourceGetting_if_IterateThroughRepositoriesToGetAdditionalInformation()
        {
            var shuffledRepositories = _sourceRepositories!.Shuffle(14563);
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
                var metadataReturningProperInformation = packageMetadataResources.Shuffle(4361).First();
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

            var (project, result) = await PerformSearch(searchedPackages);
            CheckResult(result, project, searchedPackagesAsPackageInformation);
        }

        [Test]
        public async Task GetPackageInfo_Should_ReturnInputForPackagesWithoutProperLicenseInformation()
        {
            var searchedPackagesAsPackageInformation = _fixture.CreateMany<CustomPackageInformation>().ToArray();
            var searchedPackages = searchedPackagesAsPackageInformation.Select(p =>
                    new PackageSearchMetadataMock(new PackageIdentity(p.Id, CreateMockedVersion(p.Version))) as
                        IPackageSearchMetadata)
                .ToArray();

            var (project, result) = await PerformSearch(searchedPackages);
            CheckResult(result, project, searchedPackages);
        }
        private void CheckResult(ReferencedPackageWithContext[] result,
            string project,
            IPackageSearchMetadata[] packages)
        {
            CollectionAssert.AreEquivalent(packages,
                result.Select(s => s.PackageInfo));
            foreach (var r in result)
            {
                Assert.AreEqual(project, r.Context);
            }
        }

        private INuGetVersion CreateMockedVersion(NuGetVersion innerVersion)
        {
            var mock = new Mock<INuGetVersion>();
            mock.Setup(m => m.ToString()).Returns(innerVersion.ToString());

            return mock.Object;
        }
    }
}
