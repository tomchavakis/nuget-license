namespace NuGetUtility.Wrapper.NuGetWrapper.ProjectModel
{
    public interface IPackageSpec
    {
        public IEnumerable<ITargetFrameworkInformation> TargetFrameworks { get; }
        bool IsValid();
    }
}
