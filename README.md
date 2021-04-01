# Nuget License Utility [![Build Status](https://travis-ci.com/tomchavakis/nuget-license.svg?branch=develop)](https://travis-ci.com/tomchavakis/nuget-license.svg?branch=develop) [![NuGet](https://img.shields.io/nuget/v/dotnet-project-licenses.svg)](https://www.nuget.org/packages/dotnet-project-licenses)


A .net core tool to print the licenses of a project. This tool support .NET Core and .NET Standard Projects.

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
| `-i, --input` | Project Folder |
| `--allowed-license-types` | Simple json file of a text array of allowable licenses, if no file is given, all are assumed allowed |
| `-j, --json` | (Default: false) Saves licenses list in a json file (licenses.json) |
| `--include-project-file` | (Default: false) Adds project file path to information when enabled. |
| `-l, --log-level` | (Default: Error) Sets log level for output display. Options: Error,Warning,Information,Verbose. |
| `--manual-package-information` | Simple json file of an array of LibraryInfo objects for manually determined packages. |
| `--licenseurl-to-license-mappings` | Simple json file of Dictinary<string,string> to override default mappings |
| `--include-transitive` | Include distinct transitive package licenses per project file. |
| `-o, --output` | (Default: false) Saves as text file (licenses.txt) |
| `--outfile` | Output filename |
| `--projects-filter` | Simple json file of a text array of projects to skip. Supports Ends with matching such as 'Tests.csproj' |
| `--packages-filter` | Simple json file of a text array of packages to skip. Or a regular expression defined between two forward slashes '`/regex/`'. |
| `-u, --unique` | (Default: false) Unique licenses list by Id/Version |
| `-p, --print` | (Default: true) Print licenses. |
| `--export-license-texts` | Exports the raw license texts |
| `--help` | Display this help screen. |
| `--version` | Display version information. |

## Example tool commands

```ps
dotnet-project-licenses --help
```

```ps
dotnet-project-licenses -i projectFolder
```

### Print unique licenses

Values for the input may include a folder path, a Visual Studio '.sln' file, a '.csproj' or a '.fsproj' file.

```ps
dotnet-project-licenses -i projectFolder -u
```

### Creates output file of unique licenses in a plain text 'licenses.txt' file in current directory

```ps
dotnet-project-licenses -i projectFolder -u -o
```

### Create output file 'new-name.txt' in another directory

```ps
dotnet-project-licenses -i projectFolder -o --outfile ../../../another/folder/new-name.txt
```

### Creates output json file of unique licenses in a file 'licenses.json' in the current directory

```ps
dotnet-project-licenses -i projectFolder -u -o -j
```

### Exports all license texts in the current directory

```ps
dotnet-project-licenses -i projectFolder --export-license-texts
```

### Exports all license texts in the current directory excluding all Microsoft packages

```ps
dotnet-project-licenses -i projectFolder --export-license-texts --packages-filter '/Microsoft.*/'
```