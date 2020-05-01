using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace NugetUtility
{
    public class Methods
    {
        private const string fallbackPackageUrl = "https://www.nuget.org/api/v2/package/{0}/{1}";
        private const string nugetUrl = "https://api.nuget.org/v3-flatcontainer/";
        private static readonly Dictionary<Tuple<string, string>, Package> _requestCache = new Dictionary<Tuple<string, string>, Package>();
        private static readonly Dictionary<Tuple<string, string>, string> _licenseFileCache = new Dictionary<Tuple<string, string>, string>();
        /// <summary>
        /// See https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
        /// </summary>
        private static HttpClient _httpClient;
        private readonly IReadOnlyDictionary<string, string> _licenseMappings;
        private readonly PackageOptions _packageOptions;
        private readonly XmlSerializer _serializer;

        public Methods(PackageOptions packageOptions)
        {
            if (_httpClient is null)
            {
                _httpClient = new HttpClient
                {
                    BaseAddress = new Uri(nugetUrl),
                    Timeout = TimeSpan.FromSeconds(10)
                };
            }

            _serializer = new XmlSerializer(typeof(Package));
            _packageOptions = packageOptions;
            _licenseMappings = packageOptions.LicenseToUrlMappingsDictionary;
        }

        /// <summary>
        /// Get Nuget References per project
        /// </summary>
        /// <param name="project">project name</param>
        /// <param name="packages">List of projects</param>
        /// <returns></returns>
        public async Task<PackageList> GetNugetInformationAsync(string project, IEnumerable<string> packages)
        {
            WriteOutput(Environment.NewLine + "project:" + project + Environment.NewLine, logLevel: LogLevel.Information);
            var licenses = new PackageList();
            foreach (var packageWithVersion in packages)
            {
                try
                {
                    var split = packageWithVersion.Split(',');
                    var packageId = split[0];
                    var versionNumber = split[1];

                    if (_packageOptions.PackageFilter.Any(p => string.Compare(p, packageId, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        WriteOutput(packageId + " skipped by filter.", logLevel: LogLevel.Verbose);
                        continue;
                    }

                    var lookupKey = Tuple.Create(packageId, versionNumber);

                    if (_requestCache.TryGetValue(lookupKey, out var package))
                    {
                        WriteOutput(packageWithVersion + " obtained from request cache.", logLevel: LogLevel.Information);
                        licenses.Add(packageWithVersion, package);
                        continue;
                    }

                    using (var request = new HttpRequestMessage(HttpMethod.Get, $"{packageId}/{versionNumber}/{packageId}.nuspec"))
                    using (var response = await _httpClient.SendAsync(request))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            WriteOutput($"{request.RequestUri} failed due to {response.StatusCode}!", logLevel: LogLevel.Error);
                            var fallbackResult = await GetNuGetPackageFileResult<Package>(packageId, versionNumber, $"{packageId}.nuspec");
                            if (fallbackResult is Package)
                            {
                                licenses.Add(packageWithVersion, fallbackResult);
                                _requestCache[lookupKey] = fallbackResult;
                            }
                            await HandleLicensing(fallbackResult);

                            continue;
                        }

                        WriteOutput(request.RequestUri.ToString(), logLevel: LogLevel.Information);
                        using (var responseText = await response.Content.ReadAsStreamAsync())
                        using (var textReader = new StreamReader(responseText))
                        {
                            try
                            {
                                if (_serializer.Deserialize(new NamespaceIgnorantXmlTextReader(textReader)) is Package result)
                                {
                                    licenses.Add(packageWithVersion, result);
                                    _requestCache[lookupKey] = result;
                                    await HandleLicensing(result);
                                }
                            }
                            catch (Exception e)
                            {
                                WriteOutput(e.Message, e, LogLevel.Error);
                                throw;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteOutput(ex.Message, ex, LogLevel.Error);
                }
            }

            return licenses;
        }

        public async Task<Dictionary<string, PackageList>> GetPackages()
        {
            var licenses = new Dictionary<string, PackageList>();
            var projectFiles = GetValidProjects(_packageOptions.ProjectDirectory);
            foreach (var projectFile in projectFiles)
            {
                var references = this.GetProjectReferences(projectFile);
                var currentProjectLicenses = await this.GetNugetInformationAsync(projectFile, references);
                licenses[projectFile] = currentProjectLicenses;
            }

            return licenses;
        }

        public string GetProjectExtension(bool withWildcard = false)
        {
            return !withWildcard
                ? ".csproj"
                : "*.csproj";
        }

        /// <summary>
        /// Retreive the project references from csproj file
        /// </summary>
        /// <param name="projectPath">The Project Path</param>
        /// <returns></returns>
        public IEnumerable<string> GetProjectReferences(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
            {
                throw new ArgumentNullException(projectPath);
            }

            if (!projectPath.EndsWith(GetProjectExtension()))
            {
                projectPath = GetValidProjects(projectPath).FirstOrDefault();
            }

            if (projectPath is null)
            {
                throw new FileNotFoundException();
            }

            // First try to get references from new project file format
            var references = GetProjectReferencesFromNewProjectFile(projectPath);

            // Then if needed from old packages.config
            if (!references.Any())
            {
                references = GetProjectReferencesFromPackagesConfig(projectPath);
            }

            return references ?? Array.Empty<string>();
        }

        /// <summary>
        /// Main function to cleanup
        /// </summary>
        /// <param name="packages"></param>
        /// <returns></returns>
        public List<LibraryInfo> MapPackagesToLibraryInfo(Dictionary<string, PackageList> packages)
        {
            var libraryInfos = new List<LibraryInfo>(256);
            foreach (var packageList in packages)
            {
                foreach (var item in packageList.Value.Select(p => p.Value))
                {
                    var info = MapPackageToLibraryInfo(item, packageList.Key);
                    libraryInfos.Add(info);
                }
            }

            // merge in missing manual items where there wasn't a package
            var missedManualItems = _packageOptions.ManualInformation.Except(libraryInfos, LibraryNameAndVersionComparer.Default);
            foreach (var missed in missedManualItems)
            {
                libraryInfos.Add(missed);
            }

            if (_packageOptions.UniqueOnly)
            {
                libraryInfos = libraryInfos
                    .GroupBy(x => new { x.PackageName, x.PackageVersion })
                    .Select(g =>
                    {
                        var first = g.First();
                        return new LibraryInfo
                        {
                            PackageName = first.PackageName,
                            PackageVersion = first.PackageVersion,
                            PackageUrl = first.PackageUrl,
                            Description = first.Description,
                            LicenseType = first.LicenseType,
                            LicenseUrl = first.LicenseUrl,
                            Projects = _packageOptions.IncludeProjectFile ? string.Join(";", g.Select(p => p.Projects)) : null
                        };
                    })
                    .ToList();
            }

            return libraryInfos
                    .OrderBy(p => p.PackageName)
                    .ToList();
        }

        private LibraryInfo MapPackageToLibraryInfo(Package item, string projectFile)
        {
            string licenseType = item.Metadata.License?.Text ?? null;
            string licenseUrl = item.Metadata.LicenseUrl ?? null;

            if (licenseUrl is string && string.IsNullOrWhiteSpace(licenseType))
            {
                if (_licenseMappings.TryGetValue(licenseUrl, out var license))
                {
                    licenseType = license;
                }
            }

            var manual = _packageOptions.ManualInformation
                .FirstOrDefault(f => f.PackageName == item.Metadata.Id && f.PackageVersion == item.Metadata.Version);

            return new LibraryInfo
            {
                PackageName = item.Metadata.Id ?? string.Empty,
                PackageVersion = item.Metadata.Version ?? string.Empty,
                PackageUrl = !string.IsNullOrWhiteSpace(manual?.PackageUrl)
                        ? manual.PackageUrl
                        : item.Metadata.ProjectUrl ?? string.Empty,
                Description = !string.IsNullOrWhiteSpace(manual?.Description)
                        ? manual.Description
                        : item.Metadata.Description ?? string.Empty,
                LicenseType = manual?.LicenseType ?? licenseType ?? string.Empty,
                LicenseUrl = manual?.LicenseUrl ?? licenseUrl ?? string.Empty,
                Projects = _packageOptions.IncludeProjectFile ? projectFile : null
            };
        }

        public void PrintLicenses(List<LibraryInfo> libraries)
        {
            if (libraries is null) { throw new ArgumentNullException(nameof(libraries)); }
            if (!libraries.Any()) { return; }

            WriteOutput(Environment.NewLine + "References:", logLevel: LogLevel.Always);
            WriteOutput(libraries.ToStringTable(new[] { "Reference", "Version", "Licence Type", "License" },
                                                            a => a.PackageName ?? "---",
                                                            a => a.PackageVersion ?? "---",
                                                            a => a.LicenseType ?? "---",
                                                            a => a.LicenseUrl ?? "---"), logLevel: LogLevel.Always);
        }

        public void SaveAsJson(List<LibraryInfo> libraries)
        {
            if (!libraries.Any() || !_packageOptions.JsonOutput) { return; }
            JsonSerializerSettings jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = _packageOptions.IncludeProjectFile ? NullValueHandling.Include : NullValueHandling.Ignore
            };

            using (var fileStream = new FileStream(GetOutputFilename("licenses.json"), FileMode.Create))
            using (var streamWriter = new StreamWriter(fileStream))
            {
                streamWriter.Write(JsonConvert.SerializeObject(libraries, jsonSettings));
                streamWriter.Flush();
            }
        }

        public void SaveAsTextFile(List<LibraryInfo> libraries)
        {
            if (!libraries.Any() || !_packageOptions.TextOutput) { return; }
            StringBuilder sb = new StringBuilder(256);
            foreach (var lib in libraries)
            {
                sb.Append(new string('#', 100));
                sb.AppendLine();
                sb.Append("Package:");
                sb.Append(lib.PackageName);
                sb.AppendLine();
                sb.Append("Version:");
                sb.Append(lib.PackageVersion);
                sb.AppendLine();
                sb.Append("project URL:");
                sb.Append(lib.PackageUrl);
                sb.AppendLine();
                sb.Append("Description:");
                sb.Append(lib.Description);
                sb.AppendLine();
                sb.Append("licenseUrl:");
                sb.Append(lib.LicenseUrl);
                sb.AppendLine();
                sb.Append("license Type:");
                sb.Append(lib.LicenseType);
                sb.AppendLine();
                if (_packageOptions.IncludeProjectFile)
                {
                    sb.Append("Project:");
                    sb.Append(lib.Projects);
                    sb.AppendLine();
                }
                sb.AppendLine();
            }

            File.WriteAllText(GetOutputFilename("licences.txt"), sb.ToString());
        }

        public ValidationResult ValidateLicenses(Dictionary<string, PackageList> projectPackages)
        {
            if (_packageOptions.AllowedLicenseType.Count == 0)
            {
                return new ValidationResult { IsValid = true };
            }

            var invalidPackages = projectPackages
                .SelectMany(kvp => kvp.Value.Select(p => new KeyValuePair<string, Package>(kvp.Key, p.Value)))
                .Where(p => !_packageOptions.AllowedLicenseType.Any(allowed =>
                {
                    if (string.IsNullOrWhiteSpace(allowed)) { return true; }
                    if (p.Value.Metadata.LicenseUrl is string licenseUrl)
                    {
                        if (_licenseMappings.TryGetValue(licenseUrl, out var license))
                        {
                            return allowed == license;
                        }

                        if (p.Value.Metadata.LicenseUrl?.Contains(allowed, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            return true;
                        }
                    }

                    if (p.Value.Metadata.License.IsLicenseFile())
                    {
                        var key = Tuple.Create(p.Value.Metadata.Id, p.Value.Metadata.Version);

                        if (_licenseFileCache.TryGetValue(key, out var licenseText))
                        {
                            if (licenseText.Contains(allowed, StringComparison.OrdinalIgnoreCase))
                            {
                                p.Value.Metadata.License.Text = allowed;
                                return true;
                            }
                        }
                    }

                    return allowed == p.Value.Metadata.License?.Text;
                }))
                .ToList();

            return new ValidationResult { IsValid = invalidPackages.Count == 0, InvalidPackages = invalidPackages };
        }

        private async Task<T> GetNuGetPackageFileResult<T>(string packageName, string versionNumber, string fileInPackage)
            where T : class
        {
            var fallbackEndpoint = new Uri(string.Format(fallbackPackageUrl, packageName, versionNumber));
            using (var packageRequest = new HttpRequestMessage(HttpMethod.Get, fallbackEndpoint))
            using (var packageResponse = await _httpClient.SendAsync(packageRequest))
            {
                if (!packageResponse.IsSuccessStatusCode)
                {
                    WriteOutput($"{packageRequest.RequestUri} failed due to {packageResponse.StatusCode}!", logLevel: LogLevel.Error);
                    return null;
                }

                using (var fileStream = new MemoryStream())
                {
                    await packageResponse.Content.CopyToAsync(fileStream);

                    using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Read))
                    {
                        var entry = archive.GetEntry(fileInPackage);
                        if (entry is null) { return null; }
                        using (var entryStream = entry.Open())
                        using (var textReader = new StreamReader(entryStream))
                        {
                            var typeT = typeof(T);
                            if (typeT == typeof(Package))
                            {
                                if (_serializer.Deserialize(new NamespaceIgnorantXmlTextReader(textReader)) is T result)
                                {
                                    return (T)result;
                                }
                            }
                            else if (typeT == typeof(string))
                            {
                                return await textReader.ReadToEndAsync() as T;
                            }

                            throw new ArgumentException($"{typeT.FullName} isn't supported!");
                        }
                    }
                }
            }
        }

        private IEnumerable<string> GetFilteredProjects(IEnumerable<string> projects)
        {
            if (_packageOptions.ProjectFilter.Count == 0)
            {
                return projects;
            }

            return projects.Where(project => _packageOptions.ProjectFilter
                .Any(projectToSkip =>
                    !project.Contains(projectToSkip, StringComparison.OrdinalIgnoreCase)
                ));
        }

        private async Task HandleLicensing(Package package)
        {
            if (package.Metadata.LicenseUrl is string licenseUrl
                && package.Metadata.License?.Text is null)
            {
                if (_licenseMappings.TryGetValue(licenseUrl, out var mappedLicense))
                {
                    package.Metadata.License = new License { Text = mappedLicense };
                }
            }

            if (!package.Metadata.License.IsLicenseFile()) { return; }

            var key = Tuple.Create(package.Metadata.Id, package.Metadata.Version);

            if (_licenseFileCache.TryGetValue(key, out _)) { return; }

            _licenseFileCache[key] = await GetNuGetPackageFileResult<string>(package.Metadata.Id, package.Metadata.Version, package.Metadata.License.Text);
        }

        private string GetOutputFilename(string defaultName)
        {
            return string.IsNullOrWhiteSpace(_packageOptions.OutputFileName)
               ? defaultName
               : _packageOptions.OutputFileName;
        }

        /// <summary>
        /// Retreive the project references from new csproj file format
        /// </summary>
        /// <param name="projectPath">The Project Path</param>
        /// <returns></returns>
        private IEnumerable<string> GetProjectReferencesFromNewProjectFile(string projectPath)
        {
            var projDefinition = XDocument.Load(projectPath);
            return projDefinition
                         ?.Element("Project")
                         ?.Elements("ItemGroup")
                         ?.Elements("PackageReference")
                         ?.Select(refElem => (refElem.Attribute("Include") == null ? "" : refElem.Attribute("Include").Value) + "," +
                                            (refElem.Attribute("Version") == null ? "" : refElem.Attribute("Version").Value))
                         ?? Array.Empty<string>();
        }

        /// <summary>
        /// Retrieve the project references from old packages.config file
        /// </summary>
        /// <param name="projectPath">The Project Path</param>
        /// <returns></returns>
        private IEnumerable<string> GetProjectReferencesFromPackagesConfig(string projectPath)
        {
            var dir = Path.GetDirectoryName(projectPath);
            var packagesFile = Path.Join(dir, "packages.config");

            if (File.Exists(packagesFile))
            {
                var packagesConfig = XDocument.Load(packagesFile);

                return packagesConfig
                            ?.Element("packages")
                            ?.Elements("package")
                            ?.Select(refElem => (refElem.Attribute("id")?.Value ?? "") + "," + (refElem.Attribute("version")?.Value ?? ""));
            }

            return Array.Empty<string>();
        }

        private IEnumerable<string> GetValidProjects(string projectPath)
        {
            var pathInfo = new FileInfo(projectPath);
            var extension = GetProjectExtension();
            IEnumerable<string> validProjects;
            switch (pathInfo.Extension)
            {
                case ".sln":
                    validProjects = Onion.SolutionParser.Parser.SolutionParser
                        .Parse(pathInfo.FullName)
                        .Projects
                        .Select(p => new FileInfo(Path.Combine(pathInfo.Directory.FullName, p.Path)))
                        .Where(p => p.Exists && p.Extension == extension)
                        .Select(p => p.FullName);
                    break;
                case ".csproj":
                    validProjects = new string[] { projectPath };
                    break;
                default:
                    validProjects = Directory
                        .EnumerateFiles(projectPath, GetProjectExtension(withWildcard: true), SearchOption.AllDirectories);
                    break;
            }

            return GetFilteredProjects(validProjects);
        }

        private void WriteOutput(string line, Exception exception = null, LogLevel logLevel = LogLevel.Information)
        {
            if ((int)logLevel < (int)_packageOptions.LogLevelThreshold)
            {
                return;
            }

            Console.WriteLine(line);

            if (exception is object)
            {
                Console.WriteLine(exception.ToString());
            }
        }
    }
}