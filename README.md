# Nuget License Utility [![Build Status](https://travis-ci.com/tomchavakis/nuget-license.svg?branch=develop)](https://travis-ci.com/tomchavakis/nuget-license.svg?branch=develop) [![NuGet](https://img.shields.io/nuget/v/BeatPulse.svg)]([https://www.nuget.org/packages/dotnet-project-licenses](https://www.nuget.org/packages/dotnet-project-licenses/))


A .net core tool to print the licenses of a project.

## Usage

Usage: dotnet-project-licenses [options]

** Options:

| Option | Description |
|------|-------------|
| `-i` | Project Folder |

## dotnet-project-licenses tool

**Install tool**

```
dotnet tool install --global dotnet-project-licenses

```

**Uninstall tool**

```
dotnet tool uninstall --global dotnet-project-licenses
```

## Using tool

```
dotnet-project-licenses --help

dotnet-project-licenses -i projectFolder
```

**Print unique licenses:**
```
dotnet-project-licenses -i projectFolder -u
```

** Create output file 
```
dotnet-project-licenses -i projectFolder -u -o
```