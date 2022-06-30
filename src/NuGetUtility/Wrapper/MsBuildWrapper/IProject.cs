namespace NuGetUtility.Wrapper.MsBuildWrapper
{
    public interface IProject
    {
        string GetAssetsPath();

        string GetRestoreStyleTag();

        string GetNugetStyleTag();

        int GetPackageReferenceCount();

        IEnumerable<string> GetEvaluatedIncludes();
    }
}
