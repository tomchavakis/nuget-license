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
<table>
  <tr><td width="30%">Option</td><td width="70%">Description</td></tr>
  <tr><td> -i, --input </td><td>Project Folder</td></tr>
  <tr><td> -o, --output </td><td>(Default: false) Save as text file (licenses.txt)</td></tr>
  <tr><td> --outfile </td><td>Output filename</td></tr>
  <tr><td> -f, --output-directory </td><td>Output Directory/Folder</td></tr>
  <tr><td> -j, --json </td><td>(Default: false) Save licenses list in a json file (licenses.json)</td></tr>
  <tr><td> -m, --md </td><td>(Default: false) Save licenses list in a markdown file (licenses.md)</td></tr>
  <tr><td> --include-project-file </td><td>(Default: false) Add project file path to information when enabled</td></tr>
  <tr><td> -l, --log-level </td><td>(Default: Error) Set log level for output display. Options: Error,Warning,Information,Verbose</td></tr>
  <tr><td> --allowed-license-types </td><td>Simple json file of a text array of allowable licenses, if no file is given, all are assumed allowed</td></tr>
  <tr><td> --forbidden-license-types </td><td>Simple json file of a text array of allowable licenses, if no file is given, all are assumed allowed</td></tr>
  <tr><td> --manual-package-information</td><td>Simple json file of an array of LibraryInfo objects for manually determined packages</td></tr>
  <tr><td> --licenseurl-to-license-mappings</td><td>Simple json file of Dictionary<string,string> to override default mappings</td></tr>
  <tr><td> -t, --include-transitive </td><td>Include distinct transitive package licenses per project file</td></tr>
  <tr><td> --use-project-assets-json </td><td>Use the resolved project.assets.json file for each project as the source of package information. Requires the <code>-t</code> option since this always includes transitive.references. Requires <code>nuget restore</code> or <code>dotnet restore</code> to be run first</td></tr>
  <tr><td> --projects-filter </td><td>Simple json file of a text array of projects to skip. Supports Ends with matching such as 'Tests.csproj, Tests.vbproj, Tests.fsproj'</td></tr>
  <tr><td> --packages-filter </td><td>Simple json file of a text array of packages to skip. Or a regular expression defined between two forward slashes <code>'/regex/'</code> or two hashes <code>'#regex#'</code></td></tr>
  <tr><td> -u, --unique </td><td>(Default: false) Unique licenses list by Id/Version</td></tr>
  <tr><td> -p, --print </td><td>(Default: true) Print licenses</td></tr>
  <tr><td> -e, --export-license-texts </td><td>Export the raw license texts</td></tr>
  <tr><td> -c, --convert-html-to-text </td><td>Strip HTML tags if the license file is HTML and save as plain text (EXPERIMENTAL)</td></tr>
  <tr><td> --help </td><td>Display this help screen</td></tr>
  <tr><td> --version </td><td>Display version information</td></tr>
  <tr><td> --ignore-ssl-certificate-errors </td><td>Ignore SSL certificate errors in HttpClient</td></tr>
  <tr><td> --timeout </td><td>Set HttpClient timeout in seconds</td></tr>
  <tr><td> --proxy-url </td><td>Set a proxy server URL to be used by HttpClient</td></tr>
  <tr><td> --proxy-system-auth </td><td>Use the system credentials for proxy authentication</td></tr>  
</table>

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
