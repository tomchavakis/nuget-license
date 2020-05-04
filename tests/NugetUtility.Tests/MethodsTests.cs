using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace NugetUtility.Tests
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class MethodTests
    {
        private string _projectPath;
        private Methods _methods;

        [SetUp]
        public void Setup()
        {
            _projectPath = @"../../../";
            _methods = new Methods(new PackageOptions { ProjectDirectory = _projectPath });
        }

        [Test]
        public void GetProjectExtension_Should_Be_Csproj()
        {
            Assert.AreEqual(".csproj", _methods.GetProjectExtension());
        }

        [Test]
        public void GetProjectReferences_Should_Resolve_Projects()
        {
            var packages = _methods.GetProjectReferences(_projectPath);

            CollectionAssert.IsNotEmpty(packages);
        }

        [Test]
        public async Task GetNugetInformationAsync_Should_Resolve_Projects()
        {
            var packages = _methods.GetProjectReferences(_projectPath);
            var information = await _methods.GetNugetInformationAsync(_projectPath, packages);

            CollectionAssert.AreEqual
            (
                packages.Select(x => x.Split(',')[0].ToLower()),
                information.Select(x => x.Value.Metadata.Id.ToLower())
            );
        }

        [TestCase("FluentValidation,5.1.0.0")]
        [Test]
        public async Task GetNugetInformationAsync_Should_Resolve_Missing_NuSpec_File(string package)
        {
            var packages = package.Split(';', System.StringSplitOptions.RemoveEmptyEntries);
            var information = await _methods.GetNugetInformationAsync(_projectPath, packages);

            CollectionAssert.AreEqual
            (
                packages.Select(x => x.Split(',')[0].ToLower()),
                information.Select(x => x.Value.Metadata.Id.ToLower())
            );
        }

        [Test]
        public async Task GetPackages_ProjectsFilter_Should_Remove_Test_Projects()
        {
            var methods = new Methods(new PackageOptions
            {
                ProjectsFilterOption = @"../../../SampleProjectFilters.json",
                ProjectDirectory = TestSetup.ThisProjectSolutionPath
            });

            var result = await methods.GetPackages();

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First().Key.EndsWith("NugetUtility.csproj"), "name validation");
        }

        [Test]
        public async Task GetPackages_PackagesFilter_Should_Remove_SlnParser()
        {
            var methods = new Methods(new PackageOptions
            {
                PackagesFilterOption = @"../../../SamplePackagesFilters.json",
                ProjectDirectory = TestSetup.ThisProjectSolutionPath
            });

            var result = await methods.GetPackages();

            CollectionAssert.IsNotEmpty(result);
            Assert.IsFalse(result.SelectMany(p => p.Value).Count(p => p.Key == "Onion.SolutionParser.Parser.Standard") > 0);
        }

        [Test]
        public async Task GetPackages_AllowedLicenses_Should_Throw_On_Empty()
        {
            var methods = new Methods(new PackageOptions
            {
                ProjectsFilterOption = @"../../../SampleProjectFilters.json",
                AllowedLicenseTypesOption = @"../../../SampleAllowedLicenses.json",
                ProjectDirectory = TestSetup.ThisProjectSolutionPath
            });

            var result = await methods.GetPackages();
            var validationResult = methods.ValidateLicenses(result);
            Assert.False(validationResult.IsValid);
            Assert.AreEqual(1, validationResult.InvalidPackages.Count);
        }
    }
}