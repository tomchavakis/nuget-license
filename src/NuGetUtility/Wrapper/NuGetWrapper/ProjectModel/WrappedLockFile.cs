using NuGet.ProjectModel;

namespace NuGetUtility.Wrapper.NuGetWrapper.ProjectModel
{
    internal class WrappedLockFile : ILockFile
    {
        private readonly LockFile _file;

        public WrappedLockFile(LockFile file)
        {
            _file = file;
        }

        public IEnumerable<ILockFileLibrary> Libraries => _file.Libraries.Select(l => new WrappedLockFileLibrary(l));

        public IPackageSpec PackageSpec => new WrappedPackageSpec(_file.PackageSpec);
        public IEnumerable<ILockFileTarget>? Targets => _file.Targets?.Select(t => new WrappedLockFileTarget(t));
    }
}
