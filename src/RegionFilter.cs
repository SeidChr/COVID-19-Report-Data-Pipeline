namespace Corona
{
    using System.Collections.Generic;

    public class RegionFilter
    {
        public const string OthersRegionName = "Others";

        public const string ChinaRegionName = "China";

        public const string IranRegionName = "Iran";

        public const string IrelandRegionName = "Ireland";

        public const string KoreaRegionName = "South Korea";

        public const string MoldovaRegionName = "Moldova";

        public const string HongKongRegionName = "Hong Kong";

        public const string AzerbaijanRegionName = "Azerbaijan";

        public const string VietNamRegionName = "Viet Nam";

        public const string MacauRegionName = "Macau";

        public const string RussiaRegionName = "Russia";

        public const string StMartinRegionName = "Saint Martin";

        public const string CongoRegionName = "Congo";

        public const string CzechiaRegionName = "Czech Republic";

        private const string UkRegionName = "UK";
        
        private const string TaiwanRegionName = "Taiwan";

        public static Dictionary<string, string> Aliases { get; } 
            = new Dictionary<string, string> 
            {
                ["Republic of Korea"] = KoreaRegionName,
                ["Korea, South"] = KoreaRegionName,

                ["Mainland China"] = ChinaRegionName,

                ["Cruise Ship"] = OthersRegionName,
                ["Reunion"] = OthersRegionName,

                ["Republic of Ireland"] = IrelandRegionName,
                ["Republic of Moldova"] = MoldovaRegionName,
                ["Iran (Islamic Republic of)"] = IranRegionName,
                ["Hong Kong SAR"] = HongKongRegionName, 
                [" Azerbaijan"] = AzerbaijanRegionName,
                ["Vietnam"] = VietNamRegionName,
                ["Macao SAR"] = MacauRegionName,
                ["Russian Federation"] = RussiaRegionName,
                ["St. Martin"] = StMartinRegionName,
                ["Congo (Kinshasa)"] = CongoRegionName,
                ["Czechia"] = CzechiaRegionName,
                ["Taiwan*"] = TaiwanRegionName,
                ["United Kingdom"] = UkRegionName,
            };
        
        /// 20200312: "Bolivia", "China", "Congo (Kinshasa)", "Cote d'Ivoire", "Cruise Ship", "Czechia", "Honduras", "Jamaica", "Korea, South", "Reunion", "Taiwan*", "Turkey", "United Kingdom"

        public static List<string> AllRegions { get; }
            = new List<string>
            {
                "Afghanistan",
                "Albania",
                "Algeria",
                "Andorra",
                "Argentina",
                "Armenia",
                "Australia",
                "Austria",
                AzerbaijanRegionName,
                "Bangladesh",
                "Bahrain",
                "Belarus",
                "Belgium",
                "Bhutan",
                "Bolivia",
                "Bosnia and Herzegovina",
                "Brazil",
                "Brunei",
                "Bulgaria",
                "Burkina Faso",
                "Cambodia",
                "Cameroon",
                "Canada",
                "Channel Islands",
                "Chile",
                "Colombia",
                CongoRegionName,
                "Costa Rica",
                "Cote d'Ivoire",
                "Croatia",
                "Cyprus",
                CzechiaRegionName,
                "Denmark",
                "Dominican Republic",
                "Ecuador",
                "Egypt",
                "Estonia",
                "Faroe Islands",
                "Finland",
                "France",
                "French Guiana",
                "Georgia",
                "Germany",
                "Gibraltar",
                "Greece",
                "Holy See",
                "Honduras",
                HongKongRegionName,
                "Hungary",
                "Iceland",
                "India",
                "Indonesia",
                IranRegionName,
                "Iraq",
                IrelandRegionName,
                "Israel",
                "Italy",
                "Ivory Coast",
                "Jamaica",
                "Japan",
                "Jordan",
                "Kuwait",
                "Latvia",
                "Lebanon",
                "Liechtenstein",
                "Lithuania",
                "Luxembourg",
                MacauRegionName,
                ChinaRegionName,
                "Malaysia",
                "Maldives",
                "Malta",
                "Martinique",
                "Mexico",
                MoldovaRegionName,
                "Monaco",
                "Mongolia",
                "Morocco",
                "Nepal",
                "Netherlands",
                "New Zealand",
                "Nigeria",
                "North Ireland",
                "North Macedonia",
                "Norway",
                "occupied Palestinian territory",
                "Oman",
                OthersRegionName,
                "Pakistan",
                "Palestine",
                "Panama",
                "Paraguay",
                "Peru",
                "Philippines",
                "Poland",
                "Portugal",
                "Qatar",
                "Romania",
                RussiaRegionName,
                "Saint Barthelemy",
                "Saint Martin",
                "San Marino",
                "Saudi Arabia",
                "Senegal",
                "Serbia",
                "Singapore",
                "Slovakia",
                "Slovenia",
                "South Africa",
                KoreaRegionName,
                "Spain",
                "Sri Lanka",
                "Sweden",
                "Switzerland",
                "Taipei and environs",
                TaiwanRegionName,
                "Thailand",
                "Togo",
                "Tunisia",
                "Turkey",
                UkRegionName,
                "Ukraine",
                "United Arab Emirates",
                "US",
                "Vatican City",
                VietNamRegionName,
            };

        public static List<string> CombinedPlotRegionExclusions { get; }
            = new List<string>
            {
                ChinaRegionName,
                OthersRegionName,
            };
    }
}