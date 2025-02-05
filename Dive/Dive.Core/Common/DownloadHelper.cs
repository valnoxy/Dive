using System;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;

namespace Dive.Core.Common
{
    public class FileDownloader
    {
        private readonly HttpClient _httpClient;
        public FileDownloader()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Downloads a file asynchronously and saves it to the specified path.
        /// </summary>
        /// <param name="url">The URL of the file.</param>
        /// <param name="outputPath">The destination path to save the file.</param>
        /// <param name="progress">Optional: An IProgress type to report progress (0-100).</param>
        /// <returns>A Task representing the download operation.</returns>
        public async Task DownloadFileAsync(string url, string outputPath, IProgress<int>? progress = null)
        {
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = totalBytes > 0 && progress != null;

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var totalRead = 0L;
            var buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalRead += bytesRead;

                if (canReportProgress)
                {
                    var percentage = (int)((totalRead * 100) / totalBytes);
                    progress!.Report(percentage);
                }
            }
        }
    }

    public class FileDownloaderSync
    {
        private readonly FileDownloader _fileDownloader;

        public FileDownloaderSync()
        {
            _fileDownloader = new FileDownloader();
        }

        /// <summary>
        /// Synchronously downloads a file and saves it to the specified path.
        /// </summary>
        /// <param name="url">The URL of the file.</param>
        /// <param name="outputPath">The destination path to save the file.</param>
        /// <param name="progress">Optional: An IProgress type to report progress (0-100).</param>
        public void DownloadFile(string url, string outputPath, IProgress<int>? progress = null)
        {
            _fileDownloader.DownloadFileAsync(url, outputPath, progress).GetAwaiter().GetResult();
        }
    }
}
