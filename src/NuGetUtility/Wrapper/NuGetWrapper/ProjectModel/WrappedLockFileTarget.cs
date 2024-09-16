// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

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

        public IEnumerable<ILockFileLibrary> Libraries => _target.Libraries.Select(l => new WrappedLockFileTargetLibrary(l));
    }
}
