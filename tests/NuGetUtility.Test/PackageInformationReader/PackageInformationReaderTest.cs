// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using AutoFixture;
using AutoFixture.AutoNSubstitute;
using NSubstitute;
using NuGetUtility.PackageInformationReader;
using NuGetUtility.Test.Helper.AsyncEnumerableExtension;
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
            _sourceRepositoryProvider = Substitute.For<IWrappedSourceRepositoryProvider>();
            _customPackageInformation = Enumerable.Empty<CustomPackageInformation>().ToList();
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _fixture.Customizations.Add(new NuGetVersionBuilder());
            _repositories = Array.Empty<ISourceRepository>();
            _globalPackagesFolderUtility = Substitute.For<IGlobalPackagesFolderUtility>();

            _globalPackagesFolderUtility.GetPackage(Arg.Any<PackageIdentity>()).Returns(default(IPackageMetadata?));

            _sourceRepositoryProvider.GetRepositories()
                .Returns(_ =>
                {
                    Assert.AreEqual(0, _repositories.Length);
                    _repositories = _fixture.CreateMany<ISourceRepository>().ToArray();
                    foreach (ISourceRepository repo in _repositories)
                    {
                        repo.GetPackageMetadataResourceAsync().Returns(_ => Task.FromResult(default(IPackageMetadataResource?)));
                    }
                    return _repositories;
                });

            SetupUut();
        }

        [TearDown]
        public void TearDown()
        {
            _repositories = Array.Empty<ISourceRepository>();
            _uut = null!;
        }

        private void SetupUut()
        {
            TearDown();
            _uut = new NuGetUtility.PackageInformationReader.PackageInformationReader(_sourceRepositoryProvider, _globalPackagesFolderUtility, _customPackageInformation);
        }

        private NuGetUtility.PackageInformationReader.PackageInformationReader _uut = null!;
        private IWrappedSourceRepositoryProvider _sourceRepositoryProvider = null!;
        private List<CustomPackageInformation> _customPackageInformation = null!;
        private IFixture _fixture = null!;
        private ISourceRepository[] _repositories = null!;
        private IGlobalPackagesFolderUtility _globalPackagesFolderUtility = null!;

        [Test]
        public async Task GetPackageInfo_Should_PreferProvidedCustomInformation()
        {
            _customPackageInformation = _fixture.CreateMany<CustomPackageInformation>().ToList();
            SetupUut();

            IEnumerable<PackageIdentity> searchedPackages = _customPackageInformation.Select(p => new PackageIdentity(p.Id, p.Version));

            (string project, ReferencedPackageWithContext[] result) = await PerformSearch(searchedPackages);
            CheckResult(result, project, _customPackageInformation, LicenseType.Overwrite);
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
            IEnumerable<CustomPackageInformation> packages,
            LicenseType licenseType)
        {
            CollectionAssert.AreEquivalent(packages,
                result.Select(s => new CustomPackageInformation(s.PackageInfo.Identity.Id,
                                                                s.PackageInfo.Identity.Version,
                                                                s.PackageInfo.LicenseMetadata!.License,
                                                                s.PackageInfo.Copyright,
                                                                s.PackageInfo.Authors,
                                                                s.PackageInfo.Title,
                                                                s.PackageInfo.ProjectUrl,
                                                                s.PackageInfo.Summary,
                                                                s.PackageInfo.Description)));
            foreach (ReferencedPackageWithContext r in result)
            {
                Assert.AreEqual(project, r.Context);
                Assert.AreEqual(licenseType, r.PackageInfo.LicenseMetadata!.Type);
            }
        }

        [Test]
        public async Task GetPackageInfo_Should_PreferLocalPackageCacheOverRepositories()
        {
            CustomPackageInformation[] searchedPackagesAsPackageInformation = _fixture.CreateMany<CustomPackageInformation>(20).ToArray();

            IEnumerable<PackageIdentity> searchedPackages = searchedPackagesAsPackageInformation.Select(info =>
            {
                var identity = new PackageIdentity(info.Id, info.Version);
                IPackageMetadata mockedInfo = Substitute.For<IPackageMetadata>();
                mockedInfo.Identity.Returns(identity);
                mockedInfo.Copyright.Returns(info.Copyright);
                mockedInfo.Authors.Returns(info.Authors);
                mockedInfo.Title.Returns(info.Title);
                mockedInfo.ProjectUrl.Returns(info.ProjectUrl);
                mockedInfo.Summary.Returns(info.Summary);
                mockedInfo.Description.Returns(info.Description);
                mockedInfo.LicenseMetadata.Returns(new LicenseMetadata(LicenseType.Expression, info.License));
                _globalPackagesFolderUtility.GetPackage(identity).Returns(mockedInfo);

                return identity;
            });

            (string project, ReferencedPackageWithContext[] result) = await PerformSearch(searchedPackages);
            CheckResult(result, project, searchedPackagesAsPackageInformation, LicenseType.Expression);

            foreach (ISourceRepository repo in _repositories)
            {
                await repo.Received(0).GetPackageMetadataResourceAsync();
            }
        }

        private void SetupPackagesForRepositories(IEnumerable<CustomPackageInformation> packages, IEnumerable<IPackageMetadataResource> packageMetadataResources)
        {
            foreach (CustomPackageInformation package in packages)
            {
                IPackageMetadataResource metadataReturningProperInformation = packageMetadataResources.Shuffle(6435).First();
                IPackageMetadata resultingInfo = Substitute.For<IPackageMetadata>();
                resultingInfo.Identity.Returns(new PackageIdentity(package.Id, package.Version));
                resultingInfo.LicenseMetadata.Returns(new LicenseMetadata(LicenseType.Expression, package.License));
                resultingInfo.Copyright.Returns(package.Copyright);
                resultingInfo.Authors.Returns(package.Authors);
                resultingInfo.Title.Returns(package.Title);
                resultingInfo.Summary.Returns(package.Summary);
                resultingInfo.Description.Returns(package.Description);
                resultingInfo.ProjectUrl.Returns(package.ProjectUrl);

                metadataReturningProperInformation.TryGetMetadataAsync(new PackageIdentity(package.Id, package.Version), Arg.Any<CancellationToken>()).
                    Returns(_ => Task.FromResult<IPackageMetadata?>(resultingInfo));
            }
        }

        [Test]
        public async Task GetPackageInfo_Should_IterateThroughRepositoriesToGetAdditionalInformation()
        {
            IEnumerable<ISourceRepository> shuffledRepositories = _repositories!.Shuffle(14563);
            IGrouping<int, (int Index, ISourceRepository Repo)>[] splitRepositories = shuffledRepositories.Select((repo, index) => (Index: index, Repo: repo))
                .GroupBy(e => e.Index % 2)
                .ToArray();

            ISourceRepository[] sourceRepositoriesWithPackageMetadataResource = splitRepositories[0].Select(e => e.Repo).ToArray();
            ISourceRepository[] sourceRepositoriesWithFailingPackageMetadataResource =
                splitRepositories[1].Select(e => e.Repo).ToArray();
            IPackageMetadataResource[] packageMetadataResources = sourceRepositoriesWithPackageMetadataResource.Select(r =>
                {
                    IPackageMetadataResource metadataResource = Substitute.For<IPackageMetadataResource>();
                    r.GetPackageMetadataResourceAsync().Returns(_ => Task.FromResult<IPackageMetadataResource?>(metadataResource));
                    return metadataResource;
                })
                .ToArray();
            foreach (ISourceRepository? repo in sourceRepositoriesWithFailingPackageMetadataResource)
            {
                repo.When(m => m.GetPackageMetadataResourceAsync()).Do(_ => throw new Exception());
            }

            CustomPackageInformation[] searchedPackagesAsPackageInformation = _fixture.CreateMany<CustomPackageInformation>(20).ToArray();

            SetupPackagesForRepositories(searchedPackagesAsPackageInformation, packageMetadataResources);

            IEnumerable<PackageIdentity> searchedPackages = searchedPackagesAsPackageInformation.Select(i => new PackageIdentity(i.Id, i.Version));

            (string project, ReferencedPackageWithContext[] result) = await PerformSearch(searchedPackages);
            CheckResult(result, project, searchedPackagesAsPackageInformation, LicenseType.Expression);
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
                Assert.IsNull(result.PackageInfo.Summary);
                Assert.IsNull(result.PackageInfo.Title);
            }
        }
    }
}
