# Nuget License Utility [![Build Status](https://travis-ci.com/tomchavakis/nuget-license.svg?branch=develop)](https://travis-ci.com/tomchavakis/nuget-license.svg?branch=develop) [![NuGet](https://img.shields.io/nuget/v/dotnet-project-licenses.svg)](https://www.nuget.org/packages/dotnet-project-licenses)

A .net core tool to print the licenses of a project. This tool supports .NET Core and .NET Standard and .NET Framework
Projects.

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

| Option                                        | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
|-----------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `-i, --input`                                 | Project or Solution to be analyzed                                                                                                                                                                                                                                                                                                                                                                                                                                    |
| `-ji, --json-input`                           | Similar to `-i, --input` but providing a file containing a valid JSON Array that contains all projects to be analyzed                                                                                                                                                                                                                                                                                                                                                 |
| `-t, --include-transitive`                    | When set, the analysis includes transitive packages (dependencies of packages that are directly installed to the project)                                                                                                                                                                                                                                                                                                                                             |
| `-a, --allowed-license-types`                 | File containing all allowed licenses in JSON format. If omitted, all licenses are considered to be allowed.                                                                                                                                                                                                                                                                                                                                                           |
| `-ignore, --ignored-packages`                 | File containing a JSON formatted array containing package names, that should be ignored when validating licenses. Note that even though a package is ignored, it's transitive dependencies are still validated. This Option is useful e.g. to exclude homegrown nuget packages from validation.                                                                                                                                                                       |
| `-mapping, --licenseurl-to-license-mappings`  | When used, this option allows to override the url to license mapping built into the application (see [here](src/NuGetUtility/LicenseValidator/UrlToLicenseMapping.cs))                                                                                                                                                                                                                                                                                                |
| `-override, --override-package-information`   | When used, this option allows to override the package information used for the validation. This makes sure that no attempt is made to get the associated information about the package from the available web resources. This is useful for packages that e.g. provide a license file as part of the nuget package which (at the time of writing) cannot be used for validation and thus requires the package's information to be provided by this option.            |
| `-d, --license-information-download-location` | When used, this option downloads the html content of the license URL to the specified folder. This is done for all NuGet packages that specify a license URL instead of providing the license expression.                                                                                                                                                                                                                                                             |
| `-o, --output`                                | This Parameter accepts the value `table` or `json`. It allows to select the type of output that should be given. If omitted, the output is given in tabular form.                                                                                                                                                                                                                                                                                                     |
| `-v, --output-verbosity`                      | This Parameter accepts the value `standard` or `includeIgnoredPackages`. It allows to select the verbosity of the output. The Parameters lead to the following: <br/>- **standard**: print all referenced packages with their respective license information and the origin of said.<br/>- **includeIgnoredPackages**: Print all the information printed when using `standard` and add all ignored packages to the output specifying Ignored as their license origin. |

## Example tool commands

### Show help

```ps
dotnet-project-licenses --help
```

### Validate licenses for .csproj file

```ps
dotnet-project-licenses -i project.csproj
```

### Generate machine readable output

```ps
dotnet-project-licenses -i project.csproj -o json
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
