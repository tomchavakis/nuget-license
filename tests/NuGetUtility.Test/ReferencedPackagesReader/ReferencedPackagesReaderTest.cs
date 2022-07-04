using AutoFixture;
using Moq;
using NuGetUtility.ReferencedPackagesReader;
using NuGetUtility.Test.Helper.AutoFixture;
using NuGetUtility.Test.Helper.NuGet.Protocol.Core.Types;
using NuGetUtility.Test.Helper.ShuffelledEnumerable;
using NuGetUtility.Wrapper.MsBuildWrapper;
using NuGetUtility.Wrapper.NuGetWrapper.Frameworks;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;
using NuGetUtility.Wrapper.NuGetWrapper.ProjectModel;
using NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types;
using NuGetUtility.Wrapper.NuGetWrapper.Versioning;
using NUnit.Framework;

namespace NuGetUtility.Test.ReferencedPackagesReader
{
    [TestFixture]
    internal class ReferencedPackageReaderTest
    {
        [SetUp]
        public void SetUp()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new MockBuilder());
            _ignoredPackages = fixture.CreateMany<string>();
            _msBuild = new Mock<IMsBuildAbstraction>();
            _lockFileFactory = new Mock<ILockFileFactory>();
            _metadataBuilderFactory = new Mock<IPackageSearchMetadataBuilderFactory>();
            _projectPath = fixture.Create<string>();
            _assetsFilePath = fixture.Create<string>();
            _projectMock = new Mock<IProject>();
            _lockFileMock = new Mock<ILockFile>();
            _packageSpecMock = new Mock<IPackageSpec>();
            _lockFileTargets = fixture.CreateMany<Mock<ILockFileTarget>>(TargetFrameworkCount).ToArray();
            _lockFileLibraries = fixture.CreateMany<Mock<ILockFileLibrary>>(50).ToArray();
            _packageSpecTargetFrameworks =
                fixture.CreateMany<Mock<ITargetFrameworkInformation>>(TargetFrameworkCount).ToArray();
            _targetFrameworks = fixture.CreateMany<Mock<INuGetFramework>>(TargetFrameworkCount).ToArray();
            _packageReferencesFromProjectForFramework = new Dictionary<string, PackageReference[]>();

            _msBuild.Setup(m => m.GetProject(_projectPath)).Returns(_projectMock.Object);
            _projectMock.Setup(m => m.GetPackageReferenceCount()).Returns(1);
            _projectMock.Setup(m => m.GetAssetsPath()).Returns(_assetsFilePath);
            _projectMock.SetupGet(m => m.FullPath).Returns(_projectPath);
            _lockFileFactory.Setup(m => m.GetFromFile(_assetsFilePath)).Returns(_lockFileMock.Object);
            _lockFileMock.SetupGet(m => m.PackageSpec).Returns(_packageSpecMock.Object);
            _packageSpecMock.Setup(m => m.IsValid()).Returns(true);
            _lockFileMock.SetupGet(m => m.Targets).Returns(_lockFileTargets.Select(t => t.Object));
            _lockFileMock.SetupGet(m => m.Libraries).Returns(_lockFileLibraries.Select(l => l.Object));
            _packageSpecMock.SetupGet(m => m.TargetFrameworks)
                .Returns(_packageSpecTargetFrameworks.Select(t => t.Object));
            _metadataBuilderFactory.Setup(m => m.FromIdentity(It.IsAny<PackageIdentity>())).Returns(
                (PackageIdentity id) =>
                {
                    var builder = new Mock<IPackageSearchMetadataBuilder>();
                    builder.Setup(m => m.Build()).Returns(new PackageSearchMetadataMock(id));
                    return builder.Object;
                });

            _msBuild.Setup(m => m.GetPackageReferencesFromProjectForFramework(_projectMock.Object, It.IsAny<string>()))
                .Returns((IProject _, string framework) => _packageReferencesFromProjectForFramework[framework]);

            foreach (var lockFileLibrary in _lockFileLibraries)
            {
                var version = new Mock<INuGetVersion>();
                lockFileLibrary.SetupGet(m => m.Version).Returns(version.Object);
                lockFileLibrary.SetupGet(m => m.Name).Returns(fixture.Create<string>());
            }

            foreach (var targetFramework in _targetFrameworks)
            {
                targetFramework.Setup(m => m.ToString()).Returns(fixture.Create<string>());
            }

            foreach (var targetFramework in _targetFrameworks)
            {
                var returnedLibraries = _lockFileLibraries.Shuffle().Take(5)
                    .Select(l => new PackageReference(l.Object.Name, l.Object.Version)).ToArray();
                _packageReferencesFromProjectForFramework[targetFramework.Object.ToString()!] = returnedLibraries;
            }

