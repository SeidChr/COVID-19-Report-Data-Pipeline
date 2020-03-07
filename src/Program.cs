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

        // filter regions for an own region-plot
        public const int MinConfirmed = 250;

        // filter regions for combined plot: dead
        public const int MinDead = 10;

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

            var plotRegions = Const.Regions;
            ////new string[] {
            ////    "US", "Japan", "South Korea", "France", "Germany", "Italy", "UK",
            ////    "Sweden", "Spain", "Belgium", "Iran", "Switzerland", "Norway", "Netherlands",
            ////};

            var plotDataRegional = plotRegions.ToDictionary(r => r, r => new List<PlotData>());

            DateTime lastDate = default;
            var regions = new List<string>();

            foreach (var fileInfo in orderedFiles)
            {
                System.Console.WriteLine($"Processing {fileInfo.Date:yyyy-MM-dd}");

                var fileData = await github.GetFileDataAsync(fileInfo.File);
                var dailyReport = csvEngine.ReadStringAsList(fileData.Contents);

                regions.AddRange(dailyReport.Select(report => report.Region));

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

            System.Console.WriteLine("All Regions:");
            System.Console.WriteLine(
                string.Join(
                    ", ",
                    regions.Distinct().OrderBy(r => r).Select(region => $"\"{region}\"")));

            CreatePlot(
                plotDataListGlobal,
                $"COVID-19 Cases // GLOBAL // {lastDate:dd-MM-yyy}",
                "plot.png");

            foreach (var region in plotDataRegional.Keys)
            {
                CreatePlot(
                    plotDataRegional[region],
                    $"COVID-19 Cases // {region} // {lastDate:dd-MM-yyy}",
                    $"plot-{region.ToLower().Replace(" ", string.Empty)}.png",
                    MinConfirmed);
            }

            CreateCombinedPlot(
                plotDataRegional,
                pd => pd.Existing,
                $"COVID-19 Cases // EXISTING ({MinExisting}+) // {lastDate:dd-MM-yyy}",
                "plot-existing.png",
                MinExisting);

            CreateCombinedPlot(
                plotDataRegional,
                pd => pd.Deaths,
                $"COVID-19 Cases // DEAD ({MinDead}+) // {lastDate:dd-MM-yyy}",
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
                Deaths = deaths,
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

            plt.YLabel("People");
            plt.XLabel("Date");
            plt.Legend(fontSize: 10, location: legendLocation.upperLeft);
            plt.TightenLayout(render: true);
            plt.Layout(yLabelWidth: 60, y2LabelWidth: 60, xLabelHeight: 30, titleHeight: 40);
            plt.Style(figBg: ColorTranslator.FromHtml("#ededed"));

            plt.SetCulture(plotCulture);

            return plt;
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

            CreatePlotSignal(pd => pd.Existing, "Existing", Color.Orange);
            CreatePlotSignal(pd => pd.Confirmed, "Confirmed", Color.Red);
            CreatePlotSignal(pd => pd.Recovered, "Recovered", Color.Green);
            CreatePlotSignal(pd => pd.Deaths, "Dead", Color.Black);

            plt.Axis(y2: maxConfirmed * 1.03);

            plt.Title(label);

            System.Console.WriteLine(file);
            plt.SaveFig("plots/" + file);
        }

        private static void CreateCombinedPlot(
            Dictionary<string, List<PlotData>> plotDataset,
            Func<PlotData, int> getSignal,
            string label,
            string file,
            double minSignal = 0.0)
        {
            var start = plotDataset.First().Value.First().Date.ToOADate();
            var plt = GetDefaultPlot();

            double overalMaxValue = 0;
            foreach (var group in plotDataset)
            {
                var signal = group.Value.Select(pd => (double)getSignal(pd)).ToArray();
                var signalMaxValue = signal.Max();
                if (signalMaxValue < minSignal)
                {
                    // avoid cluttering the plot with irrelevant data
                    continue;
                }

                overalMaxValue = signalMaxValue > overalMaxValue ? signalMaxValue : overalMaxValue;
                plt.PlotSignal(signal, sampleRate: 1, xOffset: start, label: group.Key);
            }

            plt.Title(label);
            Directory.CreateDirectory("plots");
            plt.Axis(y2: overalMaxValue * 1.03);

            System.Console.WriteLine(file);
            plt.SaveFig("plots/" + file);
        }
    }
}
