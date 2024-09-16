// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

namespace NuGetUtility.Wrapper.MsBuildWrapper
{
    public interface IMsBuildAbstraction
    {
        IProject GetProject(string projectPath);
        IEnumerable<string> GetProjectsFromSolution(string inputPath);
    }
}
