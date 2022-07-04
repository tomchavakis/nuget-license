using Moq;
using NuGetUtility.Extensions;
using NuGetUtility.Wrapper.MsBuildWrapper;
using NUnit.Framework;

namespace NuGetUtility.Test.Extensions
{
    [TestFixture]
    public class ProjectExtensionsTest
    {
        [SetUp]
        public void SetUp()
        {
            _projectMock = new Mock<IProject>();
            _uut = _projectMock.Object;
        }

        private Mock<IProject>? _projectMock;

        private IProject? _uut;

        [Test]
        public void HasNugetPackagesReferenced_Should_ReturnTrue_If_PackageReferenceCountIsMoreThanZero(
            [Values(1, 50, 999)] int referenceCount)
        {
            _projectMock!.Setup(m => m.GetPackageReferenceCount()).Returns(referenceCount);

            var result = _uut!.HasNugetPackagesReferenced();

            Assert.That(result, Is.True);
        }

        [Test]
        public void HasNugetPackagesReferences_Should_ReturnTrue_If_ProjectHasPackagesConfigFileReferenced()
        {
            _projectMock!.Setup(m => m.GetPackageReferenceCount()).Returns(0);
            _projectMock.Setup(m => m.GetEvaluatedIncludes()).Returns(new List<string> { "packages.config" });

            var result = _uut!.HasNugetPackagesReferenced();

            Assert.That(result, Is.True);
        }

        [Test]
        public void
            HasNugetPackagesReferenced_Should_ReturnFalse_If_PackageReferenceCountIsZeroOrLess_And_ProjectHasNoPackagesConfigFileReferenced(
                [Values(-9999, -50, 0)] int referenceCount)
        {
            _projectMock!.Setup(m => m.GetPackageReferenceCount()).Returns(referenceCount);
            _projectMock.Setup(m => m.GetEvaluatedIncludes()).Returns(Enumerable.Empty<string>());

            var result = _uut!.HasNugetPackagesReferenced();

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
            string nugetStyleTag, string restoreStyleTag)
        {
            _projectMock!.Setup(m => m.GetNugetStyleTag()).Returns(nugetStyleTag);
            _projectMock.Setup(m => m.GetRestoreStyleTag()).Returns(restoreStyleTag);
            _projectMock.Setup(m => m.GetEvaluatedIncludes()).Returns(new List<string> { "not-packages.config" });

            var result = _uut!.IsPackageReferenceProject();

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
            string nugetStyleTag, string restoreStyleTag, string evaluatedInclude)
        {
            _projectMock!.Setup(m => m.GetNugetStyleTag()).Returns(nugetStyleTag);
            _projectMock.Setup(m => m.GetRestoreStyleTag()).Returns(restoreStyleTag);
            _projectMock.Setup(m => m.GetEvaluatedIncludes()).Returns(new List<string> { evaluatedInclude });

            var result = _uut!.IsPackageReferenceProject();

            Assert.That(result, Is.False);
        }
    }
}
