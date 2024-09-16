// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NuGet.LibraryModel;

namespace NuGetUtility.Wrapper.NuGetWrapper.ProjectModel
{
    internal class WrappedLibraryDependency : ILibraryDependency
    {
        private readonly LibraryDependency _dependency;

        public WrappedLibraryDependency(LibraryDependency dependency)
        {
            _dependency = dependency;
        }
        public string Name => _dependency.Name;
    }
}
