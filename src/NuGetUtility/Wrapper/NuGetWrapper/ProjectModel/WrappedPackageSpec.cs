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
