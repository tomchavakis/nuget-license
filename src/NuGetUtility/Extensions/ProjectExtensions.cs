// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NuGetUtility.Wrapper.MsBuildWrapper;

namespace NuGetUtility.Extensions
{
    public static class ProjectExtensions
    {
        private const string PackagesConfigFileName = "packages.config";

        public static string GetPackagesConfigPath(this IProject project)
        {
            return Path.Combine(Path.GetDirectoryName(project.FullPath) ?? string.Empty, PackagesConfigFileName);
        }

        public static bool HasPackagesConfigFile(this IProject project)
        {
            return project.GetEvaluatedIncludes().Any(include => include?.Equals(PackagesConfigFileName) ?? false);
        }
    }
}
