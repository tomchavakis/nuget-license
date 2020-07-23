using FluentAssertions;
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
            _methods.GetProjectExtension().Should().Be(".csproj");
        }

        [Test]
        public void GetProjectReferences_Should_Resolve_Projects()
        {
            var packages = _methods.GetProjectReferences(_projectPath);

            packages.Should().NotBeEmpty();
        }

        [Test]
        public async Task GetNugetInformationAsync_Should_Resolve_Projects()
        {
            var packages = _methods.GetProjectReferences(_projectPath);
            var information = await _methods.GetNugetInformationAsync(_projectPath, packages);

            packages.Select(x => x.Split(',')[0].ToLower())
                .Should()
                .BeEquivalentTo(information.Select(x => x.Value.Metadata.Id.ToLower()));
        }

        [TestCase("FluentValidation,5.1.0.0")]
        [Test]
        public async Task GetNugetInformationAsync_Should_Resolve_Missing_NuSpec_File(string package)
        {
            var packages = package.Split(';', System.StringSplitOptions.RemoveEmptyEntries);
            var information = await _methods.GetNugetInformationAsync(_projectPath, packages);

            packages.Select(x => x.Split(',')[0])
                .Should()
                .BeEquivalentTo(information.Select(x => x.Value.Metadata.Id));
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

            result.Should()
                .HaveCount(1)
                .And.Match(kvp => kvp.First().Key.EndsWith("NugetUtility.csproj"));
        }

        [Test]
        public async Task GetPackages_PackagesFilter_Should_Remove_CommandLineParser()
        {
            var methods = new Methods(new PackageOptions
            {
                PackagesFilterOption = @"../../../SamplePackagesFilters.json",
                ProjectDirectory = TestSetup.ThisProjectSolutionPath
            });

            var result = await methods.GetPackages();

            result.Should().NotBeEmpty()
                .And.NotContainKey("CommandLineParser");
        }

        [Test]
        public async Task GetPackages_AllowedLicenses_Should_Throw_On_MIT()
        {
            var methods = new Methods(new PackageOptions
            {
                ProjectsFilterOption = @"../../../SampleProjectFilters.json",
                AllowedLicenseTypesOption = @"../../../SampleAllowedLicenses.json",
                ProjectDirectory = TestSetup.ThisProjectSolutionPath
            });

            var result = await methods.GetPackages();
            var validationResult = methods.ValidateLicenses(result);

            result.Should().HaveCount(1);
            validationResult.IsValid.Should().BeFalse();
            validationResult.InvalidPackages.Count.Should().Be(2);
        }

        [Test]
        public async Task GetPackages_InputJson_Should_OnlyParseGivenProjects()
        {
            var methods = new Methods(new PackageOptions
            {
                AllowedLicenseTypesOption = @"../../../SampleAllowedLicenses.json",
                ProjectDirectory = @"../../../SampleAllowedProjects.json"
            });

            var result = await methods.GetPackages();
            var validationResult = methods.ValidateLicenses(result);

            result.Should().HaveCount(1);
            validationResult.IsValid.Should().BeFalse();
            validationResult.InvalidPackages.Count.Should().Be(2);
        }
    }
}