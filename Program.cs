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
        private static CultureInfo plotCulture;

        /// <summary>
        /// Main Programm entry point.
        /// </summary>
        /// <param name="args">Commandline Arguments.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            plotCulture = new CultureInfo(string.Empty);
            var dateTimeFormat = new DateTimeFormatInfo
            {
                ShortDatePattern = "dd-MM-yyyy",
            };

            var numberFormat = new NumberFormatInfo
            {
                NumberDecimalDigits = 2,
                NumberDecimalSeparator = ",",
                NumberGroupSeparator = string.Empty,
            };

            plotCulture.DateTimeFormat = dateTimeFormat;
            plotCulture.NumberFormat = numberFormat;

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

            string[] plotRegions = { "Germany", "Italy", "US", "South Korea", "Iran", "France", "Netherlands" };
            var plotDataRegional = plotRegions.ToDictionary(r => r, r => new List<PlotData>());

            DateTime lastDate = default;
            foreach (var fileInfo in orderedFiles)
            {
                System.Console.WriteLine($"Processing {fileInfo.Date:dd-MM-yyyy}");

                var fileData = await github.GetFileDataAsync(fileInfo.File);
                var dailyReport = csvEngine.ReadStringAsList(fileData.Contents);

                foreach (var region in plotDataRegional.Keys)
                {
                    plotDataRegional[region]
                        .Add(CreatePlotData(fileInfo.Date, dailyReport.Where(r => r.Region == region)));
                }

                plotDataListGlobal.Add(CreatePlotData(fileInfo.Date, dailyReport));

                lastDate = fileInfo.Date;
            }

            CreatePlot(plotDataListGlobal, $"COVID-19 Cases // GLOBAL // {lastDate:dd-MM-yyy}", "plot.png");

            foreach (var region in plotDataRegional.Keys)
            {
                CreatePlot(
                    plotDataRegional[region],
                    $"COVID-19 Cases // {region} // {lastDate:dd-MM-yyy}",
                    $"plot-{region.ToLower().Replace(" ", string.Empty)}.png");
            }
        }

        private static PlotData CreatePlotData(DateTime date, IEnumerable<ReportData> dayList)
        {
            var deaths = dayList.Sum(i => i.Deaths);
            var recovered = dayList.Sum(i => i.Recovered);
            var confirmed = dayList.Sum(i => i.Confirmed);
            var existing = confirmed - recovered - deaths;

            ////System.Console.WriteLine($"{date:dd-MM-yyyy},{confirmed},{recovered},{deaths},{existing}");

            return new PlotData
            {
                Date = date,
                Deaths = deaths,
                Confirmed = confirmed,
                Recovered = recovered,
                Existing = existing,
            };
        }

        private static void CreatePlot(List<PlotData> plotDataList, string label, string file)
        {
            var start = plotDataList.First().Date.ToOADate();
            var plt = new ScottPlot.Plot(1000, 500);
            plt.PlotSignal(
                plotDataList.Select(pd => (double)pd.Existing).ToArray(),
                sampleRate: 1,
                xOffset: start,
                color: Color.Orange,
                label: "Existing");

            plt.PlotSignal(
                plotDataList.Select(pd => (double)pd.Confirmed).ToArray(),
                sampleRate: 1,
                xOffset: start,
                color: Color.Red,
                label: "Confirmed");

            plt.PlotSignal(
                plotDataList.Select(pd => (double)pd.Recovered).ToArray(),
                sampleRate: 1,
                xOffset: start,
                color: Color.Green,
                label: "Recovered");

            plt.PlotSignal(
                plotDataList.Select(pd => (double)pd.Deaths).ToArray(),
                sampleRate: 1,
                xOffset: start,
                color: Color.Black,
                label: "Dead");

            plt.Ticks(
                dateTimeX: true,
                useExponentialNotation: false,
                useMultiplierNotation: false,
                useOffsetNotation: false);

            plt.Title(label);
            plt.YLabel("People");
            plt.XLabel("Date");
            plt.Legend(fontSize: 10, location: legendLocation.upperLeft);
            plt.TightenLayout(render: true);
            plt.Layout(yLabelWidth: 60, y2LabelWidth: 60, xLabelHeight: 30, titleHeight: 30);
            plt.Axis(y2: plotDataList.Max(pd => pd.Confirmed) * 1.03);
            plt.Style(figBg: ColorTranslator.FromHtml("#ededed"));

            plt.SetCulture(plotCulture);

            Directory.CreateDirectory("plots");
            plt.SaveFig("plots/" + file);
        }
    }
}
