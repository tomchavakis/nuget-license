using AutoFixture;
using Moq;
using NuGetUtility.PackageInformationReader;
using NuGetUtility.Test.Helper.AsyncEnumerableExtension;
using NuGetUtility.Test.Helper.AutoFixture;
using NuGetUtility.Test.Helper.AutoFixture.NuGet.Versioning;
using NuGetUtility.Test.Helper.ShuffelledEnumerable;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;
using NuGetUtility.Wrapper.NuGetWrapper.Protocol;
using NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types;

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
            _repositories = Array.Empty<Mock<ISourceRepository>>();
            _globalPackagesFolderUtility = new();

            _sourceRepositoryProvider.Setup(m => m.GetRepositories())
                .Returns((Delegate)(() =>
                {
                    Assert.AreEqual(Array.Empty<Mock<ISourceRepository>>(), _repositories);
                    _repositories = _fixture.CreateMany<Mock<ISourceRepository>>().ToArray<Mock<ISourceRepository>>();
                    return _repositories.Select<Mock<ISourceRepository>, ISourceRepository>(r => r.Object).ToArray();
                }));

            SetupUut();
        }

        [TearDown]
        public void TearDown()
        {
            _repositories = Array.Empty<Mock<ISourceRepository>>();
            _uut = null!;
        }

        private void SetupUut()
        {
            TearDown();
            _uut = new NuGetUtility.PackageInformationReader.PackageInformationReader(_sourceRepositoryProvider.Object, _globalPackagesFolderUtility.Object, _customPackageInformation);
        }

        private NuGetUtility.PackageInformationReader.PackageInformationReader _uut = null!;
        private Mock<IWrappedSourceRepositoryProvider> _sourceRepositoryProvider = null!;
        private List<CustomPackageInformation> _customPackageInformation = null!;
        private Fixture _fixture = null!;
        private Mock<ISourceRepository>[] _repositories = null!;
        private Mock<IGlobalPackagesFolderUtility> _globalPackagesFolderUtility = null!;

        [Test]
        public async Task GetPackageInfo_Should_PreferProvidedCustomInformation()
        {
            _customPackageInformation = _fixture.CreateMany<CustomPackageInformation>().ToList();
            SetupUut();

            IEnumerable<PackageIdentity> searchedPackages = _customPackageInformation.Select(p => new PackageIdentity(p.Id, p.Version));

            (string project, ReferencedPackageWithContext[] result) = await PerformSearch(searchedPackages);
            CheckResult(result, project, _customPackageInformation);
        }

        private async Task<(string Project, ReferencedPackageWithContext[] Result)> PerformSearch(
            IEnumerable<PackageIdentity> searchedPackages)
        {
            string project = _fixture.Create<string>();
            var packageSearchRequest = new ProjectWithReferencedPackages(project, searchedPackages);
            ReferencedPackageWithContext[] result = (await _uut!.GetPackageInfo(packageSearchRequest, CancellationToken.None).Synchronize())
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
                    s.PackageInfo.LicenseMetadata!.License)));
            foreach (ReferencedPackageWithContext r in result)
            {
                Assert.AreEqual(project, r.Context);
            }
        }

        [Test]
        public async Task GetPackageInfo_Should_PreferLocalPackageCacheOverRepositories()
        {
            CustomPackageInformation[] searchedPackagesAsPackageInformation = _fixture.CreateMany<CustomPackageInformation>(20).ToArray();

            IEnumerable<PackageIdentity> searchedPackages = searchedPackagesAsPackageInformation.Select(info =>
            {
                var identity = new PackageIdentity(info.Id, info.Version);
                var mockedInfo = new Mock<IPackageMetadata>();
                mockedInfo.SetupGet(m => m.Identity).Returns(identity);
                mockedInfo.SetupGet(m => m.LicenseMetadata).Returns(new LicenseMetadata(LicenseType.Expression, info.License));
                _globalPackagesFolderUtility.Setup(m => m.GetPackage(identity)).Returns(mockedInfo.Object);

                return identity;
            });

            (string project, ReferencedPackageWithContext[] result) = await PerformSearch(searchedPackages);
            CheckResult(result, project, searchedPackagesAsPackageInformation);

            foreach (Mock<ISourceRepository> repo in _repositories)
            {
                repo.Verify(m => m.GetPackageMetadataResourceAsync(), Times.Never);
            }
        }

        private void SetupPackagesForRepositories(IEnumerable<CustomPackageInformation> packages, IEnumerable<Mock<IPackageMetadataResource>> packageMetadataResources)
        {
            foreach (CustomPackageInformation package in packages)
            {
                Mock<IPackageMetadataResource> metadataReturningProperInformation = packageMetadataResources.Shuffle(6435).First();
                var resultingInfo = new Mock<IPackageMetadata>();
                resultingInfo.SetupGet(m => m.Identity).Returns(new PackageIdentity(package.Id, package.Version));
                resultingInfo.SetupGet(m => m.LicenseMetadata).Returns(new LicenseMetadata(LicenseType.Expression, package.License));

                metadataReturningProperInformation
                    .Setup(m => m.TryGetMetadataAsync(new PackageIdentity(package.Id, package.Version), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(resultingInfo.Object);
            }
        }

        [Test]
        public async Task GetPackageInfo_Should_IterateThroughRepositoriesToGetAdditionalInformation()
        {
            IEnumerable<Mock<ISourceRepository>> shuffledRepositories = _repositories!.Shuffle(14563);
            IGrouping<int, (int Index, Mock<ISourceRepository> Repo)>[] splitRepositories = shuffledRepositories.Select((repo, index) => (Index: index, Repo: repo))
                .GroupBy(e => e.Index % 2)
                .ToArray();

            Mock<ISourceRepository>[] sourceRepositoriesWithPackageMetadataResource = splitRepositories[0].Select(e => e.Repo).ToArray();
            Mock<ISourceRepository>[] sourceRepositoriesWithFailingPackageMetadataResource =
                splitRepositories[1].Select(e => e.Repo).ToArray();
            Mock<IPackageMetadataResource>[] packageMetadataResources = sourceRepositoriesWithPackageMetadataResource.Select(r =>
                {
                    var metadataResource = new Mock<IPackageMetadataResource>();
                    r.Setup(m => m.GetPackageMetadataResourceAsync()).ReturnsAsync(metadataResource.Object);
                    return metadataResource;
                })
                .ToArray();
            foreach (Mock<ISourceRepository>? repo in sourceRepositoriesWithFailingPackageMetadataResource)
            {
                repo.Setup(m => m.GetPackageMetadataResourceAsync()).Callback(() => throw new Exception());
            }

            CustomPackageInformation[] searchedPackagesAsPackageInformation = _fixture.CreateMany<CustomPackageInformation>(20).ToArray();

            SetupPackagesForRepositories(searchedPackagesAsPackageInformation, packageMetadataResources);

            IEnumerable<PackageIdentity> searchedPackages = searchedPackagesAsPackageInformation.Select(i => new PackageIdentity(i.Id, i.Version));

            (string project, ReferencedPackageWithContext[] result) = await PerformSearch(searchedPackages);
            CheckResult(result, project, searchedPackagesAsPackageInformation);
        }

        [Test]
        public async Task GetPackageInfo_Should_ReturnDummyPackageMetadataForPackagesNotFound()
        {
            CustomPackageInformation[] searchedPackagesAsPackageInformation = _fixture.CreateMany<CustomPackageInformation>().ToArray();
            PackageIdentity[] searchedPackages = searchedPackagesAsPackageInformation.Select(p => new PackageIdentity(p.Id, p.Version)).ToArray();

            (string project, ReferencedPackageWithContext[] results) = await PerformSearch(searchedPackages);

            Assert.AreEqual(searchedPackages.Count(), results.Length);
            for (int i = 0; i < results.Length; i++)
            {
                PackageIdentity expectation = searchedPackages[i];
                ReferencedPackageWithContext result = results[i];
                Assert.AreEqual(project, result.Context);
                Assert.AreEqual(expectation.Id, result.PackageInfo.Identity.Id);
                Assert.AreEqual(expectation.Version, result.PackageInfo.Identity.Version);
                Assert.IsNull(result.PackageInfo.LicenseMetadata);
                Assert.IsNull(result.PackageInfo.LicenseUrl);
                Assert.AreEqual(string.Empty, result.PackageInfo.Summary);
                Assert.AreEqual(string.Empty, result.PackageInfo.Title);
            }
        }
    }
}
