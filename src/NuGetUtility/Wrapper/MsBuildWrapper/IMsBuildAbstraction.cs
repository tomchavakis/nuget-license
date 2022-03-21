namespace NuGetUtility.Wrapper.MsBuildWrapper
{
    public interface IMsBuildAbstraction
    {
        IEnumerable<PackageReference> GetPackageReferencesFromProjectForFramework(string projectPath, string framework);
        IProject GetProject(string projectPath);
    }
}
