namespace Corona
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Regions
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

        public const string CzechiaRegionName = "Czechia";

        private const string UkRegionName = "UK";

        private const string TaiwanRegionName = "Taiwan";

        private const string StVincentRegionName = "Saint Vincent";

        public static Dictionary<string, string> Aliases { get; }
            = new Dictionary<string, string>
            {
                ["Republic of Korea"] = KoreaRegionName,
                ["Korea, South"] = KoreaRegionName,

                ["Mainland China"] = ChinaRegionName,

                ["Cruise Ship"] = OthersRegionName,

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
                ["Czech Republic"] = CzechiaRegionName,
                ["Taiwan*"] = TaiwanRegionName,
                ["United Kingdom"] = UkRegionName,
                ["Saint Vincent and the Grenadines"] = StVincentRegionName,
            };

        /// <summary>
        /// 20200312: "Bolivia", "China", "Congo (Kinshasa)", "Cote d'Ivoire", 
        ///           "Cruise Ship", "Czechia", "Honduras", "Jamaica", "Korea, South", 
        ///           "Reunion", "Taiwan*", "Turkey", "United Kingdom"
        /// 
        /// 20200314: "Antigua and Barbuda", "Aruba", "Cayman Islands", "Cuba", 
        ///           "Ethiopia", "Guadeloupe", "Guinea", "Guyana", "Kazakhstan", 
        ///           "Kenya", "Sudan"
        /// 
        /// 20200315: "Curacao", "Eswatini", "Gabon", "Ghana", "Guatemala", 
        ///           "Guernsey", "Jersey", "Mauritania", "Namibia", "Rwanda", 
        ///           "Saint Lucia", "Saint Vincent and the Grenadines", "Seychelles", 
        ///           "Suriname", "Trinidad and Tobago", "Uruguay", "Venezuela"
        /// </summary>
        public static List<string> All { get; }
            = new List<string>
            {
                "Afghanistan",
                "Albania",
                "Algeria",
                "Andorra",
                "Antigua and Barbuda",
                "Argentina",
                "Armenia",
                "Aruba",
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
                "Cayman Islands",
                "Channel Islands",
                "Chile",
                ChinaRegionName,
                "Colombia",
                CongoRegionName,
                "Costa Rica",
                "Cote d'Ivoire",
                "Croatia",
                "Cuba",
                "Curacao",
                "Cyprus",
                CzechiaRegionName,

                "Denmark",
                "Dominican Republic",

                "Ecuador",
                "Egypt",
                "Estonia",
                "Eswatini",
                "Ethiopia",

                "Faroe Islands",
                "Finland",
                "France",
                "French Guiana",

                "Gabon",
                "Georgia",
                "Germany",
                "Ghana",
                "Gibraltar",
                "Greece",
                "Guadeloupe",
                "Guatemala",
                "Guernsey",
                "Guyana",
                "Guinea",

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
                "Jersey",
                "Jordan",

                KoreaRegionName,
                "Kazakhstan",
                "Kenya",
                "Kuwait",

                "Latvia",
                "Lebanon",
                "Liechtenstein",
                "Lithuania",
                "Luxembourg",

                MacauRegionName,
                "Malaysia",
                "Maldives",
                "Malta",
                "Martinique",
                "Mauritania",
                "Mexico",
                MoldovaRegionName,
                "Monaco",
                "Mongolia",
                "Morocco",

                "Namibia",
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

                "Reunion",
                "Romania",
                RussiaRegionName,
                "Rwanda",

                StVincentRegionName,
                StMartinRegionName,
                "Saint Barthelemy",
                "Saint Lucia",
                "San Marino",
                "Saudi Arabia",
                "Senegal",
                "Serbia",
                "Seychelles",
                "Singapore",
                "Slovakia",
                "Slovenia",
                "South Africa",
                "Spain",
                "Sri Lanka",
                "Sudan",
                "Suriname",
                "Sweden",
                "Switzerland",

                "Taipei and environs",
                TaiwanRegionName,
                "Thailand",
                "Togo",
                "Trinidad and Tobago",
                "Tunisia",
                "Turkey",

                UkRegionName,
                "Ukraine",
                "United Arab Emirates",
                "Uruguay",
                "US",

                "Vatican City",
                "Venezuela",
                VietNamRegionName,
            };

        public static List<string> CombinedPlotRegionExclusions { get; }
            = new List<string>
            {
                ChinaRegionName,
                OthersRegionName,
            };

        public static void CompareWithFoundRegions(List<string> foundRegions)
        {
            static void PrintRegionList(string label, IEnumerable<string> printRegions)
            {
                System.Console.WriteLine(label + ":");
                System.Console.WriteLine(string.Join(
                    ", ",
                    printRegions
                        .Distinct()
                        .OrderBy(r => r)
                        .Select(r => $"\"{r}\"")));
            }

            PrintRegionList("Found Regions", foundRegions);

            PrintRegionList(
                "Unknown Regions",
                foundRegions.Where(r
                    => !All.Contains(r)
                    && !Aliases.ContainsKey(r)));
        }
    }
}