using System;

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
                    await using FileStream file = File.OpenWrite(Path.Combine(_downloadDirectory, fileName));
                    var request = new HttpRequestMessage(HttpMethod.Get, url);

                    HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
                    response.EnsureSuccessStatusCode();
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        await Task.Delay((int)Math.Pow(EXPONENTIAL_BACKOFF_WAIT_TIME_MILLISECONDS, i + 1), token);
                        continue;
                    }
                    using Stream downloadStream = await response.Content.ReadAsStreamAsync(token);

                    await downloadStream.CopyToAsync(file, token);
                    return;
                }
            }
            finally
            {
                _parallelDownloadLimiter.Release();
            }
        }
    }
}
