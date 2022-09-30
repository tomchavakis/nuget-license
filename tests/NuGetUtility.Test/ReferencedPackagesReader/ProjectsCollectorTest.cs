using AutoFixture;
using Moq;
using NuGetUtility.ReferencedPackagesReader;
using NuGetUtility.Test.Helper.ShuffelledEnumerable;
using NuGetUtility.Wrapper.MsBuildWrapper;
using NUnit.Framework;

namespace NuGetUtility.Test.ReferencedPackagesReader
{
    [TestFixture]
    public class ProjectsCollectorTest
    {

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _msBuild = new Mock<IMsBuildAbstraction>();
            _uut = new ProjectsCollector(_msBuild.Object);
        }
        private Mock<IMsBuildAbstraction> _msBuild = null!;
        private ProjectsCollector _uut = null!;
        private Fixture _fixture = null!;

        [TestCase("A.csproj")]
        [TestCase("B.fsproj")]
        [TestCase("C.vbproj")]
        [TestCase("D.dbproj")]
        public void GetProjects_Should_ReturnProjectsAsListDirectly(string projectFile)
        {
            var result = _uut.GetProjects(projectFile);
            CollectionAssert.AreEqual(new[] { projectFile }, result);
            _msBuild.Verify(m => m.GetProjectsFromSolution(It.IsAny<string>()), Times.Never);
        }

        [TestCase("A.sln")]
        [TestCase("B.sln")]
        [TestCase("C.sln")]
        public void GetProjects_Should_QueryMsBuildToGetProjectsForSolutionFiles(string solutionFile)
        {
            _ = _uut.GetProjects(solutionFile);

            _msBuild.Verify(m => m.GetProjectsFromSolution(solutionFile), Times.Once);
        }

        [TestCase("A.sln")]
        [TestCase("B.sln")]
        [TestCase("C.sln")]
        public void GetProjects_Should_ReturnEmptyArray_If_SolutionContainsNoProjects(string solutionFile)
        {
            _msBuild.Setup(m => m.GetProjectsFromSolution(It.IsAny<string>())).Returns(Enumerable.Empty<string>());

            var result = _uut.GetProjects(solutionFile);
            CollectionAssert.AreEqual(new string[] { }, result);

            _msBuild.Verify(m => m.GetProjectsFromSolution(solutionFile), Times.Once);
        }

        [TestCase("A.sln")]
        [TestCase("B.sln")]
        [TestCase("C.sln")]
        public void GetProjects_Should_ReturnEmptyArray_If_SolutionContainsProjectsThatDontExist(string solutionFile)
        {
            var projects = _fixture.CreateMany<string>();
            _msBuild.Setup(m => m.GetProjectsFromSolution(It.IsAny<string>())).Returns(projects);

            var result = _uut.GetProjects(solutionFile);
            CollectionAssert.AreEqual(new string[] { }, result);

            _msBuild.Verify(m => m.GetProjectsFromSolution(solutionFile), Times.Once);
        }

        [TestCase("A.sln")]
        [TestCase("B.sln")]
        [TestCase("C.sln")]
        public void GetProjects_Should_ReturnArrayOfProjects_If_SolutionContainsProjectsThatDoExist(string solutionFile)
        {
            var projects = _fixture.CreateMany<string>().ToArray();
            CreateFiles(projects);
            _msBuild.Setup(m => m.GetProjectsFromSolution(It.IsAny<string>())).Returns(projects);

            var result = _uut.GetProjects(solutionFile);
            CollectionAssert.AreEqual(projects, result);

            _msBuild.Verify(m => m.GetProjectsFromSolution(solutionFile), Times.Once);
        }

        [TestCase("A.sln")]
        [TestCase("B.sln")]
        [TestCase("C.sln")]
        public void GetProjects_Should_ReturnOnlyExistingProjectsInSolutionFile(string solutionFile)
        {
            var existingProjects = _fixture.CreateMany<string>().ToArray();
            var missingProjects = _fixture.CreateMany<string>();

            CreateFiles(existingProjects);

            _msBuild.Setup(m => m.GetProjectsFromSolution(It.IsAny<string>()))
                .Returns(existingProjects.Concat(missingProjects).Shuffle());

            var result = _uut.GetProjects(solutionFile);
            CollectionAssert.AreEquivalent(existingProjects, result);

            _msBuild.Verify(m => m.GetProjectsFromSolution(solutionFile), Times.Once);
        }

        private void CreateFiles(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                File.WriteAllBytes(file, Array.Empty<byte>());
            }
        }
    }
}
