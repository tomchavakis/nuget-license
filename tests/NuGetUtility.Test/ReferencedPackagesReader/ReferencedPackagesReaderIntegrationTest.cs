using NuGetUtility.ReferencedPackagesReader;
using NuGetUtility.Wrapper.MsBuildWrapper;
using NuGetUtility.Wrapper.NuGetWrapper.ProjectModel;
using NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types;
using NUnit.Framework;

namespace NuGetUtility.Test.ReferencedPackagesReader
{
    [TestFixture]
    public class ReferencedPackagesReaderIntegrationTest
    {
        [SetUp]
        public void SetUp()
        {
            _uut = new ReferencedPackageReader(Enumerable.Empty<string>(), new MsBuildAbstraction(),
                new LockFileFactory(), new PackageSearchMetadataBuilderFactory());
        }

        private ReferencedPackageReader _uut;

        [Test]
        public void GetInstalledPackagesShould_ReturnPackagesForActualProjectCorrectly()
        {
            var path = Path.GetFullPath("../../../../targets/PackageReferenceProject/PackageReferenceProject.csproj");

            var result = _uut.GetInstalledPackages(path, false);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetInstalledPackagesShould_ReturnTransitivePackages()
        {
            var path = Path.GetFullPath(
                "../../../../targets/ProjectWithTransitiveReferences/ProjectWithTransitiveReferences.csproj");

            var result = _uut.GetInstalledPackages(path, true);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetInstalledPackagesShould_ReturnEmptyEnumerableForProjectsWithoutPackages()
        {
            var path = Path.GetFullPath(
                "../../../../targets/ProjectWithoutNugetReferences/ProjectWithoutNugetReferences.csproj");

            var result = _uut.GetInstalledPackages(path, false);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetInstalledPackagesShould_ThrowMsBuildAbstractionException_If_ProjectUsesPackagesConfig()
        {
            var path = Path.GetFullPath("../../../../targets/PackagesConfigProject/PackagesConfigProject.csproj");

            Assert.That(() => _uut.GetInstalledPackages(path, false),
                Throws.TypeOf<MsBuildAbstractionException>().With.Message.EqualTo(
                    $"Invalid project structure detected. Currently only PackageReference projects are supported (Project: {path})"));
        }
    }
}
