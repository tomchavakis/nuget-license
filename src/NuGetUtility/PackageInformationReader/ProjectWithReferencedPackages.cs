// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;

namespace NuGetUtility.PackageInformationReader
{
    public record ProjectWithReferencedPackages(string Project, IEnumerable<PackageIdentity> ReferencedPackages);
}
