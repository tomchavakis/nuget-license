// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

namespace NuGetUtility.Wrapper.NuGetWrapper.ProjectModel
{
    public interface ILockFile
    {
        IPackageSpec PackageSpec { get; }
        IEnumerable<ILockFileTarget>? Targets { get; }

        IEnumerable<ILockFileLibrary> Libraries { get; }
    }
}
