#nullable enable
namespace Corona
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Corona.CsvModel;
    using Corona.GithubClient;
    using Corona.Plotting.Model;
    using Corona.Shared;
    using Corona.Templating;
    using FileHelpers;
    using global::Plotting;
    using ScottPlot;

    /// <summary>
    /// Main Programm class.
    /// </summary>
    public class DailyReportPipeline
    {
        // filter regions for combined plot: existing
        public const int MinExisting = 5000;

        // filter regions for combined plot: confirmed
        public const int MinConfirmed = 5000;

        // filter regions for combined plot: confirmed
        public const int MinRecovered = 500;

        // filter regions for an own region-plot
        public const int MinConfirmedRegionalPlot = 500;

        // filter regions for combined plot: dead
        public const int MinDead = 50;

        public const int MaxSignalsPerCombinedPlot = 25;

        /// <summary>
        /// Main Programm entry point.
        /// </summary>
        /// <param name="args">Commandline Arguments.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task RunAsync()
        {
            Directory.CreateDirectory("www");

            var github = new Github();
            var plotter = new Plotter("plots");

            var dailyReportFileInfo = await github.GetFileInfoAsync(
                "CSSEGISandData",
                "COVID-19",
                "csse_covid_19_data/csse_covid_19_daily_reports");

            //var dailyReportCsvEngine = new FileHelperEngine<ReportData>();
            RecordTypeSelector recordSelector = (engine, line) 
                => line.Replace("\".*\"", "\"\"").Count(c => c == ',') >= 10
                    ? typeof(ReportData2)
                    : typeof(ReportData);

            var dailyReportCsvEngine = new MultiRecordEngine(
                recordSelector,
                typeof(ReportData), 
                typeof(ReportData2));
            var dailyReportData = new List<KeyValuePair<DateTime, List<ReportData>>>();

            static DateTime ParseDailyReportFileName(string fileName)
                => DateTime.ParseExact(
                    fileName.Substring(0, 10),
                    "MM-dd-yyyy",
                    null,
                    DateTimeStyles.None);

            var orderedFiles = dailyReportFileInfo
                .Where(f => Regex.Match(f.Name, @"\d{2}-\d{2}-\d{4}\.csv").Success)
                .Select(f => new { File = f, Date = ParseDailyReportFileName(f.Name) })
                .OrderBy(fd => fd.Date);

            List<PlotData> plotDataListGlobal = new List<PlotData>();

            var plotRegions = Regions.All;
            var plotDataRegional = plotRegions.ToDictionary(r => r, r => new List<PlotData>());

            DateTime lastDate = default;
            var allRegions = new List<string>();

            foreach (var fileInfo in orderedFiles)
            {
                System.Console.WriteLine($"Processing {fileInfo.Date:yyyy-MM-dd}");

                var fileData = await github.GetFileDataAsync(fileInfo.File);
                // var dailyReport = dailyReportCsvEngine.ReadStringAsList(fileData.Contents);
                var dailyReportEntries = dailyReportCsvEngine.ReadString(fileData.Contents);
                var dailyReport = dailyReportEntries
                    .Select(entry => entry switch 
                    {
                        ReportData r1 => r1, 
                        ReportData2 r2 => Convert(r2), 
                        _ => null 
                    })
                    .Where(n => n != null);

                // collect all regons to print them
                allRegions.AddRange(dailyReport.Select(report => report.Region));

                // revert aliased region names
                foreach (var reportData in dailyReport)
                {
                    if (Regions.Aliases.ContainsKey(reportData.Region))
                    {
                        reportData.Region = Regions.Aliases[reportData.Region];
                    }
                }

                // collect data for each (known) reagion
                foreach (var region in plotDataRegional.Keys)
                {
                    plotDataRegional[region]
                        .Add(CreatePlotData(
                            fileInfo.Date,
                            dailyReport.Where(r => r.Region.Trim() == region)));
                }

                // global data contains all regions, unfiltered
                plotDataListGlobal.Add(CreatePlotData(fileInfo.Date, dailyReport));

                lastDate = fileInfo.Date;
            }

            Regions.CompareWithFoundRegions(allRegions);

            static string GetTitle(string label)
                => $"COVID-19 Cases // {label}";

            static string GetDateTitle(string label, DateTime date)
                => $"{GetTitle(label)} // {date:yyyy-MM-dd}";

            static string GetCombinedTitle(string label, DateTime date, int minSignal, int maxSignals, string braceSuffix)
                => $"{GetTitle(label)} ({minSignal}+, {braceSuffix}) // {date:yyyy-MM-dd}";

            var createdRegionalPlotFileNames = new List<string>();
            var createdCustomPlotFileNames = new List<string>();

            plotter
                .CreatePlot(
                    plotDataListGlobal,
                    GetDateTitle("GLOBAL", lastDate),
                    "plot-global.png")
                .AddTo(createdCustomPlotFileNames);

            plotter
                .CreateDiffPlot(
                    plotDataListGlobal,
                    GetDateTitle("GLOBAL (diff)", lastDate),
                    "plot-global-diff.png")
                .AddTo(createdCustomPlotFileNames);

            string FilterRegionChars(string region)
                => Regex.Replace(region.ToLower(), @"\W", string.Empty);

            foreach (var region in plotDataRegional.Keys)
            {
                plotter
                    .CreatePlot(
                        plotDataRegional[region],
                        GetDateTitle(region, lastDate),
                        $"plot-{FilterRegionChars(region)}.png",
                        MinConfirmedRegionalPlot)
                    .AddTo(createdRegionalPlotFileNames);

                plotter
                    .CreateDiffPlot(
                        plotDataRegional[region],
                        GetDateTitle(region + " (diff)", lastDate),
                        $"plot-{FilterRegionChars(region)}-diff.png",
                        MinConfirmedRegionalPlot)
                    .AddTo(createdRegionalPlotFileNames);
            }

            // remove excluded regions
            var combinedViewRegionalData 
                = plotDataRegional.WhereRegionNotIn(Regions.CombinedPlotExclusions);

            plotter
                .CreateAveragePlot(
                    combinedViewRegionalData,
                    GetDateTitle($"GLOBAL AVERAGE (wo. China)", lastDate),
                    "plot-average.png")
                .AddTo(createdCustomPlotFileNames);

            plotter
                .CreateCombinedPlot(
                    combinedViewRegionalData,
                    pd => pd.Existing,
                    GetCombinedTitle(
                        $"EXISTING",
                        lastDate,
                        MinExisting,
                        MaxSignalsPerCombinedPlot,
                        "wo. China"),
                    "plot-existing.png",
                    MinExisting,
                    MaxSignalsPerCombinedPlot)
                .AddTo(createdCustomPlotFileNames);

            plotter
                .CreateCombinedPlot(
                    combinedViewRegionalData,
                    pd => pd.Confirmed,
                    GetCombinedTitle(
                        $"CONFIRMED",
                        lastDate,
                        MinConfirmed,
                        MaxSignalsPerCombinedPlot,
                        "wo. China"),
                    "plot-confirmed.png",
                    MinConfirmed,
                    MaxSignalsPerCombinedPlot)
                .AddTo(createdCustomPlotFileNames);

            plotter
                .CreateCombinedPlot(
                    combinedViewRegionalData,
                    pd => pd.Recovered,
                    GetCombinedTitle(
                        $"RECOVERED",
                        lastDate,
                        MinRecovered,
                        MaxSignalsPerCombinedPlot,
                        "wo. China"),
                    "plot-recovered.png",
                    MinRecovered,
                    MaxSignalsPerCombinedPlot)
                .AddTo(createdCustomPlotFileNames);

            plotter
                .CreateCombinedPlot(
                    combinedViewRegionalData,
                    pd => pd.Dead,
                    GetCombinedTitle($"DEAD", lastDate, MinDead, MaxSignalsPerCombinedPlot, "wo. China"),
                    "plot-dead.png",
                    MinDead,
                    MaxSignalsPerCombinedPlot)
                .AddTo(createdCustomPlotFileNames);

            plotter
                .CreateCombinedNormalizedPlot(
                    combinedViewRegionalData,
                    pd => pd.Dead,
                    GetTitle("DEAD NORMALIZED // start:50"),
                    "plot-dead-n.png",
                    50,
                    MinDead,
                    MaxSignalsPerCombinedPlot)
                .AddTo(createdCustomPlotFileNames);

            plotter
                .CreateCombinedNormalizedPlot(
                    combinedViewRegionalData,
                    pd => pd.Confirmed,
                    GetTitle("CONFIRMED NORMALIZED // start:50"),
                    "plot-confirmed-n.png",
                    50,
                    MinConfirmed,
                    MaxSignalsPerCombinedPlot)
                .AddTo(createdCustomPlotFileNames);

            plotter
                .CreateCombinedNormalizedPlot(
                    plotDataRegional.WhereRegionIn(Regions.CombinedPlotSet1),
                    pd => pd.Confirmed,
                    GetTitle("CONFIRMED NORMALIZED 1 // start:50"),
                    "plot-confirmed-n1.png",
                    50,
                    MinConfirmed,
                    MaxSignalsPerCombinedPlot)
                .AddTo(createdCustomPlotFileNames);
            
            plotter
                .CreateCombinedNormalizedPlot(
                    plotDataRegional.WhereRegionIn(Regions.CombinedPlotSet2),
                    pd => pd.Confirmed,
                    GetTitle("CONFIRMED NORMALIZED 2 // start:500"),
                    "plot-confirmed-n2.png",
                    500,
                    MinConfirmed,
                    MaxSignalsPerCombinedPlot)
                .AddTo(createdCustomPlotFileNames);

            // System.Console.WriteLine("Regional Plots:");
            var orderedRegionalPlots = createdRegionalPlotFileNames.OrderBy(_ => _);
            foreach (var creadedPlotFileName in orderedRegionalPlots)
            {
                System.Console.WriteLine(creadedPlotFileName);
            }

            // System.Console.WriteLine("Custom Plots:");
            var orderedCustomPlots = createdCustomPlotFileNames.OrderBy(_ => _);
            foreach (var creadedPlotFileName in orderedCustomPlots)
            {
                System.Console.WriteLine(creadedPlotFileName);
            }

            var markdownRenderer = new MarkdownPlotListRenderer("md");
            markdownRenderer.RenderPlotListMarkdown("All-Plots.md", orderedRegionalPlots.Concat(orderedCustomPlots));
            markdownRenderer.RenderPlotListMarkdown("Region-Plots.md", orderedRegionalPlots);
            markdownRenderer.RenderPlotListMarkdown("Custom-Plots.md", orderedCustomPlots);
        }

        private static PlotData CreatePlotData(DateTime date, IEnumerable<ReportData> dailyReport)
        {
            var deaths = dailyReport.Sum(i => i.Deaths);
            var recovered = dailyReport.Sum(i => i.Recovered);
            var confirmed = dailyReport.Sum(i => i.Confirmed);
            var existing = confirmed - recovered - deaths;

            return new PlotData
            {
                Date = date,
                Dead = deaths,
                Confirmed = confirmed,
                Recovered = recovered,
                Existing = existing,
            };
        }

        private static ReportData Convert(ReportData2 report) 
        {
            var result = new ReportData();
            result.Province = report.Province;
            result.Region = report.Region;

            result.Confirmed = report.Confirmed;
            result.Deaths = report.Deaths;
            result.Recovered = report.Recovered;

            return result;
        }
    }
}
