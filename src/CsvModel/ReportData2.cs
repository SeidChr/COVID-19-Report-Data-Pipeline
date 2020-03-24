#nullable enable
namespace Corona.CsvModel
{
    using FileHelpers;

    [DelimitedRecord(",")]
    [IgnoreFirst]
    public class ReportData2
    {
        [FieldOrder(10)]
        public string Fips { get; set; } = string.Empty;

        [FieldOrder(20)]
        public string Admin2 { get; set; } = string.Empty;

        [FieldOrder(30)]
        [FieldQuoted]
        public string Province { get; set; } = string.Empty;

        [FieldOrder(40)]
        [FieldQuoted]
        public string Region { get; set; } = string.Empty;

        [FieldOrder(50)]
        public string LastUpdate { get; set; } = string.Empty;

        [FieldOrder(60)]
        [FieldNullValue(0.0)]
        public double Lat { get; set; }

        [FieldOrder(70)]
        [FieldNullValue(0.0)]
        public double Lon { get; set; }

        [FieldOrder(80)]
        [FieldNullValue(0)]
        public int Confirmed { get; set; }

        [FieldOrder(90)]
        [FieldNullValue(0)]
        public int Deaths { get; set; }

        [FieldOrder(100)]
        [FieldNullValue(0)]
        public int Recovered { get; set; }

        [FieldOrder(110)]
        [FieldNullValue(0)]
        public int Active { get; set; }

        [FieldOrder(120)]
        [FieldQuoted]
        public string CombinedKey { get; set; } = string.Empty;
    }
}