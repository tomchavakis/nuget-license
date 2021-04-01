using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace NugetUtility.Tests {
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class MethodTests {
        private string _projectPath;
        private Methods _methods;

        [SetUp]
        public void Setup () {
            _projectPath = @"../../../";
            _methods = new Methods (new PackageOptions { ProjectDirectory = _projectPath });
        }

        private void AddUniquePackageOption () {
            _methods = new Methods (new PackageOptions { UniqueOnly = true, ProjectDirectory = _projectPath });
        }

        [Test]
        public void GetProjectExtension_Should_Be_CsprojOrFsProj () {
            _methods.GetProjectExtensions ().Should ().Contain (".csproj", ".fsproj");
        }

        [Test]
        public void GetProjectReferences_Should_Resolve_Projects () {
            var packages = _methods.GetProjectReferences (_projectPath);

            packages.Should ().NotBeEmpty ();
        }

        [Test]
        public async Task GetNugetInformationAsync_Should_Resolve_Projects () {
            var packages = _methods.GetProjectReferences (_projectPath);
            var referencedpackages = packages.Select (p => { var split = p.Split (","); return new PackageNameAndVersion { Name = split[0], Version = split[1] }; });
            var information = await _methods.GetNugetInformationAsync (_projectPath, referencedpackages);

            packages.Select (x => x.Split (',') [0].ToLower ())
                .Should ()
                .BeEquivalentTo (information.Select (x => x.Value.Metadata.Id.ToLower ()));
        }

        [TestCase ("FluentValidation,5.1.0.0")]
        [Test]
        public async Task GetNugetInformationAsync_Should_Resolve_Missing_NuSpec_File (string package) {
            var packages = package.Split (';', System.StringSplitOptions.RemoveEmptyEntries);
            var referencedpackages = packages.Select (p => { var split = p.Split (","); return new PackageNameAndVersion { Name = split[0], Version = split[1] }; });
            var information = await _methods.GetNugetInformationAsync (_projectPath, referencedpackages);

            packages.Select (x => x.Split (',') [0])
                .Should ()
                .BeEquivalentTo (information.Select (x => x.Value.Metadata.Id));
        }

        [TestCase ("FluentValidation", "5.1.0.0")]
        [TestCase ("System.Linq", "(4.1.0,)")]
        [TestCase ("System.Linq", "[4.1.0]")]
        [TestCase ("System.Linq", "(,4.1.0]")]
        [TestCase ("System.Linq", "(,4.1.0)")]
        [TestCase ("System.Linq", "[4.1.0,4.3.0]")]
        [TestCase ("System.Linq", "(4.1.0,4.3.0)")]
        [TestCase ("System.Linq", "[4.1.0,4.3.0)")]
        [TestCase ("BCrypt.Net-Next", "2.1.3")]
        [Test]
        public async Task GetNugetInformationAsync_Should_Properly_TreatAllAllowedNuSpecReferenceTypes (string package,
            string version) {
            var referencedpackages = new PackageNameAndVersion[] { new PackageNameAndVersion { Name = package, Version = version } };
            var information = await _methods.GetNugetInformationAsync (_projectPath, referencedpackages);

            var expectation = version.Trim (new char[] { '[', '(', ']', ')' })
                .Split (",", System.StringSplitOptions.RemoveEmptyEntries).Select (v => $"{package},{v}");
            expectation.Should ().BeEquivalentTo (information.Select (x => x.Key));
            expectation.Should ()
                .BeEquivalentTo (information.Select (x => $"{x.Value.Metadata.Id},{x.Value.Metadata.Version}"));
        }

        [Test]
        public void MapPackagesToLibraryInfo_Unique_Should_Return_One_Result () {
            AddUniquePackageOption ();
            PackageList list = new PackageList ();
            list.Add ("log4net", new Package {
                Metadata = new Metadata {
                    Id = "log4net",
                        License = new License {
                            Text = "MIT",
                                Type = "Open"
                        },
                        Version = "2.0.8",
                },
            });

            list.Add ("log4net2", new Package {
                Metadata = new Metadata {
                    Id = "log4net",
                        License = new License {
                            Text = "MIT",
                                Type = "Open"
                        },
                        Version = "2.0.8",
                }
            });

            var packages = new Dictionary<string, PackageList> ();
            packages.Add ("packages", list);
            var info = _methods.MapPackagesToLibraryInfo (packages);
            info.Count.Should ().Equals (1);
        }

        [Test]
        public async Task GetPackages_ProjectsFilter_Should_Remove_Test_Projects () {
            var methods = new Methods (new PackageOptions {
                ProjectsFilterOption = @"../../../SampleProjectFilters.json",
                    ProjectDirectory = TestSetup.ThisProjectSolutionPath
            });

            var result = await methods.GetPackages ();

            result.Should ()
                .HaveCount (1)
                .And.Match (kvp => kvp.First ().Key.EndsWith ("NugetUtility.csproj"));
        }

        [Test]
        public async Task GetPackages_PackagesFilter_Should_Remove_CommandLineParser () {
            var methods = new Methods (new PackageOptions {
                PackagesFilterOption = @"../../../SamplePackagesFilters.json",
                    ProjectDirectory = TestSetup.ThisProjectSolutionPath
            });

            var result = await methods.GetPackages ();

            result.Should ().NotBeEmpty ()
                .And.NotContainKey ("CommandLineParser");
        }

        [Test]
        public async Task GetPackages_RegexPackagesFilter_Should_Remove_CommandLineParser()
        {
            var methods = new Methods(new PackageOptions
            {
                PackagesFilterOption = "/CommandLine*/",
                ProjectDirectory = TestSetup.ThisProjectSolutionPath
            });

            var result = await methods.GetPackages();

            result.Should().NotBeEmpty()
                .And.NotContainKey("CommandLineParser");
        }

        [Test]
        public async Task GetPackages_AllowedLicenses_Should_Throw_On_MIT () {
            var methods = new Methods (new PackageOptions {
                ProjectsFilterOption = @"../../../SampleProjectFilters.json",
                    AllowedLicenseTypesOption = @"../../../SampleAllowedLicenses.json",
                    ProjectDirectory = TestSetup.ThisProjectSolutionPath
            });

            var result = await methods.GetPackages ();
            var validationResult = methods.ValidateLicenses (result);

            result.Should ().HaveCount (1);
            validationResult.IsValid.Should ().BeFalse ();
            validationResult.InvalidPackages.Count.Should ().Be (2);
        }

        [Test]
        public async Task GetPackages_InputJson_Should_OnlyParseGivenProjects () {
            var methods = new Methods (new PackageOptions {
                AllowedLicenseTypesOption = @"../../../SampleAllowedLicenses.json",
                    ProjectDirectory = @"../../../SampleAllowedProjects.json"
            });

            var result = await methods.GetPackages ();
            var validationResult = methods.ValidateLicenses (result);

            result.Should ().HaveCount (1);
            validationResult.IsValid.Should ().BeFalse ();
            validationResult.InvalidPackages.Count.Should ().Be (2);
        }
    }
}