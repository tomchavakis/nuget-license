namespace NuGetUtility.Wrapper.MsBuildWrapper
{
    public interface IMsBuildAbstraction
    {
        IEnumerable<PackageReference> GetPackageReferencesFromProjectForFramework(IProject project, string framework);
        IProject GetProject(string projectPath);
        IEnumerable<string> GetProjectsFromSolution(string inputPath);
    }
}
