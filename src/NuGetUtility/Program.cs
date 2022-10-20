﻿using McMaster.Extensions.CommandLineUtils;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using NuGetUtility.Extension;
using NuGetUtility.LicenseValidator;
using NuGetUtility.Output;
using NuGetUtility.Output.Json;
using NuGetUtility.Output.Table;
using NuGetUtility.PackageInformationReader;
using NuGetUtility.ReferencedPackagesReader;
using NuGetUtility.Serialization;
using NuGetUtility.Wrapper.HttpClientWrapper;
using NuGetUtility.Wrapper.MsBuildWrapper;
using NuGetUtility.Wrapper.NuGetWrapper.ProjectModel;
using NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types;
using System.Text.Json;

namespace NuGetUtility
{
    public class Program
    {
        private HttpClient? _httpClient;

        [Option(ShortName = "i",
            LongName = "input",
            Description = "The project (or solution) file who's dependencies should be analyzed")]
        public string? InputFile { get; } = null;

        [Option(ShortName = "ji",
            LongName = "json-input",
            Description =
                "File in json format that contains an array of all files to be evaluated. The Files can either point to a project or a solution.")]
        public string? InputJsonFile { get; } = null;

        [Option(LongName = "include-transitive",
            ShortName = "t",
            Description =
                "If set, the whole license tree is followed in order to determine all nuget's used by the projects")]
        public bool IncludeTransitive { get; } = false;

        [Option(LongName = "allowed-license-types",
            ShortName = "a",
            Description = "File in json format that contains an array of all allowed license types")]
        public string? AllowedLicenses { get; } = null;

        [Option(LongName = "ignored-packages",
            ShortName = "ignore",
            Description =
                "File in json format that contains an array of nuget package names to completely ignore (e.g. useful for nuget packages built in-house. Note that even though the packages are ignored, their transitive dependencies are not.")]
        public string? IgnoredPackages { get; } = null;

        [Option(LongName = "licenseurl-to-license-mappings",
            ShortName = "mapping",
            Description = "File in json format that contains a dictionary to map license urls to licenses.")]
        public string? LicenseMapping { get; } = null;

        [Option(LongName = "override-package-information",
            ShortName = "override",
            Description =
                "File in json format that contains a list of package and license information which should be used in favor of the online version. This option can be used to override the license type of packages that e.g. specify the license as file.")]
        public string? OverridePackageInformation { get; } = null;

        [Option(LongName = "license-information-download-location",
            ShortName = "d",
            Description =
                "When set, the application downloads all licenses given using a license URL to the specified folder.")]
        public string? DownloadLicenseInformation { get; } = null;

        [Option(LongName = "output",
            ShortName = "o",
            Description = "This parameter allows to choose between tabular and json output.")]
        public OutputType OutputType { get; } = OutputType.Table;

        [Option(LongName = "error-only",
            ShortName = "err",
            Description =
                "If this option is set and there are license validation errors, only the errors are returned as result. Otherwise all validation results are always returned.")]
        public bool ReturnErrorsOnly { get; } = false;

        private HttpClient HttpClient
        {
            get
            {
                if (_httpClient == null)
                {
                    _httpClient = new HttpClient();
                }

                return _httpClient;
            }
        }

        public static async Task Main(string[] args)
        {
            var lifetime = new AppLifetime();
            var returnCode = await CommandLineApplication.ExecuteAsync<Program>(args, lifetime.Token);
            lifetime.Done(returnCode);
        }

