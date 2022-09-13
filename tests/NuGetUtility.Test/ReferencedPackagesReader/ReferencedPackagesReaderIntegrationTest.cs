﻿using NuGetUtility.ReferencedPackagesReader;
using NuGetUtility.Wrapper.MsBuildWrapper;
using NuGetUtility.Wrapper.NuGetWrapper.ProjectModel;
using NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types;

namespace NuGetUtility.Test.ReferencedPackagesReader
{
    [TestFixture]
    public class ReferencedPackagesReaderIntegrationTest
    {
        [SetUp]
        public void SetUp()
        {
            _uut = new ReferencedPackageReader(Enumerable.Empty<string>(),
                new MsBuildAbstraction(),
                new LockFileFactory(),
                new PackageSearchMetadataBuilderFactory());
        }

        private ReferencedPackageReader? _uut;

        [Test]
        public void GetInstalledPackagesShould_ReturnPackagesForActualProjectCorrectly()
        {
            var path = Path.GetFullPath("../../../../targets/PackageReferenceProject/PackageReferenceProject.csproj");

            var result = _uut!.GetInstalledPackages(path, false);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetInstalledPackagesShould_ReturnTransitivePackages()
        {
            var path = Path.GetFullPath(
                "../../../../targets/ProjectWithTransitiveReferences/ProjectWithTransitiveReferences.csproj");

            var result = _uut!.GetInstalledPackages(path, true);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetInstalledPackagesShould_ReturnTransitiveNuget()
        {
            var path = Path.GetFullPath(
                "../../../../targets/ProjectWithTransitiveNuget/ProjectWithTransitiveNuget.csproj");

            var result = _uut!.GetInstalledPackages(path, true).ToArray();

            Assert.That(result.Count, Is.EqualTo(3));
            var titles = result.Select(metadata => metadata.Title).ToArray();
            Assert.That(titles.Contains("Moq"), Is.True);
            Assert.That(titles.Contains("Castle.Core"), Is.True);
            Assert.That(titles.Contains("System.Diagnostics.EventLog"), Is.True);
        }

        [Test]
        public void GetInstalledPackagesShould_ReturnEmptyEnumerableForProjectsWithoutPackages()
        {
            var path = Path.GetFullPath(
                "../../../../targets/ProjectWithoutNugetReferences/ProjectWithoutNugetReferences.csproj");

            var result = _uut!.GetInstalledPackages(path, false);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        [Platform(Include = "Win")]
        public void GetInstalledPackagesShould_ThrowMsBuildAbstractionException_If_ProjectUsesPackagesConfig()
        {
            var path = Path.GetFullPath("../../../../targets/PackagesConfigProject/PackagesConfigProject.csproj");

            var exception = Assert.Throws<MsBuildAbstractionException>(() => _uut!.GetInstalledPackages(path, false));
            Assert.AreEqual(
                $"Invalid project structure detected. Currently only PackageReference projects are supported (Project: {path})",
                exception?.Message);
        }
    }
}
