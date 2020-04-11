namespace Corona.Plotting
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
        private const string Subtitle = "github/SeidChr/COVID-19-Report-Data-Pipeline";

        private const double PolygonFillAlpha = .6;

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

            var maxValues = new int[2];

            this.CreateSignalBasedPlotArea(
                plt,
                plotDataset, 
                pd => pd.Confirmed, 
                pd => pd.Confirmed - pd.Recovered, 
                start, 
                nameof(PlotData.Recovered), 
                Color.Green);

            this.CreateSignalBasedPlotArea(
                plt, 
                plotDataset, 
                pd => pd.Confirmed - pd.Recovered, 
                pd => pd.Confirmed - pd.Recovered - pd.Dead, 
                start, 
                nameof(PlotData.Dead), 
                Color.Black);

            this.CreateZeroBasedPlotArea(
                plt, 
                plotDataset, 
                pd => pd.Confirmed - pd.Recovered - pd.Dead, 
                start, 
                nameof(PlotData.Existing), 
                Color.Red);

            maxValues[0] = this.CreatePlotSignal(
                plt, 
                plotDataset, 
                pd => pd.Confirmed, 
                start: start, 
                label: nameof(PlotData.Confirmed), 
                color: Color.BlueViolet, 
                markerSize: 0);

            var lastDataSet = plotDataset.Last();
            var deathRate = ((double)lastDataSet.Dead) / lastDataSet.Confirmed;
            plt.PlotAnnotation($"Deaths / Confirmed: {deathRate:0.000}", 5, 85, fillColor: Color.Transparent, shadow: true);

            plt.Axis(y2: maxValues.Max() * 1.03);

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

            void PlotDiffSignal(double[] signal, string label, Color color)
                => plt.PlotSignal(
                    signal,
                    sampleRate: 1,
                    xOffset: startDate,
                    color: color,
                    label: "DIFF " + label + $" ({signal.Last()})");

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
                    label: group.Label + $" ({group.Signal.Last()})");
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
                    label: group.Label + $" ({group.Signal.Last()})");
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
                    label: "AVG " + label + $" ({signal.Last()})");

            PlotAvgSignal(avgExistingSignal, nameof(PlotData.Existing), Color.Orange);
            PlotAvgSignal(avgConfirmedSignal, nameof(PlotData.Confirmed), Color.Red);
            PlotAvgSignal(avgRecoveredSignal, nameof(PlotData.Recovered), Color.Green);
            PlotAvgSignal(avgDeadSignal, nameof(PlotData.Dead), Color.Black);

            plt.Title(label);
            plt.Axis(y2: maxSignal * 1.03);

            this.FinalizePlot(plt, file);

            yield return file;
        }

        private static IEnumerable<double> Increment(double start, double increment, int times)
        {
            var current = start;
            for (var i = 0; i < times; i++)
            {
                yield return current;
                current += increment;
            }
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

            plt.XLabel(Subtitle, fontSize: 12);
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

        private int CreatePlotSignal(
            Plot plt,
            IEnumerable<PlotData> data,
            Func<PlotData, int> getValue,
            double start,
            string label,
            Color color,
            int markerSize = 5)
        {
            var signal = data
                .Select(pd => getValue(pd));

            var ys = signal.Select(s => (double)s).ToArray();

            var xs = Increment(start - .05, 1d, ys.Length).ToArray();

            plt.PlotScatter(
                xs,
                ys,
                color: color,
                label: label + $" ({signal.Last()})",
                markerSize: markerSize);

            return signal.Max();
        }

        private int CreateZeroBasedPlotArea(
            Plot plt,
            IEnumerable<PlotData> data,
            Func<PlotData, int> getUpperValue,
            double start,
            string label,
            Color color)
        {
            var signal = data
                .Select(pd => getUpperValue(pd));

            var sig = signal.Select(s => (double)s).ToArray();

            var arrayLength = sig.Length + 2;
            var xs = new double[arrayLength];
            var ys = new double[arrayLength];

            Array.Copy(sig, 0, ys, 1, sig.Length);

            xs[0] = xs[1] = start;
            ys[0] = 0;

            for (var i = 2; i < arrayLength; i++)
            {
                xs[i] = xs[i - 1] + 1d;
            }

            xs[arrayLength - 1] = xs[arrayLength - 2];

            plt.PlotPolygon(
                xs,
                ys,
                fillColor: color,
                fillAlpha: PolygonFillAlpha,
                label: label + $" ({signal.Last()})");

            return signal.Max();
        }

        private int CreateSignalBasedPlotArea(
            Plot plt,
            IEnumerable<PlotData> data,
            Func<PlotData, int> getUpperValue,
            Func<PlotData, int> getLowerValue,
            double start,
            string label,
            Color color)
        {
            var sig = data
                .Select(pd =>
                {
                    var upper = getUpperValue(pd);
                    var lower = getLowerValue(pd);
                    return (upper, doubleUpper: (double)upper, lower, doubleLower: (double)lower);
                }).ToArray();

            var arrayLength = sig.Length;

            var xs = new double[arrayLength * 2];
            var ys = new double[arrayLength * 2];

            var fd = start;
            var rd = start + (arrayLength - 1);

            for (int i = 0, y = arrayLength, r = arrayLength - 1; i < arrayLength; i++, y++, r--)
            {
                xs[i] = fd;
                xs[y] = rd;

                fd += 1d;
                rd -= 1d;

                ys[i] = sig[i].doubleUpper;
                ys[y] = sig[r].doubleLower;
            }

            plt.PlotPolygon(
                xs,
                ys,
                fillColor: color,
                fillAlpha: PolygonFillAlpha,
                label: label + $" ({sig.Last().upper - sig.Last().lower})");

            ////plt.PlotFill(
            ////    xs,
            ////    ys,
            ////    fillColor: color,
            ////    fillAlpha: PolygonFillAlpha,
            ////    label: label + $" ({sig.Last().upper - sig.Last().lower})");

            return sig.Max(s => s.upper);
        }
    }
}