            using (var targetFrameworksIterator = _targetFrameworks.GetEnumerator())
            {
                foreach (var lockFileTarget in _lockFileTargets)
                {
                    targetFrameworksIterator.MoveNext();
                    lockFileTarget.SetupGet(m => m.TargetFramework).Returns(targetFrameworksIterator.Current.Object);
                }
            }

            using (var targetFrameworksIterator = _targetFrameworks.GetEnumerator())
            {
                foreach (var packageSpecTargetFramework in _packageSpecTargetFrameworks)
                {
                    targetFrameworksIterator.MoveNext();
                    packageSpecTargetFramework.SetupGet(m => m.FrameworkName)
                        .Returns(targetFrameworksIterator.Current.Object);
                }
            }

            _uut = new ReferencedPackageReader(_ignoredPackages, _msBuild.Object, _lockFileFactory.Object,
                _metadataBuilderFactory.Object);
        }

        private const int TargetFrameworkCount = 5;
        private ReferencedPackageReader? _uut;
        private IEnumerable<string>? _ignoredPackages;
        private Mock<IMsBuildAbstraction>? _msBuild;
        private Mock<ILockFileFactory>? _lockFileFactory;
        private Mock<IPackageSearchMetadataBuilderFactory>? _metadataBuilderFactory;
        private string? _projectPath;
        private string? _assetsFilePath;
        private Mock<IProject>? _projectMock;
        private Mock<ILockFile>? _lockFileMock;
        private Mock<IPackageSpec>? _packageSpecMock;
        private IEnumerable<Mock<ILockFileTarget>>? _lockFileTargets;
        private IEnumerable<Mock<ILockFileLibrary>>? _lockFileLibraries;
        private IEnumerable<Mock<ITargetFrameworkInformation>>? _packageSpecTargetFrameworks;
        private IEnumerable<Mock<INuGetFramework>>? _targetFrameworks;
        private Dictionary<string, PackageReference[]>? _packageReferencesFromProjectForFramework;

        [Test]
        public void GetInstalledPackages_Should_ThrowReferencedPackageReaderException_If_PackageSpecificationIsInvalid(
            [Values] bool includeTransitive)
        {
            _packageSpecMock!.Setup(m => m.IsValid()).Returns(false);
            _projectMock!.SetupGet(m => m.FullPath).Returns(_projectPath!);

            var exception = Assert.Throws<ReferencedPackageReaderException>(() =>
                _uut!.GetInstalledPackages(_projectPath!, includeTransitive));

            Assert.AreEqual($"Failed to validate project assets for project {_projectPath}", exception!.Message);
        }

        [Test]
        public void GetInstalledPackages_Should_ThrowReferencedPackageReaderException_If_TargetsArrayIsNull(
            [Values] bool includeTransitive)
        {
            _lockFileMock!.SetupGet(m => m.Targets).Returns((IEnumerable<ILockFileTarget>?)null);

            var exception = Assert.Throws<ReferencedPackageReaderException>(() =>
                _uut!.GetInstalledPackages(_projectPath!, includeTransitive));

            Assert.AreEqual($"Failed to validate project assets for project {_projectPath}", exception!.Message);
        }

        [Test]
        public void
            GetInstalledPackages_Should_ThrowReferencedPackageReaderException_If_TargetsArrayDoesNotContainAnyElement(
                [Values] bool includeTransitive)
        {
            _lockFileMock!.SetupGet(m => m.Targets).Returns(Enumerable.Empty<ILockFileTarget>());

            var exception = Assert.Throws<ReferencedPackageReaderException>(() =>
                _uut!.GetInstalledPackages(_projectPath!, includeTransitive));

            Assert.AreEqual($"Failed to validate project assets for project {_projectPath}", exception!.Message);
        }

        [Test]
        public void
            GetInstalledPackages_Should_ThrowReferencedPackageReaderException_If_NotIncludingTransitive_And_PackageSpecFrameworkInformationGetFails()
        {
            _packageSpecMock!.SetupGet(m => m.TargetFrameworks)
                .Returns(Enumerable.Empty<ITargetFrameworkInformation>());
            var exception = Assert.Throws<ReferencedPackageReaderException>(() =>
                _uut!.GetInstalledPackages(_projectPath!, false));

            Assert.AreEqual(
                $"Failed to identify the target framework information for {_lockFileTargets!.First().Object}",
                exception!.Message);
            Assert.IsInstanceOf(typeof(InvalidOperationException), exception!.InnerException);
            Assert.AreEqual(exception.InnerException!.Message, "Sequence contains no matching element");
        }

        [Test]
        public void
            GetInstalledPackages_Should_ThrowReferencedPackageReaderException_If_IncludingTransitive_And_PackageSpecFrameworkInformationGetFails()
        {
            _packageSpecMock!.SetupGet(m => m.TargetFrameworks)
                .Returns(Enumerable.Empty<ITargetFrameworkInformation>());
            var result = _uut!.GetInstalledPackages(_projectPath!, true);
            CollectionAssert.AreEquivalent(
                _lockFileLibraries!.Select(l =>
                    new PackageSearchMetadataMock(new PackageIdentity(l.Object.Name, l.Object.Version))), result);
        }

        [Test]
        public void GetInstalledPackages_Should_GetProjectFromPath([Values] bool includeTransitive)
        {
            _uut!.GetInstalledPackages(_projectPath!, includeTransitive);
            _msBuild!.Verify(m => m.GetProject(It.IsAny<string>()), Times.Once);
            _msBuild!.Verify(m => m.GetProject(_projectPath!), Times.Once);
        }

        [Test]
        public void GetInstalledPackages_Should_LoadAssetsFileFromProject([Values] bool includeTransitive)
        {
            _uut!.GetInstalledPackages(_projectPath!, includeTransitive);
            _projectMock!.Verify(m => m.GetAssetsPath(), Times.Once);
            _lockFileFactory!.Verify(m => m.GetFromFile(It.IsAny<string>()), Times.Once);
            _lockFileFactory!.Verify(m => m.GetFromFile(_assetsFilePath!), Times.Once);
        }

        [Test]
        public void GetInstalledPackages_Should_ReturnCorrectValues_If_IncludingTransitive()
        {
            var result = _uut!.GetInstalledPackages(_projectPath!, true);
            CollectionAssert.AreEquivalent(
                _lockFileLibraries!.Select(l =>
                    new PackageSearchMetadataMock(new PackageIdentity(l.Object.Name, l.Object.Version))), result);
        }

        [Test]
        public void GetInstalledPackages_Should_ReturnCorrectValues_If_NotIncludingTransitive()
        {
            var result = _uut!.GetInstalledPackages(_projectPath!, false);

            var expectedReferences = _packageReferencesFromProjectForFramework!.SelectMany(p => p.Value).Distinct()
                .ToArray();
            var expectedResult = _lockFileLibraries!.Where(l =>
                expectedReferences.Any(e => e.PackageName.Equals(l.Object.Name)) &&
                expectedReferences.Any(e => e.Version!.Equals(l.Object.Version))).ToArray();
            CollectionAssert.AreEquivalent(
                expectedResult.Select(l =>
                    new PackageSearchMetadataMock(new PackageIdentity(l.Object.Name, l.Object.Version))), result);
        }

        [Test]
        public void GetInstalledPackages_Should_ReturnCorrectValues_If_IncludingTransitive_if_IgnoringPackages()
        {
            var ignoredPackageName = _lockFileLibraries!.Shuffle().First().Object.Name;
            _ignoredPackages = _ignoredPackages!.Append(ignoredPackageName);

            _uut = new ReferencedPackageReader(_ignoredPackages, _msBuild!.Object, _lockFileFactory!.Object,
                _metadataBuilderFactory!.Object);
            var result = _uut!.GetInstalledPackages(_projectPath!, true);

            CollectionAssert.AreEquivalent(
                _lockFileLibraries!.Where(l => l.Object.Name != ignoredPackageName).Select(l =>
                    new PackageSearchMetadataMock(new PackageIdentity(l.Object.Name, l.Object.Version))), result);
        }

        [Test]
        public void GetInstalledPackages_Should_ReturnCorrectValues_If_NotIncludingTransitive_If_IgnoringPackages()
        {
            var directReferences = _packageReferencesFromProjectForFramework!.SelectMany(p => p.Value).Distinct()
                .ToArray();
            var directReferencesResult = _lockFileLibraries!.Where(l =>
                directReferences.Any(e => e.PackageName.Equals(l.Object.Name)) &&
                directReferences.Any(e => e.Version!.Equals(l.Object.Version))).ToArray();

            var ignoredPackageName = directReferencesResult.Shuffle().First().Object.Name;
            _ignoredPackages = _ignoredPackages!.Append(ignoredPackageName);

            _uut = new ReferencedPackageReader(_ignoredPackages, _msBuild!.Object, _lockFileFactory!.Object,
                _metadataBuilderFactory!.Object);
            var result = _uut!.GetInstalledPackages(_projectPath!, false);

            CollectionAssert.AreEquivalent(
                directReferencesResult.Where(l => l.Object.Name != ignoredPackageName).Select(l =>
                    new PackageSearchMetadataMock(new PackageIdentity(l.Object.Name, l.Object.Version))), result);
        }

        [Test]
        public void
            GetInstalledPackages_Should_ReturnEmptyCollection_When_ProjectHasNoPackageReferences_And_IsNotTransitive()
        {
            _projectMock!.Setup(m => m.GetPackageReferenceCount()).Returns(0);
            _projectMock!.Setup(m => m.GetEvaluatedIncludes()).Returns(Enumerable.Empty<string>());
            var result = _uut!.GetInstalledPackages(_projectPath!, false);

            Assert.That(result.Count(), Is.EqualTo(0));
        }
    }
}
