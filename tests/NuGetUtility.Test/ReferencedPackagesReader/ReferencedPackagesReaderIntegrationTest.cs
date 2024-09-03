// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NuGetUtility.ReferencedPackagesReader;
using NuGetUtility.Wrapper.MsBuildWrapper;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;
using NuGetUtility.Wrapper.NuGetWrapper.ProjectModel;

namespace NuGetUtility.Test.ReferencedPackagesReader
{
    [TestFixture, FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public class ReferencedPackagesReaderIntegrationTest
    {
        [SetUp]
        public void SetUp()
        {
#if NETFRAMEWORK
            IPackagesConfigReader packagesConfigReader = new WindowsPackagesConfigReader();
#else
            IPackagesConfigReader packagesConfigReader = OperatingSystem.IsWindows() ? new WindowsPackagesConfigReader() : new FailingPackagesConfigReader();
#endif

            _uut = new ReferencedPackageReader(new MsBuildAbstraction(), new LockFileFactory(), packagesConfigReader);
        }

        private ReferencedPackageReader? _uut;

        [Test]
        public void GetInstalledPackagesShould_ReturnPackagesForActualProjectCorrectly()
        {
            string path = Path.GetFullPath("../../../../targets/PackageReferenceProject/PackageReferenceProject.csproj");

            IEnumerable<PackageIdentity> result = _uut!.GetInstalledPackages(path, false);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetInstalledPackagesShould_ReturnTransitivePackages()
        {
            string path = Path.GetFullPath(
                "../../../../targets/ProjectWithTransitiveReferences/ProjectWithTransitiveReferences.csproj");

            IEnumerable<PackageIdentity> result = _uut!.GetInstalledPackages(path, true);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetInstalledPackagesShould_ReturnTransitiveNuGet()
        {
            string path = Path.GetFullPath(
                "../../../../targets/ProjectWithTransitiveNuget/ProjectWithTransitiveNuget.csproj");

            PackageIdentity[] result = _uut!.GetInstalledPackages(path, true).ToArray();

            Assert.That(result.Count, Is.EqualTo(3));
            string[] titles = result.Select(metadata => metadata.Id).ToArray();
            Assert.That(titles.Contains("NSubstitute"), Is.True);
            Assert.That(titles.Contains("Castle.Core"), Is.True);
            Assert.That(titles.Contains("System.Diagnostics.EventLog"), Is.True);
        }

        [Test]
        public void GetInstalledPackagesShould_ReturnEmptyEnumerable_For_ProjectsWithoutPackages()
        {
            string path = Path.GetFullPath(
                "../../../../targets/ProjectWithoutNugetReferences/ProjectWithoutNugetReferences.csproj");

            IEnumerable<PackageIdentity> result = _uut!.GetInstalledPackages(path, false);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetInstalledPackagesShould_ReturnResolvedDependency_For_ProjectWithRangedDependencies([Values] bool includeTransitive)
        {
            string path = Path.GetFullPath(
                "../../../../targets/VersionRangesProject/VersionRangesProject.csproj");

            IEnumerable<PackageIdentity> result = _uut!.GetInstalledPackages(path, includeTransitive);

            Assert.That(result.Count, Is.EqualTo(includeTransitive ? 2 : 1));
        }

        [Test]
        [Platform(Include = "Win")]
        public void GetInstalledPackagesShould_ReturnPackages_For_PackagesConfigProject()
        {
            string path = Path.GetFullPath("../../../../targets/PackagesConfigProject/PackagesConfigProject.csproj");

            IEnumerable<PackageIdentity> result = _uut!.GetInstalledPackages(path, false);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        [Platform(Exclude = "Win")]
        public void GetInstalledPackagesShould_ThrowError_PackagesConfigProject()
        {
            string path = Path.GetFullPath("../../../../targets/PackagesConfigProject/PackagesConfigProject.csproj");

            PackagesConfigReaderException? exception = Assert.Throws<PackagesConfigReaderException>(() => _uut!.GetInstalledPackages(path, false));
            Assert.That(exception?.Message, Is.EqualTo($"Invalid project structure detected. Currently packages.config projects are only supported on Windows (Project: {path})"));
        }

#if NETFRAMEWORK
        [TestCase(true)]
        [TestCase(false)]
        public void GetInstalledPackagesShould_ReturnPackages_For_NativeCppProject_With_References(bool includeTransitive)
        {
            string path = Path.GetFullPath("../../../../targets/SimpleCppProject/SimpleCppProject.vcxproj");

            IEnumerable<PackageIdentity> result = _uut!.GetInstalledPackages(path, includeTransitive);

            Assert.That(result.Count, Is.EqualTo(2));
        }
#else
        [TestCase(true)]
        [TestCase(false)]
        public void GetInstalledPackagesShould_ThrowError_For_PackagesForNativeCppProject_With_References(bool includeTransitive)
        {
            string path = Path.GetFullPath("../../../../targets/SimpleCppProject/SimpleCppProject.vcxproj");

            MsBuildAbstractionException? exception = Assert.Throws<MsBuildAbstractionException>(() => _uut!.GetInstalledPackages(path, includeTransitive));
            Assert.That(exception?.Message, Is.EqualTo($"Please use the .net Framework version to analyze c++ projects (Project: {path})"));
        }
#endif

#if NETFRAMEWORK
        [TestCase(true)]
        [TestCase(false)]
        public void GetInstalledPackagesShould_ReturnPackages_For_NativeCppProject_Without_References(bool includeTransitive)
        {
            string path = Path.GetFullPath("../../../../targets/EmptyCppProject/EmptyCppProject.vcxproj");

            IEnumerable<PackageIdentity> result = _uut!.GetInstalledPackages(path, includeTransitive);

            Assert.That(result.Count, Is.EqualTo(0));
        }
#else
        [TestCase(true)]
        [TestCase(false)]
        public void GetInstalledPackagesShould_ThrowError_For_PackagesForNativeCppProject_Without_References(bool includeTransitive)
        {
            string path = Path.GetFullPath("../../../../targets/EmptyCppProject/EmptyCppProject.vcxproj");

            MsBuildAbstractionException? exception = Assert.Throws<MsBuildAbstractionException>(() => _uut!.GetInstalledPackages(path, includeTransitive));
            Assert.That(exception?.Message, Is.EqualTo($"Please use the .net Framework version to analyze c++ projects (Project: {path})"));
        }
#endif
    }
}
