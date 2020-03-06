#nullable enable
namespace Corona.CsvModel
{
    using FileHelpers;

    [DelimitedRecord(",")]
    [IgnoreFirst]
    public class ReportData
    {
        [FieldOrder(20)]
        [FieldQuoted]
        public string Province { get; set; } = string.Empty;

        [FieldOrder(30)]
        [FieldQuoted]
        public string Region { get; set; } = string.Empty;

        [FieldOrder(40)]
        public string LastUpdate { get; set; } = string.Empty;

        [FieldOrder(50)]
        [FieldNullValue(0)]
        public int Confirmed { get; set; }

        [FieldOrder(60)]
        [FieldNullValue(0)]
        public int Deaths { get; set; }

        [FieldOrder(70)]
        [FieldNullValue(0)]
        public int Recovered { get; set; }

        [FieldOrder(80)]
        [FieldNullValue(0.0)]
        [FieldOptional]
        public double Lat { get; set; }

        [FieldOrder(90)]
        [FieldNullValue(0.0)]
        [FieldOptional]
        public double Lon { get; set; }
    }
}