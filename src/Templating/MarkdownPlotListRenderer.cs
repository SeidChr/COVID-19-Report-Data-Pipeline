namespace Corona.Templating
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class MarkdownPlotListRenderer
    {
        public MarkdownPlotListRenderer(string markdownFolderName)
        {
            Directory.CreateDirectory(markdownFolderName);
            this.MarkdownFolderName = markdownFolderName;
        }

        public string MarkdownFolderName { get; }

        public void RenderPlotListMarkdown(string mdFileName, IEnumerable<string> plotFileNames)
        {
            var mdFilePath = Path.Combine(this.MarkdownFolderName, mdFileName);
            
            var plotNames = plotFileNames
                .Select(f => Path.GetFileNameWithoutExtension(f).Substring(5))
                .ToList();

            using (var writer = File.CreateText(mdFilePath)) 
            {
                var menuTexts = plotNames.Select(n => $"[{n}](#{n}Plot)");
                writer.Write("<a name=\"top\">" + string.Join(" - " + Environment.NewLine, menuTexts) + "</a>" + Environment.NewLine);

                foreach (var name in plotNames)
                {
                    var renderOutput =
$@"<a name=""{name}Plot"">![PLOT {name}](https://bitmonkey.z6.web.core.windows.net/plot-{name}.png)</a>  
<https://bitmonkey.z6.web.core.windows.net/plot-{name}.png>  
[top](#top)  

";
                    writer.Write(renderOutput);
                }

                writer.Flush();
            }
        }
    }
}