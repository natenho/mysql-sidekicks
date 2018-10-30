using System;

namespace MySqlSideKicks.Win
{
    class Routine
    {
        public string Schema { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Definition { get; set; }

        public override string ToString()
        {
            return $"{Schema}.{Name}";
        }

        public bool MatchesIdentifier(string identifier, string defaultSchema = "")
        {
            var sanitizedIdentifier = SanitizeIdentifier(identifier);
            var matchesSameSchema = Schema.EqualsIgnoreCase(defaultSchema) && Name.EqualsIgnoreCase(sanitizedIdentifier);
            var matchesAnotherSchema = sanitizedIdentifier.EqualsIgnoreCase(ToString());
            return matchesSameSchema || matchesAnotherSchema;
        }

        private static string SanitizeIdentifier(string identifier)
        {
            return identifier.Replace("`", string.Empty)
                .Trim();
        }
    }
}