// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NuGet.ProjectModel;
using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

namespace NuGetUtility.Wrapper.NuGetWrapper.ProjectModel
{
    internal class WrappedLockFileTargetLibrary : ILockFileLibrary
    {
        public WrappedLockFileTargetLibrary(LockFileTargetLibrary library)
        {
            Type = library.Type ?? throw new ArgumentNullException(nameof(library), $"The field {nameof(library.Type)} on {nameof(library)} must not be null");
            Name = library.Name ?? throw new ArgumentNullException(nameof(library), $"The field {nameof(library.Name)} on {nameof(library)} must not be null");
            Version = new WrappedNuGetVersion(library.Version ?? throw new ArgumentNullException(nameof(library), $"The field {nameof(library.Version)} on {nameof(library)} must not be null"));
        }

        public string Type { get; }

        public string Name { get; }

        public INuGetVersion Version { get; }
    }
}
