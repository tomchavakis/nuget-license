namespace NuGetUtility.Wrapper.NuGetWrapper.ProjectModel
{
    public interface ILockFileFactory
    {
        ILockFile GetFromFile(string path);
    }
}
