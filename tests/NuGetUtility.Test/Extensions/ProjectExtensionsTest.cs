// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

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

        [TestCase]
        public void GetPackagesConfigPath_Should_Return_CorrectPath()
        {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            _project.FullPath.Returns(path);

            string result = _project.GetPackagesConfigPath();

            Assert.That(result, Is.EqualTo(Path.Combine(Path.GetDirectoryName(path)!, "packages.config")));
        }

        [TestCase(new string?[] { }, false)]
        [TestCase(new string?[] { null }, false)]
        [TestCase(new string?[] { null, "not-packages.config" }, false)]
        [TestCase(new string?[] { "not-packages.config" }, false)]
        [TestCase(new string?[] { "packages.config" }, true)]
        [TestCase(new string?[] { null, "packages.config" }, true)]
        [TestCase(new string?[] { "not-packages.config", "packages.config" }, true)]
        [TestCase(new string?[] { null, "not-packages.config", "packages.config" }, true)]
        public void HasPackagesConfigFile_Should_Return_Correct_Result(IEnumerable<string> evaluatedIncludes, bool expectation)
        {
            _project.GetEvaluatedIncludes().Returns(evaluatedIncludes);

            Assert.That(_project.HasPackagesConfigFile(), Is.EqualTo(expectation));
        }
    }
}
