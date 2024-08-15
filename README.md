# Nuget License Utility [![Tests](https://github.com/sensslen/nuget-license/actions/workflows/action.yml/badge.svg)](https://github.com/sensslen/nuget-license/actions/workflows/action.yml) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=sensslen_nuget-license&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=sensslen_nuget-license) [![NuGet](https://img.shields.io/nuget/v/nuget-license.svg)](https://www.nuget.org/packages/nuget-license)

A .net tool to print and validate the licenses of .net code. This tool supports .NET (Core), .NET Standard and .NET Framework projects. Native c++ projects are supported too but only in the .NET Framework variant of nuget-license. These projects will not work if the tool is installed via `dotnet tool install`.

## nuget-license tool

### Install tool

```ps
dotnet tool install --global nuget-license

```

### Uninstall tool

```ps
dotnet tool uninstall --global nuget-license
```

## Usage

Usage: nuget-license [options]

**Options:**

| Option | Description               |
| ------ | ------------------------- |
| `--version` | Show version information. |
| `-i\|--input <INPUT_FILE>` | The project (or solution) file for which to analyze dependency licenses |
| `-ji\|--json-input <INPUT_JSON_FILE>` | File in json format that contains an array of all files to be evaluated. The Files can either point to a project or a solution. |
| `-t\|--include-transitive` | If set, the whole license tree is followed in order to determine all nuget's used by the projects |
| `-a\|--allowed-license-types <ALLOWED_LICENSES>` | File in json format that contains an array of all allowed license types |
| `-ignore\|--ignored-packages <IGNORED_PACKAGES>` | File in json format that contains an array of nuget package names to ignore (e.g. useful for nuget packages built in-house). Note that even though the packages are ignored, their transitive dependencies are not. Wildcard characters (*) are supported to specify ranges of ignored packages. |
| `-mapping\|--licenseurl-to-license-mappings <LICENSE_MAPPING>` | File in json format that contains a dictionary to map license urls to licenses. |
| `-override\|--override-package-information <OVERRIDE_PACKAGE_INFORMATION>` | File in json format that contains a list of package and license information which should be used in favor of the online version. This option can be used to override the license type of packages that e.g. specify the license as file. |
| `-d\|--license-information-download-location <DOWNLOAD_LICENSE_INFORMATION>` | When set, the application downloads all licenses given using a license URL to the specified folder. |
| `-o\|--output <OUTPUT_TYPE>` | This parameter allows to choose between tabular and json output. Allowed values are: Table, Json, JsonPretty. Default value is: Table. |
| `-err\|--error-only` | If this option is set and there are license validation errors, only the errors are returned as result. Otherwise all validation results are always returned. |
| `-include-ignored\|--include-ignored-packages` | If this option is set, the packages that are ignored from validation are still included in the output. |
| `-exclude-projects\|--exclude-projects-matching <EXCLUDED_PROJECTS>` | This option allows to specify project name(s) to exclude from the analysis. This can be useful to exclude test projects from the analysis when supplying a solution file as input. Wildcard characters (*) are supported to specify ranges of ignored projects. The input can either be a file name containing a list of project names in json format or a plain string that is then used as a single entry. |
| `-?\|-h\|--help` | Show help information. |

## Example tool commands

### Show help

```ps
nuget-license --help
```

### Validate licenses for .csproj file

```ps
nuget-license -i project.csproj
```

### Generate machine readable output

```ps
nuget-license -i project.csproj -o jsonPretty
```
