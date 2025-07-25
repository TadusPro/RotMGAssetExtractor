using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using System.Xml.Linq;

namespace RotMGAssetExtractor.Downloading
{
    public class Downloader
    {
        private readonly HttpClient _httpClient;

        public Downloader()
        {
            _httpClient = new HttpClient();
        }

        public async Task<(string baseCdnUrl, string buildHash)> FetchBuildInfoAsync()
        {
            const string appInitUrl = "https://www.realmofthemadgod.com/app/init?platform=standalonewindows64&key=9KnJFxtTvLu2frXv";
            var postData = "game_net=rotmg&game_net=Unity&play_platform=Unity&game_net_user_id=";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, appInitUrl)
                {
                    Content = new StringContent(postData, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded")
                };

                request.Headers.Add("X-Unity-Version", "2021.3.16f1");
                request.Headers.Add("User-Agent", "UnityPlayer/2021.3.16f1 (UnityWebRequest/1.0, libcurl/7.84.0-DEV)");

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                // Parse XML
                var doc = XDocument.Parse(responseString);
                var baseCdnUrl = doc.Root.Element("BuildCDN")?.Value;
                var buildHash = doc.Root.Element("BuildHash")?.Value;

                if (!string.IsNullOrEmpty(baseCdnUrl) && !baseCdnUrl.EndsWith("/"))
                {
                    baseCdnUrl += "/";
                }

                return (baseCdnUrl, buildHash);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GameData] Failed to fetch build info: {ex.Message}");
                return (null, null); 
            }
        }

        public async Task<List<FileData>> FetchFileListAsync(string baseCdnUrl, string buildHash)
        {
            var baseFilesUrl = $"{baseCdnUrl}{buildHash}/rotmg-exalt-win-64";
            var checksumUrl = $"{baseFilesUrl}/checksum.json";

            try
            {
                // Set headers (if not already set)
                if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
                    _httpClient.DefaultRequestHeaders.Add("User-Agent", "UnityPlayer/2021.3.16f1 (UnityWebRequest/1.0, libcurl/7.84.0-DEV)");
                if (!_httpClient.DefaultRequestHeaders.Contains("X-Unity-Version"))
                    _httpClient.DefaultRequestHeaders.Add("X-Unity-Version", "2021.3.16f1");
                if (!_httpClient.DefaultRequestHeaders.Accept.Contains(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*")))
                    _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
                if (!_httpClient.DefaultRequestHeaders.AcceptEncoding.Contains(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip")))
                    _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                if (!_httpClient.DefaultRequestHeaders.AcceptEncoding.Contains(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate")))
                    _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));

                var response = await _httpClient.GetAsync(checksumUrl);
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[GameData] Failed to fetch file list: {response.StatusCode} - {response.ReasonPhrase}");
                    return new List<FileData>();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var fileListResponse = JsonSerializer.Deserialize<FileListResponse>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                var fileList = new List<FileData>();
                if (fileListResponse?.Files != null)
                {
                    foreach (var file in fileListResponse.Files)
                    {
                        // If file.file contains subfolders, extract only the filename
                        string fileName = file.file;
                        if (fileName.Contains("/"))
                            fileName = fileName.Substring(fileName.LastIndexOf("/") + 1);

                        fileList.Add(new FileData
                        {
                            file = fileName,
                            url = $"{baseFilesUrl}/{Uri.EscapeDataString(file.file)}.gz"
                        });
                    }
                }

                return fileList;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GameData] Failed to fetch or parse file list: {ex.Message}");
                return new List<FileData>();
            }
        }

        public async Task<byte[]> DownloadAndDecompressFileAsync(string url)
        {
            try
            {
                var compressedData = await _httpClient.GetByteArrayAsync(url);

                using var compressedStream = new MemoryStream(compressedData);
                using var decompressionStream = new GZipStream(compressedStream, CompressionMode.Decompress);
                using var resultStream = new MemoryStream();
                await decompressionStream.CopyToAsync(resultStream);
                return resultStream.ToArray();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GameData] Failed to download or decompress file: {ex.Message}");
                throw;
            }
        }
    }

    public class FileData
    {
        public string file { get; set; }
        public string url { get; set; }
    }

    public class FileListResponse
    {
        public List<FileData> Files { get; set; }
    }
}