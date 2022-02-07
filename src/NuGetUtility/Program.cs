// See https://aka.ms/new-console-template for more information

using McMaster.Extensions.CommandLineUtils;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using NuGetUtility.ConsoleUtilities;
using NuGetUtility.LicenseValidator;
using NuGetUtility.PackageInformationReader;
using NuGetUtility.ReferencedPackagesReader;
using NuGetUtility.Wrapper.MsBuildWrapper;
using NuGetUtility.Wrapper.NuGetWrapper.ProjectModel;
using NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types;
using System.Text.Json;

namespace NuGetUtility
{
    public class Program
    {
        [Option(ShortName = "i", LongName = "input",
            Description = "The project file who's dependencies should be analyzed")]
        public string? InputProjectFile { get; } = null;

        [Option(ShortName = "ji", LongName = "json-input",
            Description = "File in json format that contains an array of all project files to be evaluated")]
        public string? InputJsonFile { get; } = null;

        [Option(LongName = "include-transitive",
            Description =
                "If set, the whole license tree is followed in order to determine all nuget's used by the projects")]
        public bool IncludeTransitive { get; } = false;

        [Option(LongName = "allowed-license-types",
            Description = "File in json format that contains an array of all allowed license types")]
        public string? AllowedLicenses { get; } = null;

        [Option(LongName = "ignored-packages",
            Description =
                "File in json format that contains an array of nuget package names to completely ignore (e.g. useful for nuget packages built in-house. Note that even though the packages are ignored, their transitive dependencies are not.")]
        public string? IgnoredPackages { get; } = null;

        [Option(LongName = "licenseurl-to-license-mappings",
            Description = "File in json format that contains a dictionary to map license urls to licenses.")]
        public string? LicenseMapping { get; } = null;

        public static async Task<int> Main(string[] args)
        {
            return await CommandLineApplication.ExecuteAsync<Program>(args);
        }

        private async Task<int> OnExecuteAsync()
        {
            var projects = GetProjects();
            var ignoredPackages = GetIgnoredPackages();
            var licenseMappings = GetLicenseMappings();

            var projectReader = new ReferencedPackageReader(ignoredPackages, new MsBuildAbstraction(),
                new LockFileFactory(), new PackageSearchMetadataBuilderFactory());
            var validator = new LicenseValidator.LicenseValidator(licenseMappings, new LicenseId[] { });

            foreach (var project in projects)
            {
                var installedPackages = projectReader.GetInstalledPackages(project, IncludeTransitive);

                var settings = Settings.LoadDefaultSettings(project);
                var sourceProvider = new PackageSourceProvider(settings);
                using var informationReader = new PackageInformationReader.PackageInformationReader(
                    new WrappedSourceRepositoryProvider(new SourceRepositoryProvider(sourceProvider,
                        Repository.Provider.GetCoreV3())), new List<CustomPackageInformation>());
                var downloadedInfo = informationReader.GetPackageInfo(installedPackages);

                await validator.Validate(downloadedInfo, project);
            }

            if (validator.GetErrors().Any())
            {
                TablePrinterExtensions.Create("Context", "Package", "Version", "LicenseError").FromValues(
                    validator.GetErrors(),
                    error =>
                    {
                        return new object[] { error.Context, error.PackageId, error.PackageVersion, error.Message };
                    }).Print();
                return -1;
            }

            TablePrinterExtensions.Create("Package", "Version", "License Type", "LicenseVersion").FromValues(
                validator.GetValidatedLicenses(),
                license =>
                {
                    return new object[]
                    {
                        license.PackageId, license.PackageVersion, license.License.Id, license.License.Version
                    };
                }).Print();
            return 0;
        }

        private Dictionary<Uri, LicenseId> GetLicenseMappings()
        {
            if (LicenseMapping == null)
            {
                return UrlToLicenseMapping.Default;
            }

            return JsonSerializer.Deserialize<Dictionary<Uri, LicenseId>>(File.ReadAllText(LicenseMapping))!;
        }

        private IEnumerable<string> GetIgnoredPackages()
        {
            if (IgnoredPackages == null)
            {
                return Enumerable.Empty<string>();
            }

            return JsonSerializer.Deserialize<IEnumerable<string>>(File.ReadAllText(IgnoredPackages))!;
        }

        private IEnumerable<string> GetProjects()
        {
            if (InputProjectFile != null)
            {
                return new[] { InputProjectFile };
            }

            if (InputJsonFile != null)
            {
                return JsonSerializer.Deserialize<IEnumerable<string>>(File.ReadAllText(InputJsonFile))!;
            }

            throw new FileNotFoundException("Please provide an input file");
        }
    }
}
