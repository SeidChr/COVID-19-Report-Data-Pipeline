#nullable enable
namespace Corona.GithubClient.Model
{
    using System.Collections.Generic;

    public struct Directory
    {
        public string Name;

        public List<Directory> Directories;

        public List<FileData> Files;
    }
}