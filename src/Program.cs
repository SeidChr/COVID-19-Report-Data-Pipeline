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
    using Corona.PlotModel;
    using FileHelpers;
    using ScottPlot;

    /// <summary>
    /// Main Programm class.
    /// </summary>
    public class Program
    {
        // filter regions for combined plot: existing
        public const int MinExisting = 500;

        // filter regions for combined plot: confirmed
        public const int MinConfirmed = 500;

        // filter regions for combined plot: confirmed
        public const int MinRecovered = 500;

        // filter regions for an own region-plot
        public const int MinConfirmedRegionalPlot = 250;

        // filter regions for combined plot: dead
        public const int MinDead = 50;

        private static CultureInfo? plotCulture;

        /// <summary>
        /// Main Programm entry point.
        /// </summary>
        /// <param name="args">Commandline Arguments.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Main()
        {
            InitializePlotCluture();
            Directory.CreateDirectory("plots");

            var github = new Github();
            var asyncFiles = await github.GetFileInfoAsync(
                "CSSEGISandData",
                "COVID-19",
                "csse_covid_19_data/csse_covid_19_daily_reports");

            var csvEngine = new FileHelperEngine<ReportData>();
            var data = new List<KeyValuePair<DateTime, List<ReportData>>>();
            static DateTime ParseFileName(string fileName)
                => DateTime.ParseExact(
                    fileName.Substring(0, 10),
                    "MM-dd-yyyy",
                    null,
                    DateTimeStyles.None);

            var orderedFiles = asyncFiles
                .Where(f => Regex.Match(f.Name, @"\d{2}-\d{2}-\d{4}\.csv").Success)
                .Select(f => new { File = f, Date = ParseFileName(f.Name) })
                .OrderBy(fd => fd.Date);

            List<PlotData> plotDataListGlobal = new List<PlotData>();

            var plotRegions = RegionFilter.AllRegions;

            var plotDataRegional = plotRegions.ToDictionary(r => r, r => new List<PlotData>());

            DateTime lastDate = default;
            var allRegions = new List<string>();

            foreach (var fileInfo in orderedFiles)
            {
                System.Console.WriteLine($"Processing {fileInfo.Date:yyyy-MM-dd}");

                var fileData = await github.GetFileDataAsync(fileInfo.File);
                var dailyReport = csvEngine.ReadStringAsList(fileData.Contents);

                // collect all regons to print them
                allRegions.AddRange(dailyReport.Select(report => report.Region));

                // revert aliased names
                foreach (var reportData in dailyReport) 
                {
                    if (RegionFilter.Aliases.ContainsKey(reportData.Region)) 
                    {
                        reportData.Region = RegionFilter.Aliases[reportData.Region];
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

            static void PrintRegionList(string label, IEnumerable<string> printRegions) 
            {
                System.Console.WriteLine(label + ":");
                System.Console.WriteLine(string.Join(
                    ", ",
                    printRegions
                        .Distinct()
                        .OrderBy(r => r)
                        .Select(r => $"\"{r}\"")));
            }

            PrintRegionList("All Regions", allRegions);
            PrintRegionList(
                "Unknown Regions", 
                allRegions.Where(r 
                    => !RegionFilter.AllRegions.Contains(r)
                    && !RegionFilter.Aliases.ContainsKey(r)));

            static string GetTitle(string label, DateTime date) 
                => $"COVID-19 Cases // {label} // {date:yyyy-MM-dd}";

            CreatePlot(
                plotDataListGlobal,
                GetTitle("GLOBAL", lastDate),
                "plot.png");

            // CreateAveragePlot(
            //     plotDataRegional,
            //     GetTitle($"GLOBAL AVERAGE", lastDate),
            //     "plot-avg.png");

            foreach (var region in plotDataRegional.Keys)
            {
                CreatePlot(
                    plotDataRegional[region],
                    GetTitle(region, lastDate),
                    $"plot-{region.ToLower().Replace(" ", string.Empty)}.png",
                    MinConfirmedRegionalPlot);
            }

            var combinedViewRegionalData = plotDataRegional
                .Where(pd => !RegionFilter.CombinedPlotRegionExclusions.Contains(pd.Key));

            CreateAveragePlot(
                combinedViewRegionalData,
                GetTitle($"GLOBAL AVERAGE (wo. China)", lastDate),
                "plot-avg.png");

            CreateCombinedPlot(
                combinedViewRegionalData,
                pd => pd.Existing,
                GetTitle($"EXISTING ({MinExisting}+, wo. China)", lastDate),
                "plot-existing.png",
                MinExisting);

            CreateCombinedPlot(
                combinedViewRegionalData,
                pd => pd.Confirmed,
                GetTitle($"CONFIRMED ({MinConfirmed}+, wo. China)", lastDate),
                "plot-confirmed.png",
                MinConfirmed);

            CreateCombinedPlot(
                combinedViewRegionalData,
                pd => pd.Recovered,
                GetTitle($"RECOVERED ({MinRecovered}+, wo. China)", lastDate),
                "plot-recovered.png",
                MinConfirmed);

            CreateCombinedPlot(
                combinedViewRegionalData,
                pd => pd.Dead,
                GetTitle($"DEAD ({MinDead}+, wo. China)", lastDate),
                "plot-dead.png",
                MinDead);
        }

        private static void InitializePlotCluture()
        {
            plotCulture = new CultureInfo(string.Empty);
            var dateTimeFormat = new DateTimeFormatInfo
            {
                ShortDatePattern = "yyyy-MM-dd",
            };

            var numberFormat = new NumberFormatInfo
            {
                NumberDecimalDigits = 2,
                NumberDecimalSeparator = ",",
                NumberGroupSeparator = string.Empty,
            };

            plotCulture.DateTimeFormat = dateTimeFormat;
            plotCulture.NumberFormat = numberFormat;
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

        private static Plot GetDefaultPlot()
        {
            var plt = new Plot(1000, 500);
            plt.Ticks(
                dateTimeX: true, // horizontal axis is a datetime axis
                useExponentialNotation: false, // do not sho exponents on large numbers
                useMultiplierNotation: false, // do not show a common muliplier on top
                useOffsetNotation: false);

            ////plt.YLabel("People");
            ////plt.XLabel("Date");
            plt.Legend(fontSize: 10, location: legendLocation.upperLeft);
            plt.Style(figBg: ColorTranslator.FromHtml("#ededed"));

            plt.SetCulture(plotCulture);

            return plt;
        }

        private static void FinalizePlot(Plot plt, string file)
        {
            plt.Layout(titleHeight: 40, xLabelHeight: null);
            ////yLabelWidth: 40,
            ////y2LabelWidth: 20,
            
            ////plt.TightenLayout(render: true);

            System.Console.WriteLine(file);
            plt.SaveFig("plots/" + file);
        }

        private static void CreatePlot(List<PlotData> plotDataset, string label, string file, double minConfirmed = 0.0)
        {
            var maxConfirmed = plotDataset.Max(pd => pd.Confirmed);
            if (maxConfirmed < minConfirmed)
            {
                // avoid printing a graph for countries under threashold
                return;
            }

            var plt = GetDefaultPlot();
            var start = plotDataset.First().Date.ToOADate();

            void CreatePlotSignal(Func<PlotData, int> getValue, string label, Color color)
                => plt.PlotSignal(
                    plotDataset.Select(pd => (double)getValue(pd)).ToArray(),
                    sampleRate: 1,
                    xOffset: start,
                    color: color,
                    label: label);

            CreatePlotSignal(pd => pd.Existing, nameof(PlotData.Existing), Color.Orange);
            CreatePlotSignal(pd => pd.Confirmed, nameof(PlotData.Confirmed), Color.Red);
            CreatePlotSignal(pd => pd.Recovered, nameof(PlotData.Recovered), Color.Green);
            CreatePlotSignal(pd => pd.Dead, nameof(PlotData.Dead), Color.Black);

            plt.Axis(y2: maxConfirmed * 1.03);

            plt.Title(label);

            FinalizePlot(plt, file);
        }

        private static void CreateCombinedPlot(
            IEnumerable<KeyValuePair<string, List<PlotData>>> plotDataset,
            Func<PlotData, int> getSignal,
            string label,
            string file,
            double minSignal = 0.0)
        {
            var orderedPlotDataSet = plotDataset
                .Select(pd =>
                {
                    var signal = pd.Value.Select(pd => (double)getSignal(pd)).ToArray();
                    return new
                    {
                        Label = pd.Key,
                        Signal = signal,
                        MaxSignal = signal.Max(),
                    };
                })
                .OrderByDescending(pd => pd.MaxSignal)
                //// avoid cluttering the plot with irrelevant data
                .Where(group => group.MaxSignal >= minSignal);

            var startDate = plotDataset.First().Value.First().Date.ToOADate();
            var plt = GetDefaultPlot();

            double overalMaxValue = 0;
            foreach (var group in orderedPlotDataSet)
            {
                overalMaxValue = group.MaxSignal > overalMaxValue ? group.MaxSignal : overalMaxValue;
                plt.PlotSignal(group.Signal, sampleRate: 1, xOffset: startDate, label: group.Label);
            }

            plt.Title(label);
            plt.Axis(y2: overalMaxValue * 1.03);

            FinalizePlot(plt, file);
        }

        private static void CreateAveragePlot(
            IEnumerable<KeyValuePair<string, List<PlotData>>> plotDataset,
            string label,
            string file)
        {
            double[] CreateAveragePlotSignal(Func<PlotData, int> getValue)
                => plotDataset
                .Select(pd => pd.Value.Select(pd => (double)getValue(pd)))
                .Rows()
                .Select(r => r.ToList().Average())
                .ToArray();

            var avgExistingSignal = CreateAveragePlotSignal(pd => pd.Existing);
            var avgRecoveredSignal = CreateAveragePlotSignal(pd => pd.Recovered);
            var avgConfirmedSignal = CreateAveragePlotSignal(pd => pd.Confirmed);
            var avgDeadSignal = CreateAveragePlotSignal(pd => pd.Dead);

            var maxSignal = Enumerable.Empty<double>()
                .Concat(avgConfirmedSignal)
                .Concat(avgRecoveredSignal)
                .Concat(avgExistingSignal)
                .Concat(avgDeadSignal)
                .Max();

            var startDate = plotDataset.First().Value.First().Date.ToOADate();
            var plt = GetDefaultPlot();

            void PlotAvgSignal(double[] signal, string label, Color color)
                => plt.PlotSignal(
                    signal,
                    sampleRate: 1,
                    xOffset: startDate,
                    color: color,
                    label: "AVG " + label);

            PlotAvgSignal(avgExistingSignal, nameof(PlotData.Existing), Color.Orange);
            PlotAvgSignal(avgConfirmedSignal, nameof(PlotData.Confirmed), Color.Red);
            PlotAvgSignal(avgRecoveredSignal, nameof(PlotData.Recovered), Color.Green);
            PlotAvgSignal(avgDeadSignal, nameof(PlotData.Dead), Color.Black);

            plt.Title(label);
            plt.Axis(y2: maxSignal * 1.03);

            FinalizePlot(plt, file);
        }
    }
}
