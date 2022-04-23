# Nuget License Utility [![Build Status](https://travis-ci.com/tomchavakis/nuget-license.svg?branch=develop)](https://travis-ci.com/tomchavakis/nuget-license.svg?branch=develop) [![NuGet](https://img.shields.io/nuget/v/dotnet-project-licenses.svg)](https://www.nuget.org/packages/dotnet-project-licenses)


A .net core tool to print the licenses of a project. This tool supports .NET Core and .NET Standard and .NET Framework Projects.

## dotnet-project-licenses tool

### Install tool

```ps
dotnet tool install --global dotnet-project-licenses

```

### Uninstall tool

```ps
dotnet tool uninstall --global dotnet-project-licenses
```

## Usage

Usage: dotnet-project-licenses [options]

**Options:**

| Option | Description |
|------|-------------|
| `-i, --input` | Project or Solution to be analyzed |
| `-ii, --json-input` | Similar to `-i, --input` but providing a file containing a valid JSON Array that contains all projects to be analyzed |
| `--include-transitive` | When set, the analysis includes transitive packages (dependencies of packages that are directly installed to the project) |
| `-a, --allowed-license-types` | File containing all allowed licenses in JSON format. If omitted, all licenses are considered to be allowed. |
| `-ignore, --ignored-packages` | File containing a JSON formatted array containing package names, that should be ignored when validating licenses. Note that even though a package is ignored, it's transitive dependencies are still validated. This Option is useful e.g. to exclude homegrown nuget packages from validation. |
| `-mapping, --licenseurl-to-license-mappings` | When used, this option allows to override the url to license mapping built into the application (see [here](src/NuGetUtility/LicenseValidator/UrlToLicenseMapping.cs)) |
| `-mapping, --licenseurl-to-license-mappings` | When used, this option allows to override the url to license mapping built into the application (see [here](src/NuGetUtility/LicenseValidator/UrlToLicenseMapping.cs)) |
| `-override, --override-package-information` | When used, this option allows to override the package information used for the validation. This makes sure that no attempt is made to get the associated information about the package from the available web resources. This is useful for packages that e.g. provide a license file as part of the nuget package which (at the time of writing) cannot be used for validation and thus requires the package's information to be provided by this option. |

## Example tool commands

```ps
dotnet-project-licenses --help
```

```ps
dotnet-project-licenses -i project.csproj
```

## Docker

### Build the image
```
docker build . -t nuget-license
```
### Run the same example commands as above in docker
```ps
docker run -it -v projectPath:/tmp nuget-license --help 
```

```ps
docker run -it -v projectPath:/tmp nuget-license -i /tmp/project.csproj 
```
