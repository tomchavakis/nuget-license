// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NuGet.ProjectModel;
using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

namespace NuGetUtility.Wrapper.NuGetWrapper.ProjectModel
{
    internal class WrappedLockFileLibrary : ILockFileLibrary
    {
        private readonly LockFileLibrary _library;

        public WrappedLockFileLibrary(LockFileLibrary library)
        {
            _library = library;
        }

        public string Type => _library.Type;

        public string Name => _library.Name;

        public INuGetVersion Version => new WrappedNuGetVersion(_library.Version);
    }
}
