using AutoFixture;
using NSubstitute;
using NuGetUtility.ReferencedPackagesReader;
using NuGetUtility.Test.Helper.ShuffelledEnumerable;
using NuGetUtility.Wrapper.MsBuildWrapper;

namespace NuGetUtility.Test.ReferencedPackagesReader
{
    [TestFixture]
    public class ProjectsCollectorTest
    {

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _msBuild = Substitute.For<IMsBuildAbstraction>();
            _uut = new ProjectsCollector(_msBuild);
        }
        private IMsBuildAbstraction _msBuild = null!;
        private ProjectsCollector _uut = null!;
        private Fixture _fixture = null!;

        [TestCase("A.csproj")]
        [TestCase("B.fsproj")]
        [TestCase("C.vbproj")]
        [TestCase("D.dbproj")]
        public void GetProjects_Should_ReturnProjectsAsListDirectly(string projectFile)
        {
            IEnumerable<string> result = _uut.GetProjects(projectFile);
            CollectionAssert.AreEqual(new[] { projectFile }, result);
            _msBuild.DidNotReceive().GetProjectsFromSolution(Arg.Any<string>());
        }

        [TestCase("A.sln")]
        [TestCase("B.sln")]
        [TestCase("C.sln")]
        public void GetProjects_Should_QueryMsBuildToGetProjectsForSolutionFiles(string solutionFile)
        {
            _ = _uut.GetProjects(solutionFile);

            _msBuild.Received(1).GetProjectsFromSolution(solutionFile);
        }

        [TestCase("A.sln")]
        [TestCase("B.sln")]
        [TestCase("C.sln")]
        public void GetProjects_Should_ReturnEmptyArray_If_SolutionContainsNoProjects(string solutionFile)
        {
            _msBuild.GetProjectsFromSolution(Arg.Any<string>()).Returns(Enumerable.Empty<string>());

            IEnumerable<string> result = _uut.GetProjects(solutionFile);
            CollectionAssert.AreEqual(Array.Empty<string>(), result);

            _msBuild.Received(1).GetProjectsFromSolution(solutionFile);
        }

        [TestCase("A.sln")]
        [TestCase("B.sln")]
        [TestCase("C.sln")]
        public void GetProjects_Should_ReturnEmptyArray_If_SolutionContainsProjectsThatDontExist(string solutionFile)
        {
            IEnumerable<string> projects = _fixture.CreateMany<string>();
            _msBuild.GetProjectsFromSolution(Arg.Any<string>()).Returns(projects);

            IEnumerable<string> result = _uut.GetProjects(solutionFile);
            CollectionAssert.AreEqual(Array.Empty<string>(), result);

            _msBuild.Received(1).GetProjectsFromSolution(solutionFile);
        }

        [TestCase("A.sln")]
        [TestCase("B.sln")]
        [TestCase("C.sln")]
        public void GetProjects_Should_ReturnArrayOfProjects_If_SolutionContainsProjectsThatDoExist(string solutionFile)
        {
            string[] projects = _fixture.CreateMany<string>().ToArray();
            CreateFiles(projects);
            _msBuild.GetProjectsFromSolution(Arg.Any<string>()).Returns(projects);

            IEnumerable<string> result = _uut.GetProjects(solutionFile);
            CollectionAssert.AreEqual(projects, result);

            _msBuild.Received(1).GetProjectsFromSolution(solutionFile);
        }

        [TestCase("A.sln")]
        [TestCase("B.sln")]
        [TestCase("C.sln")]
        public void GetProjects_Should_ReturnOnlyExistingProjectsInSolutionFile(string solutionFile)
        {
            string[] existingProjects = _fixture.CreateMany<string>().ToArray();
            IEnumerable<string> missingProjects = _fixture.CreateMany<string>();

            CreateFiles(existingProjects);

            _msBuild.GetProjectsFromSolution(Arg.Any<string>())
                .Returns(existingProjects.Concat(missingProjects).Shuffle(54321));

            IEnumerable<string> result = _uut.GetProjects(solutionFile);
            CollectionAssert.AreEquivalent(existingProjects, result);

            _msBuild.Received(1).GetProjectsFromSolution(solutionFile);
        }

        [Test]
        public void GetProjectsFromSolution_Should_ReturnProjectsInActualSolutionFileRelativePath()
        {
            var msbuild = new MsBuildAbstraction();
            IEnumerable<string> result = msbuild.GetProjectsFromSolution("../../../../targets/Projects.sln");
            Assert.AreEqual(5, result.Count());
        }

        [Test]
        public void GetProjectsFromSolution_Should_ReturnProjectsInActualSolutionFileAbsolutePath()
        {
            var msbuild = new MsBuildAbstraction();
            IEnumerable<string> result = msbuild.GetProjectsFromSolution(Path.GetFullPath("../../../../targets/Projects.sln"));
            Assert.AreEqual(5, result.Count());
        }

        private void CreateFiles(IEnumerable<string> files)
        {
            foreach (string file in files)
            {
                File.WriteAllBytes(file, Array.Empty<byte>());
            }
        }
    }
}
