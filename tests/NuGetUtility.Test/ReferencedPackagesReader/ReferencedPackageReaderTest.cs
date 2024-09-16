// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using AutoFixture;
using AutoFixture.AutoNSubstitute;
using NSubstitute;
using NuGetUtility.ReferencedPackagesReader;
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
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _msBuild = Substitute.For<IMsBuildAbstraction>();
            _lockFileFactory = Substitute.For<ILockFileFactory>();
            _projectPath = _fixture.Create<string>();
            _assetsFilePath = _fixture.Create<string>();
            _projectMock = Substitute.For<IProject>();
            _lockFileMock = Substitute.For<ILockFile>();
            _packageSpecMock = Substitute.For<IPackageSpec>();
            _packagesConfigReader = Substitute.For<IPackagesConfigReader>();
            _lockFileTargets = _fixture.CreateMany<ILockFileTarget>(TargetFrameworkCount).ToArray();
            _lockFileLibraries = _fixture.CreateMany<ILockFileLibrary>(50).ToArray();
            _packageSpecTargetFrameworks =
                _fixture.CreateMany<ITargetFrameworkInformation>(TargetFrameworkCount).ToArray();
            _targetFrameworks = _fixture.CreateMany<INuGetFramework>(TargetFrameworkCount).ToArray();
            _referencedPackagesForFramework = new Dictionary<INuGetFramework, PackageIdentity[]>();
            _directlyReferencedPackagesForFramework = new Dictionary<INuGetFramework, PackageIdentity[]>();

            _msBuild.GetProject(_projectPath).Returns(_projectMock);
            _projectMock.TryGetAssetsPath(out Arg.Any<string>()).Returns(args =>
            {
                args[0] = _assetsFilePath;
                return true;
            });
            _projectMock.FullPath.Returns(_projectPath);
            _lockFileFactory.GetFromFile(_assetsFilePath).Returns(_lockFileMock);
            _lockFileMock.PackageSpec.Returns(_packageSpecMock);
            _packageSpecMock.IsValid().Returns(true);
            _lockFileMock.Targets.Returns(_lockFileTargets);
            _packageSpecMock.TargetFrameworks.Returns(_packageSpecTargetFrameworks);

            var rnd = new Random(75643);
            foreach (ILockFileLibrary lockFileLibrary in _lockFileLibraries)
            {
                INuGetVersion version = Substitute.For<INuGetVersion>();
                lockFileLibrary.Version.Returns(version);
                lockFileLibrary.Name.Returns(_fixture.Create<string>());
            }

            foreach (INuGetFramework targetFramework in _targetFrameworks)
            {
                targetFramework.ToString().Returns(_fixture.Create<string>());
            }

            using (IEnumerator<INuGetFramework> targetFrameworksIterator = _targetFrameworks.GetEnumerator())
            {
                foreach (ILockFileTarget lockFileTarget in _lockFileTargets)
                {
                    targetFrameworksIterator.MoveNext();
                    lockFileTarget.TargetFramework.Returns(targetFrameworksIterator.Current);

                    ILockFileLibrary[] referencedLibraries = _lockFileLibraries.Shuffle(rnd)
                        .Take(5)
                        .ToArray();
                    _referencedPackagesForFramework[targetFrameworksIterator.Current] = referencedLibraries.Select(l => new PackageIdentity(l.Name, l.Version!)).ToArray();
                    lockFileTarget.Libraries.Returns(referencedLibraries);
                }
            }

            using (IEnumerator<INuGetFramework> targetFrameworksIterator = _targetFrameworks.GetEnumerator())
            {
                foreach (ITargetFrameworkInformation packageSpecTargetFramework in _packageSpecTargetFrameworks)
                {
                    targetFrameworksIterator.MoveNext();
                    packageSpecTargetFramework.FrameworkName
                        .Returns(targetFrameworksIterator.Current);

                    PackageIdentity[] directDependencies = _referencedPackagesForFramework[targetFrameworksIterator.Current].Shuffle(rnd)
                        .Take(2).ToArray();

                    _directlyReferencedPackagesForFramework[targetFrameworksIterator.Current] = directDependencies;
                    packageSpecTargetFramework.Dependencies.Returns(directDependencies.Select(l =>
                    {
                        ILibraryDependency sub = Substitute.For<ILibraryDependency>();
                        sub.Name.Returns(l.Id);
                        return sub;
                    }));
                }
            }

            _uut = new ReferencedPackageReader(_msBuild, _lockFileFactory, _packagesConfigReader);
        }

        private const int TargetFrameworkCount = 5;
        private ReferencedPackageReader _uut = null!;
        private IMsBuildAbstraction _msBuild = null!;
        private ILockFileFactory _lockFileFactory = null!;
        private IPackagesConfigReader _packagesConfigReader = null!;
        private string _projectPath = null!;
        private string _assetsFilePath = null!;
        private IProject _projectMock = null!;
        private ILockFile _lockFileMock = null!;
        private IPackageSpec _packageSpecMock = null!;
        private IEnumerable<ILockFileTarget> _lockFileTargets = null!;
        private IEnumerable<ILockFileLibrary> _lockFileLibraries = null!;
        private IEnumerable<ITargetFrameworkInformation> _packageSpecTargetFrameworks = null!;
        private IEnumerable<INuGetFramework> _targetFrameworks = null!;
        private IFixture _fixture = null!;
        private IDictionary<INuGetFramework, PackageIdentity[]> _referencedPackagesForFramework = null!;
        private IDictionary<INuGetFramework, PackageIdentity[]> _directlyReferencedPackagesForFramework = null!;

        [Test]
        public void GetInstalledPackages_Should_ThrowReferencedPackageReaderException_If_PackageSpecificationIsInvalid(
            [Values] bool includeTransitive)
        {
            _packageSpecMock.IsValid().Returns(false);
            _projectMock.FullPath.Returns(_projectPath);

            ReferencedPackageReaderException? exception = Assert.Throws<ReferencedPackageReaderException>(() =>
                _uut.GetInstalledPackages(_projectPath, includeTransitive));

            Assert.AreEqual($"Failed to validate project assets for project {_projectPath}", exception!.Message);
        }

        [Test]
        public void GetInstalledPackages_Should_ThrowReferencedPackageReaderException_If_TargetsArrayIsNull(
            [Values] bool includeTransitive)
        {
            _lockFileMock.Targets.Returns((IEnumerable<ILockFileTarget>?)null);

            ReferencedPackageReaderException? exception = Assert.Throws<ReferencedPackageReaderException>(() =>
                _uut.GetInstalledPackages(_projectPath, includeTransitive));

            Assert.AreEqual($"Failed to validate project assets for project {_projectPath}", exception!.Message);
        }

        [Test]
        public void
            GetInstalledPackages_Should_ThrowReferencedPackageReaderException_If_TargetsArrayDoesNotContainAnyElement(
                [Values] bool includeTransitive)
        {
            _lockFileMock.Targets.Returns(Enumerable.Empty<ILockFileTarget>());

            ReferencedPackageReaderException? exception = Assert.Throws<ReferencedPackageReaderException>(() =>
                _uut.GetInstalledPackages(_projectPath, includeTransitive));

            Assert.AreEqual($"Failed to validate project assets for project {_projectPath}", exception!.Message);
        }

        [Test]
        public void
            GetInstalledPackages_Should_ThrowReferencedPackageReaderException_If_NotIncludingTransitive_And_PackageSpecFrameworkInformationGetFails()
        {
            _packageSpecMock.TargetFrameworks
                .Returns(Enumerable.Empty<ITargetFrameworkInformation>());
            ReferencedPackageReaderException? exception = Assert.Throws<ReferencedPackageReaderException>(() =>
                _uut.GetInstalledPackages(_projectPath, false));

            Assert.AreEqual(
                $"Failed to identify the target framework information for {_lockFileTargets.First()}",
                exception!.Message);
            Assert.IsInstanceOf(typeof(InvalidOperationException), exception.InnerException);
            Assert.AreEqual("Sequence contains no matching element", exception.InnerException!.Message);
        }

        [Test]
        public void
            GetInstalledPackages_Should_ThrowReferencedPackageReaderException_If_Requested_FrameworkIsNotFound()
        {
            string targetFramework = _fixture.Create<string>();
            _packageSpecMock.TargetFrameworks
                .Returns(Enumerable.Empty<ITargetFrameworkInformation>());
            ReferencedPackageReaderException? exception = Assert.Throws<ReferencedPackageReaderException>(() =>
                _uut.GetInstalledPackages(_projectPath, false, targetFramework));

            Assert.AreEqual(
                $"Target framework {targetFramework} not found.",
                exception!.Message);
        }

        [Test]
        public void
            GetInstalledPackages_Should_ReturnCorrectValues_If_TargetFrameworks_Returns_Empty_And_Requested_Transitive_Packages()
        {
            _packageSpecMock.TargetFrameworks.Returns(Enumerable.Empty<ITargetFrameworkInformation>());
            IEnumerable<PackageIdentity> result = _uut.GetInstalledPackages(_projectPath, true);
            CollectionAssert.AreEquivalent(_referencedPackagesForFramework.SelectMany(kvp => kvp.Value).Distinct(), result);
        }

        [Test]
        public void GetInstalledPackages_Should_GetProjectFromPath([Values] bool includeTransitive)
        {
            _uut.GetInstalledPackages(_projectPath, includeTransitive);
            _msBuild.Received(1).GetProject(Arg.Any<string>());
            _msBuild.Received(1).GetProject(_projectPath);
        }

        [Test]
        public void GetInstalledPackages_Should_TryLoadAssetsFileFromProject([Values] bool includeTransitive)
        {
            _uut.GetInstalledPackages(_projectPath, includeTransitive);
            _projectMock.Received(1).TryGetAssetsPath(out Arg.Any<string>());
            _lockFileFactory.Received(1).GetFromFile(Arg.Any<string>());
            _lockFileFactory.Received(1).GetFromFile(_assetsFilePath);
        }

        [Test]
        public void GetInstalledPackages_Should_ReturnCorrectValues_If_IncludingTransitive()
        {
            IEnumerable<PackageIdentity> result = _uut.GetInstalledPackages(_projectPath, true);
            CollectionAssert.AreEquivalent(_referencedPackagesForFramework.SelectMany(kvp => kvp.Value).Distinct(), result);
        }

        [Test]
        public void GetInstalledPackages_Should_OnlyReturnPackages_For_TargetFramework()
        {
            string name = _fixture.Create<string>();
            INuGetFramework targetFramework = _targetFrameworks.Shuffle(new Random(69843456)).First();
            targetFramework.Equals(name).Returns(true);
            IEnumerable<PackageIdentity> result = _uut.GetInstalledPackages(_projectPath, true, name);
            CollectionAssert.AreEquivalent(_referencedPackagesForFramework[targetFramework], result);
        }

        [Test]
        public void GetInstalledPackages_Should_ReturnCorrectValues_If_NotIncludingTransitive()
        {
            IEnumerable<PackageIdentity> result = _uut.GetInstalledPackages(_projectPath, false);

            PackageIdentity[] expectedReferences = _directlyReferencedPackagesForFramework.SelectMany(p => p.Value)
                .Distinct()
                .ToArray();
            ILockFileLibrary[] expectedResult = _lockFileLibraries.Where(l =>
                    Array.Exists(expectedReferences, e => e.Id.Equals(l.Name)) &&
                    Array.Exists(expectedReferences, e => e.Version!.Equals(l.Version)))
                .ToArray();
            CollectionAssert.AreEquivalent(
                expectedResult.Select(l =>
                    new PackageIdentity(l.Name, l.Version)),
                result);
        }

        [Test]
        public void
            GetInstalledPackages_Should_ReturnEmptyCollection_If_Cannot_Get_Asset_File_Path_And_Has_No_Packages_Config()
        {
            _projectMock.TryGetAssetsPath(out Arg.Any<string>()).Returns(false);
            _projectMock.GetEvaluatedIncludes().Returns(Enumerable.Empty<string>());
            IEnumerable<PackageIdentity> result = _uut.GetInstalledPackages(_projectPath, false);

            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public void GetInstalledPackages_Should_Use_PackageGonfigReader_If_ProjectIsPackageConfigProject(
            [Values] bool includeTransitive)
        {
            _projectMock.TryGetAssetsPath(out Arg.Any<string>()).Returns(false);
            _projectMock.FullPath.Returns(_projectPath);
            _projectMock.GetEvaluatedIncludes().Returns(new List<string> { "packages.config" });

            _ = _uut.GetInstalledPackages(_projectPath, includeTransitive);

            _packagesConfigReader.Received(1).GetPackages(Arg.Any<IProject>());
            _packagesConfigReader.Received(1).GetPackages(_projectMock);
        }

        [Test]
        public void GetInstalledPackages_Should_ReturnPackagesReturnedBy_PackageGonfigReader_If_ProjectIsPackageConfigProject(
            [Values] bool includeTransitive)
        {
            _projectMock.TryGetAssetsPath(out Arg.Any<string>()).Returns(false);
            _projectMock.FullPath.Returns(_projectPath);
            _projectMock.GetEvaluatedIncludes().Returns(new List<string> { "packages.config" });
            PackageIdentity[] expectedPackages = _referencedPackagesForFramework.First().Value;
            _packagesConfigReader.GetPackages(Arg.Any<IProject>()).Returns(expectedPackages);

            IEnumerable<PackageIdentity> packages = _uut.GetInstalledPackages(_projectPath, includeTransitive);

            CollectionAssert.AreEqual(expectedPackages, packages);
        }
    }
}
