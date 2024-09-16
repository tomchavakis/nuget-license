// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

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

        public IEnumerable<ILibraryDependency> Dependencies => _info.Dependencies.Select(library => new WrappedLibraryDependency(library));

        public override string ToString()
        {
            return _info.ToString();
        }
    }
}
