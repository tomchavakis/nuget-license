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

| <div style="width:250px">Option</div> | Description |
|------|-------------|
| `-i`, `--input` | Project Folder |
| `-o`, `--output` | (Default: false) Save as text file (licenses.txt). |
| `--outfile` | Output filename |
| `-f`, `--output-directory` | Output Directory/Folder. |
| `-j`, `--json` | (Default: false) Save licenses list in a json file (licenses.json). |
| `-m`, `--md` | (Default: false) Save licenses list in a markdown file (licenses.md). |
| `--include-project-file` | (Default: false) Add project file path to information when enabled. |
| `-l`, `--log-level` | (Default: Error) Set log level for output display. Options: Error,Warning,Information,Verbose. |
| `--allowed-license-types` | Simple json file of a text array of allowable licenses, if no file is given, all are assumed allowed. |
| `--manual-package-information` | Simple json file of an array of LibraryInfo objects for manually determined packages. |
| `--licenseurl-to-license-mappings` | Simple json file of Dictionary<string,string> to override default mappings. |
| `-t`, `--include-transitive` | Include distinct transitive package licenses per project file. |
| `--use-project-assets-json` | Use the resolved project.assets.json file for each project as the source of package information. Requires the `-t` option since this always includes transitive.references. Requires `nuget restore` or `dotnet restore` to be run first. |
| `--projects-filter` | Simple json file of a text array of projects to skip. Supports Ends with matching such as 'Tests.csproj, Tests.vbproj, Tests.fsproj'. |
| `--packages-filter` | Simple json file of a text array of packages to skip. Or a regular expression defined between two forward slashes '`/regex/`' or two hashes '`#regex#`'. |
| `-u`, `--unique` | (Default: false) Unique licenses list by Id/Version. |
| `-p`, `--print` | (Default: true) Print licenses. |
| `-e`, `--export-license-texts` | Export the raw license texts |
| `-c`, `--convert-html-to-text` | Strip HTML tags if the license file is HTML and save as plain text (EXPERIMENTAL) | 
| `--help` | Display this help screen. |
| `--version` | Display version information. |
| `--ignore-ssl-certificate-errors` | Ignore SSL certificate errors in HttpClient. |
| `--timeout` | Set HttpClient timeout in seconds. |
| `--proxy-url` | Set a proxy server URL to be used by HttpClient. |
| `--proxy-system-auth` | Use the system credentials for proxy authentication. |

## Example tool commands

```ps
dotnet-project-licenses --help
```

```ps
dotnet-project-licenses -i projectFolder
```

### Print unique licenses

Values for the input may include a folder path, a Visual Studio '.sln' file, a '.csproj' or a '.fsproj' file or a '.vbproj' file.

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

### Exports all license texts in ~/Projects/github directory and output json in ~/Projects/output.json

```ps
dotnet-project-licenses -i projectFolder -o -j -f ~/Projects/github --outfile ~/Projects/output.json --export-license-texts
```

### Exports all license texts in the current directory excluding all Microsoft packages. Licenses in HTML format are saved as plain text files.

```ps
dotnet-project-licenses -i projectFolder --export-license-texts --convert-html-to-text --packages-filter '/Microsoft.*/'
```

### Prints licenses used by a compiled solution excluding all System packages
```ps
dotnet-project-licenses -i projectSolution.sln --use-project-assets-json --packages-filter '#System\..*#'
```

### Use a proxy server when getting nuget package information via http requests

```ps
dotnet-project-licenses -i projectFolder --proxy-url "http://my.proxy.com:8080"
```

### Use a proxy server requiring authentication with the system credentials

```ps
dotnet-project-licenses -i projectFolder --proxy-url "http://my.proxy.com:8080" --proxy-system-auth
```


## Docker

### Build the image
```
docker build . -t nuget-license
```
### Run the image and export the licenses locally
```
docker run -it -v projectPath:/tmp nuget-license -i /tmp -f /tmp --export-license-texts -l Verbose

where projectPath is the path of the project that you want to export the licenses. 
You can also add the command parameters of the tool.

ex.
docker run -it -v ~/Projects/github/nuget-license:/tmp nuget-license -i /tmp -o --export-license-texts -l Verbose
```
