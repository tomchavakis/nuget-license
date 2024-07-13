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
| `-i, --input` | Project or Solution to be analyzed |
| `-ji, --json-input` | Similar to `-i, --input` but providing a file containing a valid JSON Array that contains all projects to be analyzed |
| `-t, --include-transitive` | When set, the analysis includes transitive packages (dependencies of packages that are directly installed to the project) |
| `-a, --allowed-license-types` | File containing all allowed licenses in JSON format. If omitted, all licenses are considered to be allowed. |
| `-ignore, --ignored-packages` | File containing a JSON formatted array containing package names, that should be ignored when validating licenses. Package names specified can contain simple Wildcard characters (*) which are used to match any number of characters. Note that even though a package is ignored, it's transitive dependencies are still validated. This Option is useful e.g. to exclude homegrown nuget packages from validation. |
| `-include-ignored, --include-ignored-packages` | This flag allows to explicitly include ignored packages in the output. |
| `-mapping, --licenseurl-to-license-mappings` | When used, this option allows to add to the url to license mapping built into the application (see [here](src/NuGetUtility/LicenseValidator/UrlToLicenseMapping.cs)) |
| `-override, --override-package-information` | When used, this option allows to override the package information used for the validation. This makes sure that no attempt is made to get the associated information about the package from the available web resources. This is useful for packages that e.g. provide a license file as part of the nuget package which (at the time of writing) cannot be used for validation and thus requires the package's information to be provided by this option. |
| `-d, --license-information-download-location` | When used, this option downloads the html content of the license URL to the specified folder. This is done for all NuGet packages that specify a license URL instead of providing the license expression. |
| `-o, --output` | This Parameter accepts the value `table`, `json` or `jsonPretty`. It allows to select the type of output that should be given. If omitted, the output is given in tabular form. |
| `-err, --error-only` | This flag allows to print only packages that contain validation errors (if there are any). This allows the user to focus on errors instead of having to deal with many properly validated |
| `-?, -h, --help` | Show help for the application and exit |
| `--version` | Show version information of the application and exit |

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
