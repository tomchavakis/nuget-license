using NuGet.ProjectModel;

namespace NuGetUtility.Wrapper.NuGetWrapper.ProjectModel
{
    public class LockFileFactory : ILockFileFactory
    {
        private readonly LockFileFormat _format = new LockFileFormat();

        public ILockFile GetFromFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new NugetWrapperException(
                    $"Project assets file was not found for project {path}.\nPlease execute a restore command before executing.");
            }

            return new WrappedLockFile(_format.Read(path));
        }
    }
}
