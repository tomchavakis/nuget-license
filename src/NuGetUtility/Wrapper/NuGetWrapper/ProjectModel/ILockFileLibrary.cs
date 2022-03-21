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
