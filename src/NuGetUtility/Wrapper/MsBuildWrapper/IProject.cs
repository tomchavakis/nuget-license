// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

namespace NuGetUtility.Wrapper.MsBuildWrapper
{
    public interface IProject
    {
        public string FullPath { get; }

        string GetAssetsPath();

        string GetRestoreStyleTag();

        string GetNuGetStyleTag();

        int GetPackageReferenceCount();

        IEnumerable<string> GetEvaluatedIncludes();
    }
}
