// See https://aka.ms/new-console-template for more information

using McMaster.Extensions.CommandLineUtils;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using NuGetUtility.ConsoleUtilities;
using NuGetUtility.LicenseValidator;
using NuGetUtility.PackageInformationReader;
using NuGetUtility.ReferencedPackagesReader;
using NuGetUtility.Serialization;
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

        [Option(LongName = "include-transitive", ShortName = "t",
            Description =
                "If set, the whole license tree is followed in order to determine all nuget's used by the projects")]
        public bool IncludeTransitive { get; } = false;

        [Option(LongName = "allowed-license-types", ShortName = "a",
            Description = "File in json format that contains an array of all allowed license types")]
        public string? AllowedLicenses { get; } = null;

        [Option(LongName = "ignored-packages", ShortName = "ignore",
            Description =
                "File in json format that contains an array of nuget package names to completely ignore (e.g. useful for nuget packages built in-house. Note that even though the packages are ignored, their transitive dependencies are not.")]
        public string? IgnoredPackages { get; } = null;

        [Option(LongName = "licenseurl-to-license-mappings", ShortName = "mapping",
            Description = "File in json format that contains a dictionary to map license urls to licenses.")]
        public string? LicenseMapping { get; } = null;

        [Option(LongName = "override-package-information", ShortName = "override",
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
            var validationExceptions = new List<Exception>();

            foreach (var project in projects)
            {
                IEnumerable<IPackageSearchMetadata> installedPackages;
                try
                {
                    installedPackages = projectReader.GetInstalledPackages(project, IncludeTransitive);
                }
                catch (MsBuildAbstractionException e)
                {
                    validationExceptions.Add(e);
                    continue;
                }

                var settings = Settings.LoadDefaultSettings(project);
                var sourceProvider = new PackageSourceProvider(settings);
                using var informationReader = new PackageInformationReader.PackageInformationReader(
                    new WrappedSourceRepositoryProvider(new SourceRepositoryProvider(sourceProvider,
                        Repository.Provider.GetCoreV3())), overridePackageInformation);
                var downloadedInfo = informationReader.GetPackageInfo(installedPackages, CancellationToken.None);

                await validator.Validate(downloadedInfo, project);
            }

            if (validationExceptions.Any())
            {
                foreach (var exception in validationExceptions)
                {
                    await Console.Error.WriteLineAsync(exception.Message);
                }

                return -1;
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

            TablePrinterExtensions.Create("Package", "Version", "License Expression").FromValues(
                    validator.GetValidatedLicenses(),
                    license => { return new object[] { license.PackageId, license.PackageVersion, license.License }; })
                .Print();
            return 0;
        }

        private IEnumerable<CustomPackageInformation> GetOverridePackageInformation()
        {
            if (OverridePackageInformation == null)
            {
                return Enumerable.Empty<CustomPackageInformation>();
            }

            var serializerOptions = new JsonSerializerOptions();
            serializerOptions.Converters.Add(new NuGetVersionConverter());
            return JsonSerializer.Deserialize<IEnumerable<CustomPackageInformation>>(
                File.ReadAllText(OverridePackageInformation), serializerOptions)!;
        }

        private IEnumerable<string> GetAllowedLicenses()
        {
            if (AllowedLicenses == null)
            {
                return Enumerable.Empty<string>();
            }

            return JsonSerializer.Deserialize<IEnumerable<string>>(File.ReadAllText(AllowedLicenses))!;
        }

        private Dictionary<Uri, string> GetLicenseMappings()
        {
            if (LicenseMapping == null)
            {
                return UrlToLicenseMapping.Default;
            }

            var serializerOptions = new JsonSerializerOptions();
            serializerOptions.Converters.Add(new UriDictionaryJsonConverter<string>());
            return JsonSerializer.Deserialize<Dictionary<Uri, string>>(File.ReadAllText(LicenseMapping),
                serializerOptions)!;
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
