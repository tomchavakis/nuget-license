namespace NuGetUtility.Wrapper.HttpClientWrapper
{
    public class FileDownloader : IFileDownloader
    {
        private readonly HttpClient _client;
        private readonly string _downloadDirectory;

        public FileDownloader(HttpClient client, string downloadDirectory)
        {
            _client = client;
            _downloadDirectory = downloadDirectory;
        }

        public async Task DownloadFile(Uri url, string fileName)
        {
            await using var file = File.OpenWrite(Path.Combine(_downloadDirectory, fileName));
            await using var downloadStream = await _client.GetStreamAsync(url);

            await downloadStream.CopyToAsync(file);
        }
    }
}
