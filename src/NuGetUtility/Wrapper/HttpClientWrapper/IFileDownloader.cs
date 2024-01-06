namespace NuGetUtility.Wrapper.HttpClientWrapper
{
    public interface IFileDownloader
    {
        public Task DownloadFile(Uri url, string fileName, CancellationToken token);
    }
}
