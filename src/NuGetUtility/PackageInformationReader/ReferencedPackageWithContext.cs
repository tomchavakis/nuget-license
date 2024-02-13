// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NuGetUtility.Wrapper.NuGetWrapper.Packaging;

namespace NuGetUtility.PackageInformationReader
{
    public record ReferencedPackageWithContext(string Context, IPackageMetadata PackageInfo);
}
