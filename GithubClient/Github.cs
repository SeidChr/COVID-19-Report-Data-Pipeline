namespace Corona.GithubClient
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// Github classes.
    /// </summary>
    public class Github
    {
        private readonly string accessToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="Github"/> class.
        /// </summary>
        /// <param name="accessToken">option github access token for private repos.</param>
        public Github(string accessToken = null)
        {
            this.accessToken = accessToken;
        }

        private Lazy<HttpClient> Client { get; set; } = new Lazy<HttpClient>();

        /// <summary>
        /// recursively get the contents of all files and subdirectories within a directory
        /// based on https://gist.github.com/EvanSnapp/ddf7f7f793474ea9631cbc0960295983 .
        /// </summary>
        /// <param name="respositoryOwner">github account.</param>
        /// <param name="repositoryName">github repo.</param>
        /// <param name="directoryPath">path in repo.</param>
        /// <param name="readFilePattern">regex pattern for file to be downloaded.</param>
        /// <param name="recursive">check sub directories.</param>
        /// <returns>Directory-Model.</returns>
        public async Task<Model.Directory> DumpDirectoryAsync(
            string respositoryOwner,
            string repositoryName,
            string directoryPath = "/",
            string readFilePattern = @".*",
            bool recursive = false)
        {
            var dirContents = await this.GetFileInfoAsync(
                respositoryOwner,
                repositoryName,
                directoryPath,
                true);

            // read in data
            Model.Directory result = default;
            result.Name = "root";
            result.Directories = new List<Model.Directory>();
            result.Files = new List<Model.FileData>();

            foreach (Model.FileInfo file in dirContents)
            {
                if (file.Type == "dir")
                {
                    if (recursive)
                    {
                        Model.Directory sub = await this.DumpDirectoryAsync(
                            respositoryOwner,
                            repositoryName,
                            Path.Combine(directoryPath, file.Name),
                            readFilePattern,
                            recursive);

                        result.Directories.Add(sub);
                    }
                    else
                    {
                        result.Directories.Add(
                            new Model.Directory
                            {
                                Name = file.Name,
                            });
                    }
                }
                else
                {
                    if (Regex.Match(file.Name, readFilePattern).Success)
                    {
                        result.Files.Add(await this.GetFileDataAsync(file));
                    }
                }
            }

            return result;
        }

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

        private static void AttachHeaders(HttpRequestMessage requestMessage, string authToken)
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