        private async Task<int> OnExecuteAsync(CancellationToken cancellationToken)
        {
            var inputFiles = GetInputFiles();
            var ignoredPackages = GetIgnoredPackages();
            var licenseMappings = GetLicenseMappings();
            var allowedLicenses = GetAllowedLicenses();
            var overridePackageInformation = GetOverridePackageInformation();
            var urlLicenseFileDownloader = GetFileDownloader();
            var output = GetOutputFormatter();

            var msBuild = new MsBuildAbstraction();
            var projectCollector = new ProjectsCollector(msBuild);
            var projectReader = new ReferencedPackageReader(ignoredPackages,
                msBuild,
                new LockFileFactory(),
                new PackageSearchMetadataBuilderFactory());
            var validator = new LicenseValidator.LicenseValidator(licenseMappings,
                allowedLicenses,
                urlLicenseFileDownloader);
            var projectReaderExceptions = new List<Exception>();

            var projects = inputFiles.SelectMany(file => projectCollector.GetProjects(file));
            var packagesForProject = projects.Select(p =>
            {
                IEnumerable<IPackageSearchMetadata>? installedPackages = null;
                try
                {
                    installedPackages = projectReader.GetInstalledPackages(p, IncludeTransitive);
                }
                catch (Exception e)
                {
                    projectReaderExceptions.Add(e);
                }
                return new ProjectWithReferencedPackages(p,
                    installedPackages ?? Enumerable.Empty<IPackageSearchMetadata>());
            });
            var downloadedLicenseInformation =
                packagesForProject.SelectMany(p => GetPackageInfos(p, overridePackageInformation, cancellationToken));
            var results = await validator.Validate(downloadedLicenseInformation);

            if (projectReaderExceptions.Any())
            {
                await WriteValidationExceptions(projectReaderExceptions);

                return -1;
            }

            await using var outputStream = Console.OpenStandardOutput();
            await output.Write(outputStream, results.ToList());
            return 0;
        }
        private IAsyncEnumerable<ReferencedPackageWithContext> GetPackageInfos(
            ProjectWithReferencedPackages projectWithReferences,
            IEnumerable<CustomPackageInformation> overridePackageInformation,
            CancellationToken cancellation)
        {
            var settings = Settings.LoadDefaultSettings(projectWithReferences.Project);
            var sourceProvider = new PackageSourceProvider(settings);
            using var informationReader = new PackageInformationReader.PackageInformationReader(
                new WrappedSourceRepositoryProvider(new SourceRepositoryProvider(sourceProvider,
                    Repository.Provider.GetCoreV3())),
                overridePackageInformation);
            return informationReader.GetPackageInfo(new ProjectWithReferencedPackages(projectWithReferences.Project,
                    projectWithReferences.ReferencedPackages),
                cancellation);
        }

        private IOutputFormatter GetOutputFormatter()
        {
            return OutputType switch
            {
                OutputType.Json => new JsonOutputFormatter(false, ReturnErrorsOnly),
                OutputType.JsonPretty => new JsonOutputFormatter(true, ReturnErrorsOnly),
                OutputType.Table => new TableOutputFormatter(ReturnErrorsOnly),
                _ => throw new ArgumentOutOfRangeException($"{OutputType} not supported")
            };
        }

        private IFileDownloader GetFileDownloader()
        {
            if (DownloadLicenseInformation == null)
            {
                return new NopFileDownloader();
            }

            if (!Directory.Exists(DownloadLicenseInformation))
            {
                Directory.CreateDirectory(DownloadLicenseInformation);
            }

            return new FileDownloader(HttpClient, DownloadLicenseInformation);
        }

        private static async Task WriteValidationExceptions(List<Exception> validationExceptions)
        {
            foreach (var exception in validationExceptions)
            {
                await Console.Error.WriteLineAsync(exception.ToString());
            }
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
                File.ReadAllText(OverridePackageInformation),
                serializerOptions)!;
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

        private IEnumerable<string> GetInputFiles()
        {
            if (InputFile != null)
            {
                return new[] { InputFile };
            }

            if (InputJsonFile != null)
            {
                return JsonSerializer.Deserialize<IEnumerable<string>>(File.ReadAllText(InputJsonFile))!;
            }

            throw new FileNotFoundException("Please provide an input file");
        }
    }
}
