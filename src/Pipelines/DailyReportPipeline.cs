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
    using FileHelpers;
    using global::Plotting;
    using ScottPlot;

    /// <summary>
    /// Main Programm class.
    /// </summary>
    public class DailyReportPipeline
    {
        // filter regions for combined plot: existing
        public const int MinExisting = 1000;

        // filter regions for combined plot: confirmed
        public const int MinConfirmed = 1000;

        // filter regions for combined plot: confirmed
        public const int MinRecovered = 500;

        // filter regions for an own region-plot
        public const int MinConfirmedRegionalPlot = 250;

        // filter regions for combined plot: dead
        public const int MinDead = 10;

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

            var dailyReportCsvEngine = new FileHelperEngine<ReportData>();
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
                var dailyReport = dailyReportCsvEngine.ReadStringAsList(fileData.Contents);

                // collect all regons to print them
                allRegions.AddRange(dailyReport.Select(report => report.Region));

                // revert aliased names
                foreach (var reportData in dailyReport)
                {
                    if (Regions.Aliases.ContainsKey(reportData.Region))
                    {
                        reportData.Region = Regions.Aliases[reportData.Region];
                    }
                }

                foreach (var region in plotDataRegional.Keys)
                {
                    plotDataRegional[region]
                        .Add(CreatePlotData(
                            fileInfo.Date,
                            dailyReport.Where(r => r.Region.Trim() == region)));
                }

                plotDataListGlobal.Add(CreatePlotData(fileInfo.Date, dailyReport));

                lastDate = fileInfo.Date;
            }

            Regions.CompareWithFoundRegions(allRegions);

            static string GetTitle(string label, DateTime date)
                => $"COVID-19 Cases // {label} // {date:yyyy-MM-dd}";

            static string GetCombinedTitle(string label, DateTime date, int minSignal, int maxSignals, string braceSuffix)
                => $"COVID-19 Cases // {label} ({minSignal}+, {braceSuffix}) // {date:yyyy-MM-dd}";

            var createdPlotFileNames = new List<string>();

            plotter
                .CreatePlot(
                    plotDataListGlobal,
                    GetTitle("GLOBAL", lastDate),
                    "plot-global.png")
                .AddTo(createdPlotFileNames);

            string FilterRegionChars(string region)
                => Regex.Replace(region.ToLower(), @"\W", string.Empty);

            foreach (var region in plotDataRegional.Keys)
            {
                plotter
                    .CreatePlot(
                        plotDataRegional[region],
                        GetTitle(region, lastDate),
                        $"plot-{FilterRegionChars(region)}.png",
                        MinConfirmedRegionalPlot)
                    .AddTo(createdPlotFileNames);
            }

            var combinedViewRegionalData = plotDataRegional
                .Where(pd => !Regions.CombinedPlotRegionExclusions.Contains(pd.Key));

            plotter
                .CreateAveragePlot(
                    combinedViewRegionalData,
                    GetTitle($"GLOBAL AVERAGE (wo. China)", lastDate),
                    "plot-average.png")
                .AddTo(createdPlotFileNames);

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
                .AddTo(createdPlotFileNames);

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
                .AddTo(createdPlotFileNames);

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
                .AddTo(createdPlotFileNames);

            plotter
                .CreateCombinedPlot(
                    combinedViewRegionalData,
                    pd => pd.Dead,
                    GetCombinedTitle($"DEAD", lastDate, MinDead, MaxSignalsPerCombinedPlot, "wo. China"),
                    "plot-dead.png",
                    MinDead,
                    MaxSignalsPerCombinedPlot)
                .AddTo(createdPlotFileNames);

            foreach (var creadedPlotFileName in createdPlotFileNames)
            {
                System.Console.WriteLine(creadedPlotFileName);
            }
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
    }
}
