namespace NuGetUtility.Wrapper.HttpClientWrapper
{
    public class NopFileDownloader : IFileDownloader
    {
        public Task DownloadFile(Uri url, string fileName, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
