using NuGetUtility.Wrapper.MsBuildWrapper;

namespace NuGetUtility.ReferencedPackagesReader
{
    public class ProjectsCollector
    {
        private readonly IMsBuildAbstraction _msBuild;
        public ProjectsCollector(IMsBuildAbstraction msBuild)
        {
            _msBuild = msBuild;
        }

        public IEnumerable<string> GetProjects(string inputPath)
        {
            return Path.GetExtension(inputPath).Equals(".sln")
                ? _msBuild.GetProjectsFromSolution(inputPath).Where(File.Exists)
                : new List<string>(new[] { inputPath });
        }
    }
}
