// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using System.Net.Http;

namespace NuGetUtility.Wrapper.HttpClientWrapper
{
    public class FileDownloader : IFileDownloader
    {
        private readonly SemaphoreSlim _parallelDownloadLimiter = new SemaphoreSlim(10, 10);
        private readonly HttpClient _client;
        private readonly string _downloadDirectory;
        private const int EXPONENTIAL_BACKOFF_WAIT_TIME_MILLISECONDS = 200;
        private const int MAX_RETRIES = 5;

        public FileDownloader(HttpClient client, string downloadDirectory)
        {
            _client = client;
            _downloadDirectory = downloadDirectory;
        }

        public async Task DownloadFile(Uri url, string fileName, CancellationToken token)
        {
            await _parallelDownloadLimiter.WaitAsync(token);
            try
            {
                for (int i = 0; i < MAX_RETRIES; i++)
                {
                    if (await TryDownload(fileName, url, token))
                    {
                        return;
                    }
                    await Task.Delay((int)Math.Pow(EXPONENTIAL_BACKOFF_WAIT_TIME_MILLISECONDS, i + 1), token);
                }
            }
            finally
            {
                _parallelDownloadLimiter.Release();
            }
        }

#if NETFRAMEWORK
        private async Task<bool> TryDownload(string fileName, Uri url, CancellationToken _)
        {
            using FileStream file = File.OpenWrite(Path.Combine(_downloadDirectory, fileName));
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            // System.Net.HttpStatusCode.TooManyRequests does not exist in .net472
            if (response.StatusCode == (System.Net.HttpStatusCode)429)
            {
                return false;
            }
            response.EnsureSuccessStatusCode();
            using Stream downloadStream = await response.Content.ReadAsStreamAsync();

            await downloadStream.CopyToAsync(file);
            return true;
        }
#else
        private async Task<bool> TryDownload(string fileName, Uri url, CancellationToken token)
        {

            await using FileStream file = File.OpenWrite(Path.Combine(_downloadDirectory, fileName));
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                return false;
            }
            response.EnsureSuccessStatusCode();
            using Stream downloadStream = await response.Content.ReadAsStreamAsync(token);

            await downloadStream.CopyToAsync(file, token);
            return true;
        }
#endif
    }
}
