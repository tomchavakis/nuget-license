namespace NuGetUtility.Wrapper.MsBuildWrapper
{
    public interface IProject
    {
        public string FullPath { get; }
        string GetAssetsPath();

        string GetRestoreStyleTag();

        string GetNugetStyleTag();

        int GetPackageReferenceCount();

        IEnumerable<string> GetEvaluatedIncludes();
    }
}
