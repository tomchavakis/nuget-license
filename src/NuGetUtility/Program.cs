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

        [Option(LongName = "include-transitive", ShortName = "",
            Description =
                "If set, the whole license tree is followed in order to determine all nuget's used by the projects")]
        public bool IncludeTransitive { get; } = false;

        [Option(LongName = "allowed-license-types",
            Description = "File in json format that contains an array of all allowed license types")]
        public string? AllowedLicenses { get; } = null;

        [Option(LongName = "ignored-packages", ShortName = "",
            Description =
                "File in json format that contains an array of nuget package names to completely ignore (e.g. useful for nuget packages built in-house. Note that even though the packages are ignored, their transitive dependencies are not.")]
        public string? IgnoredPackages { get; } = null;

        [Option(LongName = "licenseurl-to-license-mappings",
            Description = "File in json format that contains a dictionary to map license urls to licenses.")]
        public string? LicenseMapping { get; } = null;

        [Option(LongName = "override-package-information",
            Description =
                "File in json format that contains a list of package and license information which should be used in favor of the online version. This option can be used to override the license type of packages that e.g. specify the license as file.")]
        public string? OverridePackageInformation { get; } = null;

        public static async Task Main(string[] args)
        {
            var lifetime = new AppLifetime();
            var returnCode = await CommandLineApplication.ExecuteAsync<Program>(args, lifetime.Token);
            lifetime.Done(returnCode);
        }

        private async Task<int> OnExecuteAsync()
        {
            var projects = GetProjects();
            var ignoredPackages = GetIgnoredPackages();
            var licenseMappings = GetLicenseMappings();
            var allowedLicenses = GetAllowedLicenses();
            var overridePackageInformation = GetOverridePackageInformation();

            var projectReader = new ReferencedPackageReader(ignoredPackages, new MsBuildAbstraction(),
                new LockFileFactory(), new PackageSearchMetadataBuilderFactory());
            var validator = new LicenseValidator.LicenseValidator(licenseMappings, allowedLicenses);

            foreach (var project in projects)
            {
                var installedPackages = projectReader.GetInstalledPackages(project, IncludeTransitive);

                var settings = Settings.LoadDefaultSettings(project);
                var sourceProvider = new PackageSourceProvider(settings);
                using var informationReader = new PackageInformationReader.PackageInformationReader(
                    new WrappedSourceRepositoryProvider(new SourceRepositoryProvider(sourceProvider,
                        Repository.Provider.GetCoreV3())), overridePackageInformation);
                var downloadedInfo = informationReader.GetPackageInfo(installedPackages, CancellationToken.None);

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

        private IEnumerable<CustomPackageInformation> GetOverridePackageInformation()
        {
            if (OverridePackageInformation == null)
            {
                return Enumerable.Empty<CustomPackageInformation>();
            }

            return JsonSerializer.Deserialize<IEnumerable<CustomPackageInformation>>(
                File.ReadAllText(OverridePackageInformation))!;
        }

        private IEnumerable<LicenseId> GetAllowedLicenses()
        {
            if (AllowedLicenses == null)
            {
                return Enumerable.Empty<LicenseId>();
            }

            return JsonSerializer.Deserialize<IEnumerable<LicenseId>>(File.ReadAllText(AllowedLicenses))!;
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
