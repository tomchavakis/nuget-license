using Microsoft.Build.Evaluation;
using NuGetUtility.ReferencedPackagesReader;
using NuGetUtility.Wrapper.MsBuildWrapper;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;
using NuGetUtility.Wrapper.NuGetWrapper.ProjectModel;

namespace NuGetUtility.Test.ReferencedPackagesReader
{
    [TestFixture]
    public class ReferencedPackagesReaderIntegrationTest
    {
        [SetUp]
        public void SetUp()
        {
            IMsBuildAbstraction msBuildAbstraction = OperatingSystem.IsWindows() ? new WindowsMsBuildAbstraction() : new MsBuildAbstraction();
            IPackagesConfigReader packagesConfigReader = OperatingSystem.IsWindows() ? new WindowsPackagesConfigReader() : new FailingPackagesConfigReader();

            _uut = new ReferencedPackageReader(msBuildAbstraction, new LockFileFactory(), packagesConfigReader);
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
        public void GetInstalledPackagesShould_ReturnTransitiveNuget()
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
        public void GetInstalledPackagesShould_ReturnEmptyEnumerableForProjectsWithoutPackages()
        {
            string path = Path.GetFullPath(
                "../../../../targets/ProjectWithoutNugetReferences/ProjectWithoutNugetReferences.csproj");

            IEnumerable<PackageIdentity> result = _uut!.GetInstalledPackages(path, false);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        [Platform(Include = "Win")]
        public void GetInstalledPackagesShould_ReturnPackagesForPackagesConfigProject()
        {
            string path = Path.GetFullPath("../../../../targets/PackagesConfigProject/PackagesConfigProject.csproj");

            IEnumerable<PackageIdentity> result = _uut!.GetInstalledPackages(path, false);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        [Platform(Exclude = "Win")]
        public void GetInstalledPackagesShould_ThrowError()
        {
            string path = Path.GetFullPath("../../../../targets/PackagesConfigProject/PackagesConfigProject.csproj");

            PackagesConfigReaderException? exception = Assert.Throws<PackagesConfigReaderException>(() => _uut!.GetInstalledPackages(path, false));
            Assert.That(exception?.Message, Is.EqualTo($"Invalid project structure detected. Currently packages.config projects are only supported on Windows (Project: {path})"));
        }
    }
}
