using NuGet.ProjectModel;
using NuGetUtility.Wrapper.NuGetWrapper.Frameworks;

namespace NuGetUtility.Wrapper.NuGetWrapper.ProjectModel
{
    internal class WrappedTargetFrameworkInformation : ITargetFrameworkInformation
    {
        private readonly TargetFrameworkInformation _info;

        public WrappedTargetFrameworkInformation(TargetFrameworkInformation info)
        {
            _info = info;
        }

        public INuGetFramework FrameworkName => new WrappedNuGetFramework(_info.FrameworkName);

        public override string ToString()
        {
            return _info.ToString();
        }
    }
}
