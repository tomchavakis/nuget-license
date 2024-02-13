// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NuGet.ProjectModel;

namespace NuGetUtility.Wrapper.NuGetWrapper.ProjectModel
{
    internal class WrappedPackageSpec : IPackageSpec
    {
        private readonly PackageSpec? _spec;

        public WrappedPackageSpec(PackageSpec? spec)
        {
            _spec = spec;
        }

        public bool IsValid()
        {
            return _spec != null;
        }

        public IEnumerable<ITargetFrameworkInformation> TargetFrameworks =>
            _spec?.TargetFrameworks.Select(t => new WrappedTargetFrameworkInformation(t)) ??
            Enumerable.Empty<ITargetFrameworkInformation>();
    }
}
