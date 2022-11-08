using NuGetUtility.ReferencedPackagesReader;
using NuGetUtility.Wrapper.MsBuildWrapper;
using NuGetUtility.Wrapper.NuGetWrapper.ProjectModel;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;

namespace NuGetUtility.Test.ReferencedPackagesReader
{
    [TestFixture]
    public class ReferencedPackagesReaderIntegrationTest
    {
        [SetUp]
        public void SetUp()
        {
            _uut = new ReferencedPackageReader(new MsBuildAbstraction(), new LockFileFactory());
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
            Assert.That(titles.Contains("Moq"), Is.True);
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
        public void GetInstalledPackagesShould_ThrowMsBuildAbstractionException_If_ProjectUsesPackagesConfig()
        {
            string path = Path.GetFullPath("../../../../targets/PackagesConfigProject/PackagesConfigProject.csproj");

            MsBuildAbstractionException? exception = Assert.Throws<MsBuildAbstractionException>(() => _uut!.GetInstalledPackages(path, false));
            Assert.AreEqual(
                $"Invalid project structure detected. Currently only PackageReference projects are supported (Project: {path})",
                exception?.Message);
        }
    }
}
