using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System;
using System.Threading.Tasks;
using CommandLine.Text;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace NugetUtility.Tests
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class MethodTests
    {
        private string _projectPath = @"../../../";
        private Methods _methods;

        public void AddMethods()
        {
            _methods = new Methods(new PackageOptions { ProjectDirectory = _projectPath, Timeout = 10 });
        }

        private void AddUniquePackageOption()
        {
            _methods = new Methods(new PackageOptions { UniqueOnly = true, ProjectDirectory = _projectPath });
        }

        [Test]
        public void GetProjectExtension_Should_Be_CsprojOrFsProj()
        {
            AddMethods();
            _methods.GetProjectExtensions().Should().Contain(".csproj", ".fsproj");
        }

        [Test]
        public void GetProjectReferences_Should_Resolve_Projects()
        {
            AddMethods();
            var packages = _methods.GetProjectReferences(_projectPath);

            packages.Should().NotBeEmpty();
        }

        [Test]
        public async Task GetNugetInformationAsync_Should_Resolve_Projects()
        {
            AddMethods();
            var packages = _methods.GetProjectReferences(_projectPath);
            var referencedpackages = packages.Select(p => { var split = p.Split(","); return new PackageNameAndVersion { Name = split[0], Version = split[1] }; });
            var information = await _methods.GetNugetInformationAsync(_projectPath, referencedpackages);

            packages.Select(x => x.Split(',')[0].ToLower())
                .Should()
                .BeEquivalentTo(information.Select(x => x.Value.Metadata.Id.ToLower()));
        }

        [TestCase("FluentValidation,6.1.0.0")]
        [Test]
        public async Task GetNugetInformationAsync_Should_Resolve_Missing_NuSpec_File(string package)
        {
            AddMethods();
            var packages = package.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var referencedpackages = packages.Select(p => { var split = p.Split(","); return new PackageNameAndVersion { Name = split[0], Version = split[1] }; });
            var information = await _methods.GetNugetInformationAsync(_projectPath, referencedpackages);

            packages.Select(x => x.Split(',')[0])
                .Should()
                .BeEquivalentTo(information.Select(x => x.Value.Metadata.Id));
        }

        [TestCase("CommandLineParser,2.8.0")]
        [Test]
        public async Task GetNugetInformationAsync_Should_Resolve_NuSpec_File_From_Local_Cache(string package)
        {
            AddMethods();
            var packages = package.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var referencedpackages = packages.Select(p => { var split = p.Split(","); return new PackageNameAndVersion { Name = split[0], Version = split[1] }; });
            var information = await _methods.GetNugetInformationAsync(_projectPath, referencedpackages);

            packages.Select(x => x.Split(',')[0])
                .Should()
                .BeEquivalentTo(information.Select(x => x.Value.Metadata.Id));
        }

        [Test]
        public void MapPackagesToLibraryInfo_Unique_Should_Return_One_Result()
        {
            AddUniquePackageOption();
            PackageList list = new PackageList();
            list.Add("log4net", new Package
            {
                Metadata = new Metadata
                {
                    Id = "log4net",
                    License = new License
                    {
                        Text = "MIT",
                        Type = "Open"
                    },
                    Version = "2.0.8",
                },
            });

            list.Add("log4net2", new Package
            {
                Metadata = new Metadata
                {
                    Id = "log4net",
                    License = new License
                    {
                        Text = "MIT",
                        Type = "Open"
                    },
                    Version = "2.0.8",
                }
            });

            var packages = new Dictionary<string, PackageList>();
            packages.Add("packages", list);
            var info = _methods.MapPackagesToLibraryInfo(packages);
            info.Count.Should().Equals(1);
        }

        [Test]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task MapPackagesToLibraryInfo_Proxy_Should_Return_License()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {

            var methods = new Methods(
                new PackageOptions
                {
                    ProjectDirectory = _projectPath,
                    ProxyURL = "http://localhost:8080",
                    ProxySystemAuth = true
                });


            PackageList list = new PackageList();
            list.Add("log4net", new Package
            {
                Metadata = new Metadata
                {
                    Id = "log4net",
                    License = new License
                    {
                        Text = "MIT",
                        Type = "Open"
                    },
                    Version = "2.0.8",
                },

            });

            list.Add("Newtonsoft.Json", new Package
            {
                Metadata = new Metadata
                {
                    Id = "Newtonsoft.Json",
                    License = new License
                    {
                        Text = "MIT",
                        Type = "Open"
                    },
                    Version = "6.0.9",
                }
            });

            var packages = new Dictionary<string, PackageList>();
            packages.Add("packages", list);
            var info = methods.MapPackagesToLibraryInfo(packages);
            info.Count.Should().Equals(2);
        }



        [Test]
        public async Task GetPackages_ProjectsFilter_Should_Remove_Test_Projects()
        {
            var methods = new Methods(new PackageOptions
            {
                ProjectsFilterOption = @"../../../SampleProjectFilters.json",
                ProjectDirectory = TestSetup.ThisProjectSolutionPath,
                Timeout = 10
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
                ProjectDirectory = TestSetup.ThisProjectSolutionPath,
                Timeout = 10
            });

            var result = await methods.GetPackages();

            result.Should().NotBeEmpty()
                .And.NotContainKey("CommandLineParser");
        }

        [Test]
        public async Task GetPackages_RegexPackagesFilter_Should_Remove_CommandLineParser()
        {
            var methods = new Methods(new PackageOptions
            {
                PackagesFilterOption = "/CommandLine*/",
                ProjectDirectory = TestSetup.ThisProjectSolutionPath,
                Timeout = 10
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
            validationResult.InvalidPackages.Count.Should().Be(5);
        }

        [Test]
        public async Task GetPackages_InputJson_Should_OnlyParseGivenProjects()
        {
            var methods = new Methods(new PackageOptions
            {
                AllowedLicenseTypesOption = @"../../../SampleAllowedLicenses.json",
                ProjectDirectory = @"../../../SampleAllowedProjects.json",
                Timeout = 10
            });

            var result = await methods.GetPackages();
            var validationResult = methods.ValidateLicenses(result);

            result.Should().HaveCount(1);
            validationResult.IsValid.Should().BeFalse();
            validationResult.InvalidPackages.Count.Should().Be(5);
        }

        [Test]
        public async Task ValidateLicenses_ForbiddenLicenses_Should_Be_Invalid()
        {
            var methods = new Methods(new PackageOptions
            {
                ProjectsFilterOption = @"../../../SampleProjectFilters.json",
                ForbiddenLicenseTypesOption = @"../../../SampleForbiddenLicenses.json",
                ProjectDirectory = TestSetup.ThisProjectSolutionPath,
                Timeout = 10
            });

            var result = await methods.GetPackages();
            var mapped = methods.MapPackagesToLibraryInfo(result);
            var validationResult = methods.ValidateLicenses(mapped);

            validationResult.IsValid.Should().BeFalse();
            validationResult.InvalidPackages.Should().HaveCount(2);
            validationResult.InvalidPackages.ElementAt(0).LicenseType.Should().Be("MIT");
            validationResult.InvalidPackages.ElementAt(0).PackageName.Should().Be("HtmlAgilityPack");
            validationResult.InvalidPackages.ElementAt(1).LicenseType.Should().Be("MIT");
            validationResult.InvalidPackages.ElementAt(1).PackageName.Should().Be("Newtonsoft.Json");
        }

        [Test]
        public async Task ValidateLicenses_AllowedLicenses_Should_Be_Invalid()
        {
            var methods = new Methods(new PackageOptions
            {
                ProjectsFilterOption = @"../../../SampleProjectFilters.json",
                AllowedLicenseTypesOption = @"../../../SampleAllowedLicenses.json",
                ProjectDirectory = TestSetup.ThisProjectSolutionPath,
                Timeout = 10
            });

            var result = await methods.GetPackages();
            var mapped = methods.MapPackagesToLibraryInfo(result);
            var validationResult = methods.ValidateLicenses(mapped);

            validationResult.IsValid.Should().BeFalse();
            validationResult.InvalidPackages.Should().HaveCount(5);
            validationResult.InvalidPackages.ElementAt(0).LicenseType.Should().Be("License.md");
            validationResult.InvalidPackages.ElementAt(1).LicenseType.Should().Be("MIT");
            validationResult.InvalidPackages.ElementAt(2).LicenseType.Should().Be("MIT");
            validationResult.InvalidPackages.ElementAt(3).LicenseType.Should().Be("Apache-2.0");
            validationResult.InvalidPackages.ElementAt(4).LicenseType.Should().Be("MS-EULA");
        }

        [Test]
        public async Task GetProjectReferencesFromAssetsFile_Should_Resolve_Transitive_Assets()
        {
            var methods = new Methods(new PackageOptions
            {
                UseProjectAssetsJson = true,
                IncludeTransitive = true,
                ProjectDirectory = @"../../../NugetUtility.Tests.csproj",
                Timeout = 10
            });

            var packages = await methods.GetPackages();
            packages.Should().ContainKey("../../../NugetUtility.Tests.csproj");
            packages.Should().HaveCount(1);
            var list = packages.Values.First();

            // Just look for a few expected packages. First-order refs:
            list.Should().ContainKey($"NUnit,{typeof(TestAttribute).Assembly.GetName().Version.ToString(3)}");

            // Some second-order refs:
            list.Should().ContainKey($"CommandLineParser,{typeof(UsageAttribute).Assembly.GetName().Version.ToString(3)}");
            list.Should().Contain(x => x.Key.StartsWith("System.IO.Compression,"));

            // Some third-order refs:
            list.Should().Contain(x => x.Key.StartsWith("System.Buffers,"));
        }

        [TestCase("BenchmarkDotNet", "0.12.1", "https://licenses.nuget.org/MIT", "MIT")]
        [TestCase("BCrypt.Net-Next", "2.1.3", "https://github.com/BcryptNet/bcrypt.net/blob/master/licence.txt", "")]
        [TestCase("System.Memory", "4.5.4", "https://github.com/dotnet/corefx/blob/master/LICENSE.TXT", "MIT")]
        [TestCase("System.Text.RegularExpressions", "4.3.0", "http://go.microsoft.com/fwlink/?LinkId=329770", "MS-EULA")]
        [Test]
        public async Task ExportLicenseTexts_Should_Export_File(string packageName, string packageVersion, string licenseUrl, string licenseType)
        {
            var methods = new Methods(new PackageOptions
            {
                ProjectDirectory = _projectPath,
                Timeout = 10,
                ExportLicenseTexts = true,
            });

            List<LibraryInfo> infos = new List<LibraryInfo>();
            infos.Add(new LibraryInfo()
            {
                PackageName = packageName,
                PackageVersion = packageVersion,
                LicenseUrl = licenseUrl,
                LicenseType = licenseType,
            });
            await methods.ExportLicenseTexts(infos);
            var directory = methods.GetExportDirectory();
            var outpath = Path.Combine(directory, packageName + "_" + packageVersion + ".txt");
            var outpathhtml = Path.Combine(directory, packageName + "_" + packageVersion + ".html");
            Assert.That(File.Exists(outpath) || File.Exists(outpathhtml));
        }

        [TestCase("BenchmarkDotNet", "License.txt", "10.12.1")]
        [Test]
        public async Task GetLicenceFromNpkgFile_Should_Return_False(string packageName, string licenseFile, string packageVersion)
        {
            var methods = new Methods(new PackageOptions
            {
                ProjectDirectory = _projectPath,
                Timeout = 10,
                ExportLicenseTexts = true,
            });

            var result = await methods.GetLicenceFromNpkgFile(packageName, licenseFile, packageVersion);
            Assert.False(result);
        }

        [Test]
        public void HttpClient_IgnoreSslError_CallbackTest()
        {
            Assert.True(Methods.IgnoreSslCertificateErrorCallback(null, null, null, System.Net.Security.SslPolicyErrors.None));
        }

        [TestCase("System.Linq", "(4.1.0,)")]
        [TestCase("BCrypt.Net-Next", "2.1.3")]
        [Test]
        public void HttpClient_IgnoreSslError_GetNugetInformationAsync(string package, string version)
        {
            var methods = new Methods(new PackageOptions { ProjectDirectory = _projectPath, IgnoreSslCertificateErrors = true });

            var referencedpackages = new PackageNameAndVersion[] { new PackageNameAndVersion { Name = package, Version = version } };

            Assert.DoesNotThrowAsync(async () => await _methods.GetNugetInformationAsync(_projectPath, referencedpackages));
        }

        [Test]
        public async Task GetPackages_ShouldSupportPackageReferencesWithVersionRange()
        {
            var methods = new Methods(new PackageOptions
            {
                ProjectDirectory = "../../../TestProjectFiles/PackageRangeReference.csproj",
                Timeout = 10
            });

            var result = await methods.GetPackages();

            using (new AssertionScope())
            {
                result.Should()
                    .HaveCount(1);

                result.First().Value.Should().HaveCount(1);

                result.First().Value.First().Value.Metadata.Id.Should().Be("FluentAssertions");
            }
        }
    }
}