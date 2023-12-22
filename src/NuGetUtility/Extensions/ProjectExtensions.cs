using NuGetUtility.Wrapper.MsBuildWrapper;

namespace NuGetUtility.Extensions
{
    public static class ProjectExtensions
    {
        private const string PackageReferenceValue = "PackageReference";
        private const string PackagesConfigFileName = "packages.config";

        public static bool HasNugetPackagesReferenced(this IProject project)
        {
            return (project.GetPackageReferenceCount() > 0) || project.HasPackagesConfigFile();
        }

        public static bool IsPackageReferenceProject(this IProject project)
        {
            return TargetIsEqualIfSet(project.GetNugetStyleTag(), PackageReferenceValue) &&
                   TargetIsEqualIfSet(project.GetRestoreStyleTag(), PackageReferenceValue) &&
                   !project.HasPackagesConfigFile();
        }

        public static string GetPackagesConfigPath(this IProject project)
        {
            return Path.Join(Path.GetDirectoryName(project.FullPath), PackagesConfigFileName);
        }

        private static bool HasPackagesConfigFile(this IProject project)
        {
            return project.GetEvaluatedIncludes().Any(include => include?.Equals(PackagesConfigFileName) ?? false);
        }

        private static bool TargetIsEqualIfSet(string source, string target)
        {
            return string.IsNullOrEmpty(source) || source.Equals(target);
        }
    }
}
