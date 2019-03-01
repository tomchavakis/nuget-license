# Nuget License Utility

A .net core tool to get the licenses of a project

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