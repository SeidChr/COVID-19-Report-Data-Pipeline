namespace Corona.CsvModel
{
    using System;
    using FileHelpers;

    [DelimitedRecord(",")]
    [IgnoreFirst]
    public class DailyReport
    {
        [FieldOrder(20)]
        [FieldQuoted]
        public string Province;

        [FieldOrder(30)]
        [FieldQuoted]
        public string Region;

        [FieldOrder(40)]
        public string LastUpdate;

        [FieldOrder(50)]
        [FieldNullValue(0)]
        public int Confirmed;

        [FieldOrder(60)]
        [FieldNullValue(0)]
        public int Deaths;

        [FieldOrder(70)]
        [FieldNullValue(0)]
        public int Recovered;
    }
}