// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

namespace NuGetUtility.Wrapper.NuGetWrapper.ProjectModel
{
    public interface ILockFileLibrary
    {
        string Type { get; }
        string Name { get; }
        INuGetVersion Version { get; }
    }
}
