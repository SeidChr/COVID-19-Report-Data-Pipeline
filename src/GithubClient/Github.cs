#nullable enable
namespace Corona.GithubClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class Github
    {
        private readonly string? accessToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="Github"/> class.
        /// </summary>
        /// <param name="accessToken">option github access token for private repos.</param>
        public Github(string? accessToken = null)
        {
            this.accessToken = accessToken;
        }

        private Lazy<HttpClient> Client { get; set; } = new Lazy<HttpClient>();

        /// <summary>
        /// Fetches files in a directory.
        /// </summary>
        /// <param name="respositoryOwner">Owner of the repository.</param>
        /// <param name="repositoryName">Name of the repository.</param>
        /// <param name="directoryPath">relative path inside of the repository.</param>
        /// <param name="includeDirectories">Also return directories (type="dir").</param>
        /// <returns>Enumeration of file-infos.</returns>
        public async Task<IEnumerable<Model.FileInfo>> GetFileInfoAsync(
            string respositoryOwner,
            string repositoryName,
            string directoryPath = "/",
            bool includeDirectories = false)
        {
            var baseUri = new Uri(
                $"https://api.github.com/repos/{respositoryOwner}/{repositoryName}/contents/");
            var uri = new Uri(baseUri, directoryPath);
            var content = await this.GetResponseData(uri);

            // System.Console.WriteLine(jsonStr);
            IEnumerable<Model.FileInfo> result
                = JsonConvert.DeserializeObject<Model.FileInfo[]>(content);

            if (includeDirectories)
            {
                result = result.Where(f => f.Type != "dir");
            }

            return result;
        }

        /// <summary>
        /// Fetches the content of a file descibed by a fileInfo.
        /// </summary>
        /// <param name="fileInfo">File to fetch content of.</param>
        /// <returns>File data.</returns>
        public async Task<Model.FileData> GetFileDataAsync(
            Model.FileInfo fileInfo)
            => new Model.FileData
            {
                Name = fileInfo.Name,
                Contents = await this.GetResponseData(fileInfo.Download_Url),
            };

        private static void AttachHeaders(HttpRequestMessage requestMessage, string? authToken)
        {
            if (authToken != null)
            {
                var basicAuth = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes(
                        authToken + ":x-oauth-basic"));

                requestMessage.Headers.Add(
                    "Authorization",
                    "Basic " + basicAuth);
            }

            requestMessage.Headers.Add("User-Agent", "github-api-client");
        }

        private async Task<string> GetResponseData(string url)
            => await this.GetResponseData(new Uri(url));

        private async Task<string> GetResponseData(Uri url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            AttachHeaders(request, this.accessToken);

            var response = await this.Client.Value.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            response.Dispose();

            return content;
        }
    }
}