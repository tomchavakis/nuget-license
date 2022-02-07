namespace NuGetUtility.Wrapper.NuGetWrapper.ProjectModel
{
    public interface ILockFile
    {
        IPackageSpec PackageSpec { get; }
        IEnumerable<ILockFileTarget>? Targets { get; }

        IEnumerable<ILockFileLibrary> Libraries { get; }
    }
}
