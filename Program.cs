namespace Corona
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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
        /// <summary>
        /// Main Programm entry point.
        /// </summary>
        /// <param name="args">Commandline Arguments.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            var github = new Github();
            var asyncFiles = await github.GetFileInfoAsync(
                "CSSEGISandData",
                "COVID-19",
                "csse_covid_19_data/csse_covid_19_daily_reports");

            var csvEngine = new FileHelperEngine<DailyReport>();
            var data = new List<KeyValuePair<DateTime, List<DailyReport>>>();
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

            List<PlotData> plotData = new List<PlotData>();
            foreach (var fileInfo in orderedFiles)
            {
                // System.Console.WriteLine("F:" + fileInfo.File.Name);
                var fileData = await github.GetFileDataAsync(fileInfo.File);
                var dayList = csvEngine.ReadStringAsList(fileData.Contents);

                var deaths = dayList.Sum(i => i.Deaths);
                var recovered = dayList.Sum(i => i.Recovered);
                var confirmed = dayList.Sum(i => i.Confirmed);
                var existing = confirmed - recovered - deaths;

                plotData.Add(new PlotData
                {
                    Date = fileInfo.Date,
                    Deaths = deaths,
                    Confirmed = confirmed,
                    Recovered = recovered,
                    Existing = existing,
                });

                System.Console.WriteLine($"{fileInfo.Date:dd-MM-yyyy},{confirmed},{recovered},{deaths},{existing}");
            }

            DateTime start = plotData.First().Date;
            var plt = new ScottPlot.Plot(1000, 500);
            plt.PlotSignal(
                plotData.Select(pd => (double)pd.Existing).ToArray(),
                sampleRate: 1,
                xOffset: start.ToOADate(),
                label: "Existing");

            plt.PlotSignal(
                plotData.Select(pd => (double)pd.Confirmed).ToArray(),
                sampleRate: 1,
                xOffset: start.ToOADate(),
                label: "Confirmed");

            plt.PlotSignal(
                plotData.Select(pd => (double)pd.Recovered).ToArray(),
                sampleRate: 1,
                xOffset: start.ToOADate(),
                label: "Recovered");

            plt.PlotSignal(
                plotData.Select(pd => (double)pd.Deaths).ToArray(),
                sampleRate: 1,
                xOffset: start.ToOADate(),
                label: "Dead");

            plt.Ticks(
                dateTimeX: true,
                useExponentialNotation: false,
                useMultiplierNotation: false,
                useOffsetNotation: false);

            plt.Title("Corona Virus Cases");
            plt.YLabel("Cases");
            plt.XLabel("Date");
            plt.Legend(fontSize: 10, location: legendLocation.upperLeft);

            plt.SaveFig("plot.png");
        }
    }
}
