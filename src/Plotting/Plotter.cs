namespace Plotting
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using Corona.Plotting.Model;
    using Corona.Shared;
    using ScottPlot;
    using ScottPlot.Config;

    public class Plotter
    {
        private readonly string directory;

        public Plotter(string directory)
        {
            Directory.CreateDirectory(directory);
            this.directory = directory;
        }
        
        public IEnumerable<string> CreatePlot(
            List<PlotData> plotDataset,
            string label,
            string file,
            double minConfirmed = 0.0,
            int startAtConfirmed = 0)
        {
            var maxConfirmed = plotDataset.Max(pd => pd.Confirmed);
            if (maxConfirmed < minConfirmed)
            {
                // avoid printing a graph for countries under threashold
                yield break;
            }

            var plt = GetDefaultPlot();

            if (startAtConfirmed > 0) 
            {
                plotDataset = plotDataset
                    .SkipWhile(pd => pd.Confirmed < startAtConfirmed)
                    .ToList();
            }

            var start = plotDataset.First().Date.ToOADate();

            void CreatePlotSignal(
                Func<PlotData, int> getValue,
                string label,
                Color color)
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

            this.FinalizePlot(plt, file);

            yield return file;
        }

        public IEnumerable<string> CreateDiffPlot(
            List<PlotData> plotDataset,
            string label,
            string file,
            double minConfirmed = 0.0)
        {
            var maxConfirmed = plotDataset.Max(pd => pd.Confirmed);
            if (maxConfirmed < minConfirmed)
            {
                // avoid printing a graph for countries under threashold
                yield break;
            }

            double[] CreateDiffPlotSignal(Func<PlotData, int> getValue)
                => plotDataset
                .Select(pd => (double)getValue(pd))
                .ToArray()
                .Diff();

            var diffExistingSignal = CreateDiffPlotSignal(pd => pd.Existing);
            var diffRecoveredSignal = CreateDiffPlotSignal(pd => pd.Recovered);
            var diffConfirmedSignal = CreateDiffPlotSignal(pd => pd.Confirmed);
            var diffDeadSignal = CreateDiffPlotSignal(pd => pd.Dead);

            var maxSignal = Enumerable.Empty<double>()
                .Concat(diffConfirmedSignal)
                .Concat(diffRecoveredSignal)
                .Concat(diffExistingSignal)
                .Concat(diffDeadSignal)
                .Max();

            var plt = GetDefaultPlot();
            var startDate = plotDataset.First().Date.ToOADate();

            //// void CreatePlotDiffSignal(
            ////     Func<PlotData, int> getValue,
            ////     string label,
            ////     Color color)
            ////     => plt.PlotSignal(
            ////         plotDataset.Select(pd => (double)getValue(pd)).ToArray().Diff(),
            ////         sampleRate: 1,
            ////         xOffset: start,
            ////         color: color,
            ////         label: label);

            void PlotDiffSignal(double[] signal, string label, Color color)
                => plt.PlotSignal(
                    signal,
                    sampleRate: 1,
                    xOffset: startDate,
                    color: color,
                    label: "DIFF " + label);

            PlotDiffSignal(diffExistingSignal, nameof(PlotData.Existing), Color.Orange);
            PlotDiffSignal(diffConfirmedSignal, nameof(PlotData.Confirmed), Color.Red);
            PlotDiffSignal(diffRecoveredSignal, nameof(PlotData.Recovered), Color.Green);
            PlotDiffSignal(diffDeadSignal, nameof(PlotData.Dead), Color.Black);

            plt.Axis(y2: maxSignal * 1.03);

            plt.Title(label);

            this.FinalizePlot(plt, file);

            yield return file;
        }

        public IEnumerable<string> CreateCombinedPlot(
            IEnumerable<KeyValuePair<string, List<PlotData>>> plotDataset,
            Func<PlotData, int> getSignal,
            string label,
            string file,
            double minSignal = 0.0,
            int maxSignals = 20)
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
                .Where(group => group.MaxSignal >= minSignal)
                .Take(maxSignals);

            var startDate = plotDataset
                .First()
                .Value
                .First()
                .Date
                .ToOADate();

            var plt = GetDefaultPlot();

            double overalMaxValue = 0;
            foreach (var group in orderedPlotDataSet)
            {
                overalMaxValue = group.MaxSignal > overalMaxValue
                    ? group.MaxSignal
                    : overalMaxValue;

                plt.PlotSignal(
                    group.Signal,
                    sampleRate: 1,
                    xOffset: startDate,
                    label: group.Label);
            }

            plt.Title(label);
            plt.Axis(y2: overalMaxValue * 1.03);

            this.FinalizePlot(plt, file);

            yield return file;
        }

        public IEnumerable<string> CreateCombinedNormalizedPlot(
            IEnumerable<KeyValuePair<string, List<PlotData>>> plotDataset,
            Func<PlotData, int> getSignal,
            string label,
            string file,
            double startSignal = 50,
            double minSignal = 0.0,
            int maxSignals = 20)
        {
            var orderedPlotDataSet = plotDataset
                .Select(pd =>
                {
                    var signal = pd.Value
                        .Select(pd => (double)getSignal(pd))
                        .SkipWhile(s => s < startSignal)
                        .ToArray();

                    return new
                    {
                        Label = pd.Key,
                        Signal = signal,
                        MaxSignal = signal.Any() ? signal.Max() : -1,
                    };
                })
                .OrderByDescending(pd => pd.MaxSignal)
                .Where(group => group.MaxSignal >= minSignal)
                .Take(maxSignals);

            var plt = GetDefaultPlot();

            plt.Ticks(dateTimeX: false);

            double overalMaxValue = 0;
            foreach (var group in orderedPlotDataSet)
            {
                overalMaxValue = group.MaxSignal > overalMaxValue
                    ? group.MaxSignal
                    : overalMaxValue;

                plt.PlotSignal(
                    group.Signal,
                    sampleRate: 1,
                    ////xOffset: startDate,
                    label: group.Label);
            }

            plt.Title(label);
            plt.Axis(y2: overalMaxValue * 1.03);

            this.FinalizePlot(plt, file);

            yield return file;
        }

        public IEnumerable<string> CreateAveragePlot(
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

            this.FinalizePlot(plt, file);

            yield return file;
        }
        
        private static Plot GetDefaultPlot()
        {
            var plt = new Plot(1000, 500);

            plt.Ticks(
                dateTimeX: true, // horizontal axis is a datetime axis
                useExponentialNotation: false, // do not sho exponents on large numbers
                useMultiplierNotation: false, // do not show a common muliplier on top
                useOffsetNotation: false);
                ////xTickRotation: 90

            plt.XLabel("github/SeidChr/COVID-19-Report-Data-Pipeline", fontSize: 12);
            ////plt.XLabel("Date");

            plt.Legend(fontSize: 10, location: legendLocation.upperLeft);
            plt.Style(figBg: ColorTranslator.FromHtml("#ededed"));

            plt.SetCulture(
                shortDatePattern: "MM-dd",
                decimalDigits: 2,
                decimalSeparator: ",",
                numberGroupSeparator: string.Empty);

            return plt;
        }

        private void FinalizePlot(Plot plt, string file)
        {
            plt.Layout(titleHeight: 40, xLabelHeight: 30);
            ////plt.AxisAuto(horizontalMargin: 0.5, verticalMargin: 0.5);
            ////yLabelWidth: 40,
            ////y2LabelWidth: 20,

            ////plt.TightenLayout(render: true);

            plt.Grid(xSpacingDateTimeUnit: DateTimeUnit.Day, xSpacing: 7);

            // System.Console.WriteLine(file);
            plt.SaveFig(this.directory + "/" + file);
        }
    }
}