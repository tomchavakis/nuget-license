using AutoFixture;
using Moq;
using NuGetUtility.ReferencedPackagesReader;
using NuGetUtility.Test.Helper.AutoFixture;
using NuGetUtility.Test.Helper.ShuffelledEnumerable;
using NuGetUtility.Wrapper.MsBuildWrapper;
using NuGetUtility.Wrapper.NuGetWrapper.Frameworks;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;
using NuGetUtility.Wrapper.NuGetWrapper.ProjectModel;
using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

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

            _msBuild.Setup(m => m.GetPackageReferencesFromProjectForFramework(_projectMock.Object, It.IsAny<string>()))
                .Returns((IProject _, string framework) => _packageReferencesFromProjectForFramework[framework]);

            foreach (Mock<ILockFileLibrary> lockFileLibrary in _lockFileLibraries)
            {
                var version = new Mock<INuGetVersion>();
                lockFileLibrary.SetupGet(m => m.Version).Returns(version.Object);
                lockFileLibrary.SetupGet(m => m.Name).Returns(fixture.Create<string>());
            }

            foreach (Mock<INuGetFramework> targetFramework in _targetFrameworks)
            {
                targetFramework.Setup(m => m.ToString()).Returns(fixture.Create<string>());
            }

            foreach (Mock<INuGetFramework> targetFramework in _targetFrameworks)
            {
                PackageReference[] returnedLibraries = _lockFileLibraries.Shuffle(75643)
                    .Take(5)
                    .Select(l => new PackageReference(l.Object.Name, l.Object.Version))
                    .ToArray();
                _packageReferencesFromProjectForFramework[targetFramework.Object.ToString()!] = returnedLibraries;
            }

            using (IEnumerator<Mock<INuGetFramework>> targetFrameworksIterator = _targetFrameworks.GetEnumerator())
            {
                foreach (Mock<ILockFileTarget> lockFileTarget in _lockFileTargets)
                {
                    targetFrameworksIterator.MoveNext();
                    lockFileTarget.SetupGet(m => m.TargetFramework).Returns(targetFrameworksIterator.Current.Object);
                }
            }

            using (IEnumerator<Mock<INuGetFramework>> targetFrameworksIterator = _targetFrameworks.GetEnumerator())
            {
                foreach (Mock<ITargetFrameworkInformation> packageSpecTargetFramework in _packageSpecTargetFrameworks)
                {
                    targetFrameworksIterator.MoveNext();
                    packageSpecTargetFramework.SetupGet(m => m.FrameworkName)
                        .Returns(targetFrameworksIterator.Current.Object);
                }
            }

            _uut = new ReferencedPackageReader(_ignoredPackages,
                _msBuild.Object,
                _lockFileFactory.Object);
        }

        private const int TargetFrameworkCount = 5;
        private ReferencedPackageReader _uut = null!;
        private IEnumerable<string> _ignoredPackages = null!;
        private Mock<IMsBuildAbstraction> _msBuild = null!;
        private Mock<ILockFileFactory> _lockFileFactory = null!;
        private string _projectPath = null!;
        private string _assetsFilePath = null!;
        private Mock<IProject> _projectMock = null!;
        private Mock<ILockFile> _lockFileMock = null!;
        private Mock<IPackageSpec> _packageSpecMock = null!;
        private IEnumerable<Mock<ILockFileTarget>> _lockFileTargets = null!;
        private IEnumerable<Mock<ILockFileLibrary>> _lockFileLibraries = null!;
        private IEnumerable<Mock<ITargetFrameworkInformation>> _packageSpecTargetFrameworks = null!;
        private IEnumerable<Mock<INuGetFramework>> _targetFrameworks = null!;
        private Dictionary<string, PackageReference[]> _packageReferencesFromProjectForFramework = null!;

        [Test]
        public void GetInstalledPackages_Should_ThrowReferencedPackageReaderException_If_PackageSpecificationIsInvalid(
            [Values] bool includeTransitive)
        {
            _packageSpecMock.Setup(m => m.IsValid()).Returns(false);
            _projectMock.SetupGet(m => m.FullPath).Returns(_projectPath);

            ReferencedPackageReaderException? exception = Assert.Throws<ReferencedPackageReaderException>(() =>
                _uut.GetInstalledPackages(_projectPath, includeTransitive));

            Assert.AreEqual($"Failed to validate project assets for project {_projectPath}", exception!.Message);
        }

        [Test]
        public void GetInstalledPackages_Should_ThrowReferencedPackageReaderException_If_TargetsArrayIsNull(
            [Values] bool includeTransitive)
        {
            _lockFileMock.SetupGet(m => m.Targets).Returns((IEnumerable<ILockFileTarget>?)null);

            ReferencedPackageReaderException? exception = Assert.Throws<ReferencedPackageReaderException>(() =>
                _uut.GetInstalledPackages(_projectPath, includeTransitive));

            Assert.AreEqual($"Failed to validate project assets for project {_projectPath}", exception!.Message);
        }

        [Test]
        public void
            GetInstalledPackages_Should_ThrowReferencedPackageReaderException_If_TargetsArrayDoesNotContainAnyElement(
                [Values] bool includeTransitive)
        {
            _lockFileMock.SetupGet(m => m.Targets).Returns(Enumerable.Empty<ILockFileTarget>());

            ReferencedPackageReaderException? exception = Assert.Throws<ReferencedPackageReaderException>(() =>
                _uut.GetInstalledPackages(_projectPath, includeTransitive));

            Assert.AreEqual($"Failed to validate project assets for project {_projectPath}", exception!.Message);
        }

        [Test]
        public void
            GetInstalledPackages_Should_ThrowReferencedPackageReaderException_If_NotIncludingTransitive_And_PackageSpecFrameworkInformationGetFails()
        {
            _packageSpecMock.SetupGet(m => m.TargetFrameworks)
                .Returns(Enumerable.Empty<ITargetFrameworkInformation>());
            ReferencedPackageReaderException? exception = Assert.Throws<ReferencedPackageReaderException>(() =>
                _uut.GetInstalledPackages(_projectPath, false));

            Assert.AreEqual(
                $"Failed to identify the target framework information for {_lockFileTargets.First().Object}",
                exception!.Message);
            Assert.IsInstanceOf(typeof(InvalidOperationException), exception.InnerException);
            Assert.AreEqual(exception.InnerException!.Message, "Sequence contains no matching element");
        }

        [Test]
        public void
            GetInstalledPackages_Should_ThrowReferencedPackageReaderException_If_IncludingTransitive_And_PackageSpecFrameworkInformationGetFails()
        {
            _packageSpecMock.SetupGet(m => m.TargetFrameworks)
                .Returns(Enumerable.Empty<ITargetFrameworkInformation>());
            IEnumerable<PackageIdentity> result = _uut.GetInstalledPackages(_projectPath, true);
            CollectionAssert.AreEquivalent(
                _lockFileLibraries.Select(l =>
                    new PackageIdentity(l.Object.Name, l.Object.Version)),
                result);
        }

        [Test]
        public void GetInstalledPackages_Should_GetProjectFromPath([Values] bool includeTransitive)
        {
            _uut.GetInstalledPackages(_projectPath, includeTransitive);
            _msBuild.Verify(m => m.GetProject(It.IsAny<string>()), Times.Once);
            _msBuild.Verify(m => m.GetProject(_projectPath), Times.Once);
        }

        [Test]
        public void GetInstalledPackages_Should_LoadAssetsFileFromProject([Values] bool includeTransitive)
        {
            _uut.GetInstalledPackages(_projectPath, includeTransitive);
            _projectMock.Verify(m => m.GetAssetsPath(), Times.Once);
            _lockFileFactory.Verify(m => m.GetFromFile(It.IsAny<string>()), Times.Once);
            _lockFileFactory.Verify(m => m.GetFromFile(_assetsFilePath), Times.Once);
        }

        [Test]
        public void GetInstalledPackages_Should_ReturnCorrectValues_If_IncludingTransitive()
        {
            IEnumerable<PackageIdentity> result = _uut.GetInstalledPackages(_projectPath, true);
            CollectionAssert.AreEquivalent(
                _lockFileLibraries.Select(l =>
                    new PackageIdentity(l.Object.Name, l.Object.Version)),
                result);
        }

        [Test]
        public void GetInstalledPackages_Should_ReturnCorrectValues_If_NotIncludingTransitive()
        {
            IEnumerable<PackageIdentity> result = _uut.GetInstalledPackages(_projectPath, false);

            PackageReference[] expectedReferences = _packageReferencesFromProjectForFramework.SelectMany(p => p.Value)
                .Distinct()
                .ToArray();
            Mock<ILockFileLibrary>[] expectedResult = _lockFileLibraries.Where(l =>
                    expectedReferences.Any(e => e.PackageName.Equals(l.Object.Name)) &&
                    expectedReferences.Any(e => e.Version!.Equals(l.Object.Version)))
                .ToArray();
            CollectionAssert.AreEquivalent(
                expectedResult.Select(l =>
                    new PackageIdentity(l.Object.Name, l.Object.Version)),
                result);
        }

        [Test]
        public void GetInstalledPackages_Should_ReturnCorrectValues_If_IncludingTransitive_if_IgnoringPackages()
        {
            string ignoredPackageName = _lockFileLibraries!.Shuffle(6543154).First().Object.Name;
            _ignoredPackages = _ignoredPackages!.Append(ignoredPackageName);

            _uut = new ReferencedPackageReader(_ignoredPackages,
                _msBuild.Object,
                _lockFileFactory.Object);
            IEnumerable<PackageIdentity> result = _uut.GetInstalledPackages(_projectPath, true);

            CollectionAssert.AreEquivalent(
                _lockFileLibraries.Where(l => l.Object.Name != ignoredPackageName)
                    .Select(l =>
                        new PackageIdentity(l.Object.Name, l.Object.Version)),
                result);
        }
        
        [Test]
        public void GetInstalledPackages_Should_ReturnCorrectValues_If_IncludingTransitive_if_IgnoringPackagesWithWildCardAtEnd()
        {
            string shouldBeIgnored = _lockFileLibraries!.Shuffle(6543154).First().Object.Name;
            string ignoredPackageName = shouldBeIgnored[..30] + "*";
            _ignoredPackages = _ignoredPackages!.Append(ignoredPackageName);

            _uut = new ReferencedPackageReader(_ignoredPackages,
                _msBuild.Object,
                _lockFileFactory.Object);
            IEnumerable<PackageIdentity> result = _uut.GetInstalledPackages(_projectPath, true);

            CollectionAssert.AreEquivalent(
                _lockFileLibraries.Where(l => l.Object.Name != shouldBeIgnored)
                    .Select(l =>
                        new PackageIdentity(l.Object.Name, l.Object.Version)),
                result);
        }

        [Test]
        public void GetInstalledPackages_Should_ReturnCorrectValues_If_NotIncludingTransitive_If_IgnoringPackages()
        {
            PackageReference[] directReferences = _packageReferencesFromProjectForFramework.SelectMany(p => p.Value)
                .Distinct()
                .ToArray();
            Mock<ILockFileLibrary>[] directReferencesResult = _lockFileLibraries.Where(l =>
                    directReferences.Any(e => e.PackageName.Equals(l.Object.Name)) &&
                    directReferences.Any(e => e.Version!.Equals(l.Object.Version)))
                .ToArray();

            string ignoredPackageName = directReferencesResult.Shuffle(9).First().Object.Name;
            _ignoredPackages = _ignoredPackages!.Append(ignoredPackageName);

            _uut = new ReferencedPackageReader(_ignoredPackages,
                _msBuild.Object,
                _lockFileFactory.Object);
            IEnumerable<PackageIdentity> result = _uut.GetInstalledPackages(_projectPath, false);

            CollectionAssert.AreEquivalent(
                directReferencesResult.Where(l => l.Object.Name != ignoredPackageName)
                    .Select(l =>
                        new PackageIdentity(l.Object.Name, l.Object.Version)),
                result);
        }

        [Test]
        public void GetInstalledPackages_Should_ReturnCorrectValues_If_NotIncludingTransitive_If_IgnoringPackagesWithWildCardAtStart()
        {
            PackageReference[] directReferences = _packageReferencesFromProjectForFramework.SelectMany(p => p.Value)
                .Distinct()
                .ToArray();
            Mock<ILockFileLibrary>[] directReferencesResult = _lockFileLibraries.Where(l =>
                    directReferences.Any(e => e.PackageName.Equals(l.Object.Name)) &&
                    directReferences.Any(e => e.Version!.Equals(l.Object.Version)))
                .ToArray();

            string shouldBeIgnored = directReferencesResult.Shuffle(9).First().Object.Name;
            string ignoredPackageName = "*" + shouldBeIgnored[5..];
            _ignoredPackages = _ignoredPackages!.Append(ignoredPackageName);

            _uut = new ReferencedPackageReader(_ignoredPackages,
                _msBuild.Object,
                _lockFileFactory.Object);
            IEnumerable<PackageIdentity> result = _uut.GetInstalledPackages(_projectPath, false);

            CollectionAssert.AreEquivalent(
                directReferencesResult.Where(l => l.Object.Name != shouldBeIgnored)
                    .Select(l =>
                        new PackageIdentity(l.Object.Name, l.Object.Version)),
                result);
        }
        
        [Test]
        public void GetInstalledPackages_Should_ReturnCorrectValues_If_NotIncludingTransitive_If_IgnoringPackagesWithWildCardInMiddle()
        {
            PackageReference[] directReferences = _packageReferencesFromProjectForFramework.SelectMany(p => p.Value)
                .Distinct()
                .ToArray();
            Mock<ILockFileLibrary>[] directReferencesResult = _lockFileLibraries.Where(l =>
                    directReferences.Any(e => e.PackageName.Equals(l.Object.Name)) &&
                    directReferences.Any(e => e.Version!.Equals(l.Object.Version)))
                .ToArray();

            string shouldBeIgnored = directReferencesResult.Shuffle(9).First().Object.Name;
            string ignoredPackageName = shouldBeIgnored[..15] + "*" + shouldBeIgnored[20..];
            _ignoredPackages = _ignoredPackages!.Append(ignoredPackageName);

            _uut = new ReferencedPackageReader(_ignoredPackages,
                _msBuild.Object,
                _lockFileFactory.Object);
            IEnumerable<PackageIdentity> result = _uut.GetInstalledPackages(_projectPath, false);

            CollectionAssert.AreEquivalent(
                directReferencesResult.Where(l => l.Object.Name != shouldBeIgnored)
                    .Select(l =>
                        new PackageIdentity(l.Object.Name, l.Object.Version)),
                result);
        }

        [Test]
        public void GetInstalledPackages_Should_ReturnCorrectValues_If_NotIncludingTransitive_If_IgnoringPackagesWithWildCardAtEnd()
        {
            PackageReference[] directReferences = _packageReferencesFromProjectForFramework.SelectMany(p => p.Value)
                .Distinct()
                .ToArray();
            Mock<ILockFileLibrary>[] directReferencesResult = _lockFileLibraries.Where(l =>
                    directReferences.Any(e => e.PackageName.Equals(l.Object.Name)) &&
                    directReferences.Any(e => e.Version!.Equals(l.Object.Version)))
                .ToArray();

            string shouldBeIgnored = directReferencesResult.Shuffle(9).First().Object.Name;
            string ignoredPackageName = shouldBeIgnored[..30] + "*";
            _ignoredPackages = _ignoredPackages!.Append(ignoredPackageName);

            _uut = new ReferencedPackageReader(_ignoredPackages,
                _msBuild.Object,
                _lockFileFactory.Object);
            IEnumerable<PackageIdentity> result = _uut.GetInstalledPackages(_projectPath, false);

            CollectionAssert.AreEquivalent(
                directReferencesResult.Where(l => l.Object.Name != shouldBeIgnored)
                    .Select(l =>
                        new PackageIdentity(l.Object.Name, l.Object.Version)),
                result);
        }

        [Test]
        public void GetInstalledPackages_Should_ReturnCorrectValues_If_NotIncludingTransitive_If_IgnoringPackagesWithWildCardAtStartAndEnd()
        {
            PackageReference[] directReferences = _packageReferencesFromProjectForFramework.SelectMany(p => p.Value)
                .Distinct()
                .ToArray();
            Mock<ILockFileLibrary>[] directReferencesResult = _lockFileLibraries.Where(l =>
                    directReferences.Any(e => e.PackageName.Equals(l.Object.Name)) &&
                    directReferences.Any(e => e.Version!.Equals(l.Object.Version)))
                .ToArray();

            string shouldBeIgnored = directReferencesResult.Shuffle(9).First().Object.Name;
            string ignoredPackageName = "*" + shouldBeIgnored[5..30] + "*";
            _ignoredPackages = _ignoredPackages!.Append(ignoredPackageName);

            _uut = new ReferencedPackageReader(_ignoredPackages,
                _msBuild.Object,
                _lockFileFactory.Object);
            IEnumerable<PackageIdentity> result = _uut.GetInstalledPackages(_projectPath, false);

            CollectionAssert.AreEquivalent(
                directReferencesResult.Where(l => l.Object.Name != shouldBeIgnored)
                    .Select(l =>
                        new PackageIdentity(l.Object.Name, l.Object.Version)),
                result);
        }

        [Test]
        public void
            GetInstalledPackages_Should_ReturnEmptyCollection_When_ProjectHasNoPackageReferences_And_IsNotTransitive()
        {
            _projectMock.Setup(m => m.GetPackageReferenceCount()).Returns(0);
            _projectMock.Setup(m => m.GetEvaluatedIncludes()).Returns(Enumerable.Empty<string>());
            IEnumerable<PackageIdentity> result = _uut.GetInstalledPackages(_projectPath, false);

            Assert.That(result.Count(), Is.EqualTo(0));
        }
    }
}
