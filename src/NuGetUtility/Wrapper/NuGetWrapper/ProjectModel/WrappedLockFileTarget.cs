using NuGet.ProjectModel;
using NuGetUtility.Wrapper.NuGetWrapper.Frameworks;

namespace NuGetUtility.Wrapper.NuGetWrapper.ProjectModel
{
    internal class WrappedLockFileTarget : ILockFileTarget
    {
        private readonly LockFileTarget _target;

        public WrappedLockFileTarget(LockFileTarget target)
        {
            _target = target;
        }

        public INuGetFramework TargetFramework => new WrappedNuGetFramework(_target.TargetFramework);
    }
}
