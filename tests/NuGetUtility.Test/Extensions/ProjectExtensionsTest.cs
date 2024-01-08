using NSubstitute;
using NuGetUtility.Extensions;
using NuGetUtility.Wrapper.MsBuildWrapper;

namespace NuGetUtility.Test.Extensions
{
    [TestFixture]
    public class ProjectExtensionsTest
    {
        [SetUp]
        public void SetUp()
        {
            _project = Substitute.For<IProject>();
        }

        private IProject _project = null!;

        [Test]
        public void HasNugetPackagesReferenced_Should_ReturnTrue_If_PackageReferenceCountIsMoreThanZero(
            [Values(1, 50, 999)] int referenceCount)
        {
            _project.GetPackageReferenceCount().Returns(referenceCount);

            bool result = _project.HasNugetPackagesReferenced();

            Assert.That(result, Is.True);
        }

        [Test]
        public void HasNugetPackagesReferences_Should_ReturnTrue_If_ProjectHasPackagesConfigFileReferenced()
        {
            _project.GetPackageReferenceCount().Returns(0);
            _project.GetEvaluatedIncludes().Returns(new List<string> { "packages.config" });

            bool result = _project!.HasNugetPackagesReferenced();

            Assert.That(result, Is.True);
        }

        [Test]
        public void
            HasNugetPackagesReferenced_Should_ReturnFalse_If_PackageReferenceCountIsZeroOrLess_And_ProjectHasNoPackagesConfigFileReferenced(
                [Values(-9999, -50, 0)] int referenceCount)
        {
            _project.GetPackageReferenceCount().Returns(referenceCount);
            _project.GetEvaluatedIncludes().Returns(Enumerable.Empty<string>());

            bool result = _project!.HasNugetPackagesReferenced();

            Assert.That(result, Is.False);
        }

        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase(null, "PackageReference")]
        [TestCase("", "PackageReference")]
        [TestCase("PackageReference", null)]
        [TestCase("PackageReference", "")]
        [TestCase("PackageReference", "PackageReference")]
        public void IsPackageReferenceProject_Should_ReturnTrue_If_ProjectIsPackageReferenceProject(
            string nugetStyleTag,
            string restoreStyleTag)
        {
            _project.GetNugetStyleTag().Returns(nugetStyleTag);
            _project.GetRestoreStyleTag().Returns(restoreStyleTag);
            _project.GetEvaluatedIncludes().Returns(new List<string> { "not-packages.config" });

            bool result = _project.IsPackageReferenceProject();

            Assert.That(result, Is.True);
        }

        [TestCase("InvalidTag", "InvalidTag", "packages.config")]
        [TestCase("InvalidTag", "PackageReference", "packages.config")]
        [TestCase("InvalidTag", "InvalidTag", "not-packages.config")]
        [TestCase("InvalidTag", "PackageReference", "not-packages.config")]
        [TestCase("PackageReference", "InvalidTag", "packages.config")]
        [TestCase("PackageReference", "PackageReference", "packages.config")]
        [TestCase("PackageReference", "InvalidTag", "not-packages.config")]
        public void IsPackageReferenceProject_Should_ReturnFalse_If_ProjectIsNotPackageReferenceProject(
            string nugetStyleTag,
            string restoreStyleTag,
            string evaluatedInclude)
        {
            _project.GetNugetStyleTag().Returns(nugetStyleTag);
            _project.GetRestoreStyleTag().Returns(restoreStyleTag);
            _project.GetEvaluatedIncludes().Returns(new List<string> { evaluatedInclude });

            bool result = _project.IsPackageReferenceProject();

            Assert.That(result, Is.False);
        }

        [TestCase()]
        public void GetPackagesConfigPath_Should_Return_CorrectPath()
        {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            _project.FullPath.Returns(path);

            string result = _project.GetPackagesConfigPath();

            Assert.That(result, Is.EqualTo(Path.Combine(Path.GetDirectoryName(path)!, "packages.config")));
        }
    }
}
