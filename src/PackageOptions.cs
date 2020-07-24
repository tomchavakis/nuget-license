using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;
using System.Linq;
using static NugetUtility.Utilties;

namespace NugetUtility
{
    public class PackageOptions
    {
        private ICollection<string> _allowedLicenseTypes;
        private ICollection<LibraryInfo> _manualInformation;
        private ICollection<string> _projectFilter;
        private ICollection<string> _packagesFilter;
        private Dictionary<string, string> _customLicenseToUrlMappings;

        [Option("allowed-license-types", Default = null, HelpText = "Simple json file of a text array of allowable licenses, if no file is given, all are assumed allowed")]
        public string AllowedLicenseTypesOption { get; set; }

        [Option('j', "json", Default = false, HelpText = "Saves licenses list in a json file (licenses.json)")]
        public bool JsonOutput { get; set; }

        [Option("include-project-file", Default = false, HelpText = "Adds project file path to information when enabled.")]
        public bool IncludeProjectFile { get; set; }

        [Option('l', "log-level", Default = LogLevel.Error, HelpText = "Sets log level for output display. Options: Error|Warning|Information|Verbose.")]
        public LogLevel LogLevelThreshold { get; set; }

        [Option("manual-package-information", Default = null, HelpText = "Simple json file of an array of LibraryInfo objects for manually determined packages.")]
        public string ManualInformationOption { get; set; }

        [Option("licenseurl-to-license-mappings", Default = null, HelpText = "Simple json file of Dictinary<string,string> to override default mappings")]
        public string LicenseToUrlMappingsOption { get; set; }

        [Option('o', "output", Default = false, HelpText = "Savas as text file (licenses.txt)")]
        public bool TextOutput { get; set; }

        [Option("outfile", Default = null, HelpText = "Output filename")]
        public string OutputFileName { get; set; }

        [Option('i', "input", HelpText = "The projects in which to search for used nuget packages. This can either be a folder, a project file, a solution file or a json file containing a list of projects.")]
        public string ProjectDirectory { get; set; }

        [Option("projects-filter", Default = null, HelpText = "Simple json file of a text array of projects to skip. Supports Ends with matching such as 'Tests.csproj'")]
        public string ProjectsFilterOption { get; set; }

        [Option("packages-filter", Default = null, HelpText = "Simple json file of a text array of packages to skip.")]
        public string PackagesFilterOption { get; set; }

        [Option('u', "unique", Default = false, HelpText = "Unique licenses list by Id/Version")]
        public bool UniqueOnly { get; set; }

        [Option('p', "print", Default = true, HelpText = "Print licenses.")]
        public bool? Print { get; set; }

        [Option("export-license-texts", Default = false, HelpText = "Exports the raw license texts")]
        public bool ExportLicenseTexts { get; set; }

        [Option("include-transitive", Default = false, HelpText = "Include distinct transitive package licenses per project file.")]
        public bool IncludeTransitive { get; set; }

        [Usage(ApplicationAlias = "dotnet-project-licenses")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
                    new Example("Simple", new PackageOptions { ProjectDirectory = "~/Projects/test-project" }),
                    new Example("VS Solution", new PackageOptions { ProjectDirectory = "~/Projects/test-project/project.sln" }),
                    new Example("Unique VS Solution to Custom JSON File", new PackageOptions
                    {
                        ProjectDirectory = "~/Projects/test-project/project.sln",
                        UniqueOnly = true,
                        JsonOutput = true,
                        OutputFileName = @"~/Projects/another-folder/licenses.json"
                    }),
                };
            }
        }

        public ICollection<string> AllowedLicenseType
        {
            get
            {
                if (_allowedLicenseTypes is object) { return _allowedLicenseTypes; }

                return _allowedLicenseTypes = ReadListFromFile<string>(AllowedLicenseTypesOption);
            }
        }

        public ICollection<LibraryInfo> ManualInformation
        {
            get
            {
                if (_manualInformation is object) { return _manualInformation; }

                return _manualInformation = ReadListFromFile<LibraryInfo>(ManualInformationOption);
            }
        }

        public ICollection<string> ProjectFilter
        {
            get
            {
                if (_projectFilter is object) { return _projectFilter; }

                return _projectFilter = ReadListFromFile<string>(ProjectsFilterOption)
                    .Select(x => x.EnsureCorrectPathCharacter())
                    .ToList();
            }
        }

        public ICollection<string> PackageFilter
        {
            get
            {
                if (_packagesFilter is object) { return _packagesFilter; }

                return _packagesFilter = ReadListFromFile<string>(PackagesFilterOption);
            }
        }

        public IReadOnlyDictionary<string, string> LicenseToUrlMappingsDictionary
        {
            get
            {
                if (_customLicenseToUrlMappings is object) { return _customLicenseToUrlMappings; }

                return _customLicenseToUrlMappings = ReadDictionaryFromFile(LicenseToUrlMappingsOption, LicenseToUrlMappings.Default);
            }
        }
    }
}
