#nullable enable
#pragma warning disable SA1310 // underscore part of parsed json
#pragma warning disable SA1309 // leading underscore part of parsed json
namespace Corona.GithubClient.Model
{
    public struct FileInfo
    {
        public string Name;

        public string Type;

        public string Download_Url;

        public LinkFields _links;
    }
}