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
            IFixture fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _msBuild = Substitute.For<IMsBuildAbstraction>();
            _lockFileFactory = Substitute.For<ILockFileFactory>();
            _projectPath = fixture.Create<string>();
            _assetsFilePath = fixture.Create<string>();
            _projectMock = Substitute.For<IProject>();
            _lockFileMock = Substitute.For<ILockFile>();
            _packageSpecMock = Substitute.For<IPackageSpec>();
            _packagesConfigReader = Substitute.For<IPackagesConfigReader>();
            _lockFileTargets = fixture.CreateMany<ILockFileTarget>(TargetFrameworkCount).ToArray();
            _lockFileLibraries = fixture.CreateMany<ILockFileLibrary>(50).ToArray();
            _packageSpecTargetFrameworks =
                fixture.CreateMany<ITargetFrameworkInformation>(TargetFrameworkCount).ToArray();
            _targetFrameworks = fixture.CreateMany<INuGetFramework>(TargetFrameworkCount).ToArray();
            _packageReferencesFromProjectForFramework = new Dictionary<string, PackageReference[]>();

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
            _lockFileMock.Libraries.Returns(_lockFileLibraries);
            _packageSpecMock.TargetFrameworks.Returns(_packageSpecTargetFrameworks);

            _msBuild.GetPackageReferencesFromProjectForFramework(_projectMock, Arg.Any<string>())
                .Returns(args => _packageReferencesFromProjectForFramework[args.ArgAt<string>(1)].Select(p => p.PackageName));

            foreach (ILockFileLibrary lockFileLibrary in _lockFileLibraries)
            {
                INuGetVersion version = Substitute.For<INuGetVersion>();
                lockFileLibrary.Version.Returns(version);
                lockFileLibrary.Name.Returns(fixture.Create<string>());
            }

            foreach (INuGetFramework targetFramework in _targetFrameworks)
            {
                targetFramework.ToString().Returns(fixture.Create<string>());
            }

            foreach (INuGetFramework targetFramework in _targetFrameworks)
            {
                PackageReference[] returnedLibraries = _lockFileLibraries.Shuffle(75643)
                    .Take(5)
                    .Select(l => new PackageReference(l.Name, l.Version))
                    .ToArray();
                _packageReferencesFromProjectForFramework[targetFramework.ToString()!] = returnedLibraries;
            }

            using (IEnumerator<INuGetFramework> targetFrameworksIterator = _targetFrameworks.GetEnumerator())
            {
                foreach (ILockFileTarget lockFileTarget in _lockFileTargets)
                {
                    targetFrameworksIterator.MoveNext();
                    lockFileTarget.TargetFramework.Returns(targetFrameworksIterator.Current);
                }
            }

            using (IEnumerator<INuGetFramework> targetFrameworksIterator = _targetFrameworks.GetEnumerator())
            {
                foreach (ITargetFrameworkInformation packageSpecTargetFramework in _packageSpecTargetFrameworks)
                {
                    targetFrameworksIterator.MoveNext();
                    packageSpecTargetFramework.FrameworkName
                        .Returns(targetFrameworksIterator.Current);
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
        private Dictionary<string, PackageReference[]> _packageReferencesFromProjectForFramework = null!;

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
            GetInstalledPackages_Should_ThrowReferencedPackageReaderException_If_IncludingTransitive_And_PackageSpecFrameworkInformationGetFails()
        {
            _packageSpecMock.TargetFrameworks
                .Returns(Enumerable.Empty<ITargetFrameworkInformation>());
            IEnumerable<PackageIdentity> result = _uut.GetInstalledPackages(_projectPath, true);
            CollectionAssert.AreEquivalent(
                _lockFileLibraries.Select(l =>
                    new PackageIdentity(l.Name, l.Version)),
                result);
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
            CollectionAssert.AreEquivalent(
                _lockFileLibraries.Select(l =>
                    new PackageIdentity(l.Name, l.Version)),
                result);
        }

        [Test]
        public void GetInstalledPackages_Should_ReturnCorrectValues_If_NotIncludingTransitive()
        {
            IEnumerable<PackageIdentity> result = _uut.GetInstalledPackages(_projectPath, false);

            PackageReference[] expectedReferences = _packageReferencesFromProjectForFramework.SelectMany(p => p.Value)
                .Distinct()
                .ToArray();
            ILockFileLibrary[] expectedResult = _lockFileLibraries.Where(l =>
                    Array.Exists(expectedReferences, e => e.PackageName.Equals(l.Name)) &&
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
            PackageIdentity[] expectedPackages = _packageReferencesFromProjectForFramework.First().Value.Select(l => new PackageIdentity(l.PackageName, l.Version!)).ToArray();
            _packagesConfigReader.GetPackages(Arg.Any<IProject>()).Returns(expectedPackages);

            IEnumerable<PackageIdentity> packages = _uut.GetInstalledPackages(_projectPath, includeTransitive);

            CollectionAssert.AreEqual(expectedPackages, packages);
        }
    }
}
