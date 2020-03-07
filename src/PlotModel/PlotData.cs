namespace Corona.PlotModel
{
    using System;

    public struct PlotData
    {
        public DateTime Date { get; set; }

        public int Dead { get; set; }

        public int Recovered { get; set; }

        public int Confirmed { get; set; }

        public int Existing { get; set; }
    }
}