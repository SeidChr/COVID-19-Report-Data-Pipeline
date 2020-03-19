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

        private const string BahamasRegionName = "Bahamas";

        private const string PalestineRegionName = "Palestine";

        private const string TaipeiRegionName = "Taipei";

        private const string ItalyRegionName = "Italy";

        private const string GermanyRegionName = "Germany";

        private const string UsRegionName = "US";

        private const string SpainRegionName = "Spain";
        
        private const string GambiaRegionName = "Gambia";

        public static Dictionary<string, string> Aliases { get; }
            = new Dictionary<string, string>
            {
                ["Republic of Korea"] = KoreaRegionName,
                ["Korea, South"] = KoreaRegionName,

                ["Congo (Kinshasa)"] = CongoRegionName,
                ["Republic of the Congo"] = CongoRegionName,
                ["Congo (Brazzaville)"] = CongoRegionName,

                ["The Gambia"] = GambiaRegionName,
                ["Gambia, The"] = GambiaRegionName,

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
                ["Czech Republic"] = CzechiaRegionName,
                ["Taiwan*"] = TaiwanRegionName,
                ["United Kingdom"] = UkRegionName,
                ["Saint Vincent and the Grenadines"] = StVincentRegionName,
                ["The Bahamas"] = BahamasRegionName,
                ["occupied Palestinian territory"] = PalestineRegionName,
                ["Taipei and environs"] = TaipeiRegionName,
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
        /// 
        /// 20200317: "Benin", "Central African Republic", "Congo (Brazzaville)", 
        ///           "Equatorial Guinea", "Greenland", "Guam", "Kosovo", "Liberia", 
        ///           "Mayotte", "Puerto Rico", "Republic of the Congo", "Somalia", 
        ///           "Tanzania", "The Bahamas", "Uzbekistan"
        /// 
        /// 20200318: "Barbados", "Montenegro", "The Gambia"
        /// 
        /// 20200319: "Djibouti", "Gambia, The", "Kyrgyzstan", "Mauritius", "Zambia"
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
                "Bahrain",
                "Bangladesh",      
                "Barbados", 
                "Belarus",
                "Belgium",
                "Benin",
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
                "Central African Republic",
                "Channel Islands",
                "Chile",
                "Colombia",
                "Costa Rica",
                "Cote d'Ivoire",
                "Croatia",
                "Cuba",
                "Curacao",
                "Cyprus",
                "Denmark",
                "Djibouti", 
                "Dominican Republic",
                "Ecuador",
                "Egypt",
                "Equatorial Guinea",
                "Estonia",
                "Eswatini",
                "Ethiopia",
                "Faroe Islands",
                "Finland",
                "France",
                "French Guiana",
                "Gabon",
                "Georgia",
                "Ghana",
                "Gibraltar",
                "Greece",
                "Greenland",
                "Guadeloupe",
                "Guam",
                "Guatemala",
                "Guernsey",
                "Guinea",
                "Guyana",
                "Holy See",
                "Honduras",
                "Hungary",
                "Iceland",
                "India",
                "Indonesia",
                "Iraq",
                "Israel",
                "Ivory Coast",
                "Jamaica",
                "Japan",
                "Jersey",
                "Jordan",
                "Kazakhstan",
                "Kenya",
                "Kosovo",
                "Kuwait",
                "Kyrgyzstan", 
                "Latvia",
                "Lebanon",
                "Liberia",
                "Liechtenstein",
                "Lithuania",
                "Luxembourg",
                "Malaysia",
                "Maldives",
                "Malta",
                "Martinique",
                "Mauritania",
                "Mauritius", 
                "Mayotte",
                "Mexico",
                "Monaco",
                "Mongolia",
                "Montenegro",
                "Morocco",
                "Namibia",
                "Nepal",
                "Netherlands",
                "New Zealand",
                "Nigeria",
                "North Ireland",
                "North Macedonia",
                "Norway",
                "Oman",
                "Pakistan",
                "Panama",
                "Paraguay",
                "Peru",
                "Philippines",
                "Poland",
                "Portugal",
                "Puerto Rico",
                "Qatar",
                "Reunion",
                "Romania",
                "Rwanda",
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
                "Somalia",
                "South Africa",
                "Sri Lanka",
                "Sudan",
                "Suriname",
                "Sweden",
                "Switzerland",
                "Tanzania",
                "Thailand",
                "Togo",
                "Trinidad and Tobago",
                "Tunisia",
                "Turkey",
                "Ukraine",
                "United Arab Emirates",
                "Uruguay",
                "Uzbekistan",
                "Vatican City",
                "Venezuela",
                "Zambia",
                AzerbaijanRegionName,
                BahamasRegionName,
                ChinaRegionName,
                CongoRegionName,
                CzechiaRegionName,
                GambiaRegionName,
                GermanyRegionName,
                HongKongRegionName,
                IranRegionName,
                IrelandRegionName,
                ItalyRegionName,
                KoreaRegionName,
                MacauRegionName,
                MoldovaRegionName,
                OthersRegionName,
                PalestineRegionName,
                RussiaRegionName,
                SpainRegionName,
                StMartinRegionName,
                StVincentRegionName,
                TaipeiRegionName,
                TaiwanRegionName,
                UkRegionName,
                UsRegionName,
                VietNamRegionName,
            };

        public static List<string> CombinedPlotExclusions { get; }
            = new List<string>
            {
                ChinaRegionName,
                OthersRegionName,
            };
        
        public static List<string> CombinedPlotSet1 { get; }
            = new List<string>
            {
                ItalyRegionName,
                GermanyRegionName,
                ////ChinaRegionName,
                UsRegionName,
                ////KoreaRegionName,
                IranRegionName,
                SpainRegionName,
            };

        public static List<string> CombinedPlotSet2 { get; }
            = new List<string>
            {
                ItalyRegionName,
                ////GermanyRegionName,
                ChinaRegionName,
                ////UsRegionName,
                KoreaRegionName,
                IranRegionName,
                ////SpainRegionName,
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