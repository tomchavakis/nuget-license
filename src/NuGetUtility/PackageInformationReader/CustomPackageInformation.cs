// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

namespace NuGetUtility.PackageInformationReader
{
    public record struct CustomPackageInformation(string Id, INuGetVersion Version, string License, string? Copyright = null, string? Authors = null, string? Title = null, string? ProjectUrl = null, string? Summary = null, string? Description = null);
}
