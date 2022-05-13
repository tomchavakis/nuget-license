namespace NuGetUtility.Wrapper.MsBuildWrapper
{
    public interface IProject
    {
        string GetAssetsPath();

        bool HasNugetPackagesReferenced();

        bool IsNotPackageReferenceProject();
    }
}
