using NuGetUtility.Wrapper.NuGetWrapper.Frameworks;

namespace NuGetUtility.Wrapper.NuGetWrapper.ProjectModel
{
    public interface ILockFileTarget
    {
        INuGetFramework TargetFramework { get; }
    }
}
