using NuGetUtility.Wrapper.NuGetWrapper.Frameworks;

namespace NuGetUtility.Wrapper.NuGetWrapper.ProjectModel
{
    public interface ITargetFrameworkInformation
    {
        INuGetFramework FrameworkName { get; }
    }
}
