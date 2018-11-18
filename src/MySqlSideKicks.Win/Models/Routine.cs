using System;

namespace MySqlSideKicks.Win
{
    public class Routine
    {
        public string Schema { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Definition { get; set; }
        public string FullName { get => $"{Schema}.{Name}"; }
        public string QuotedFullName { get => $"`{Schema}`.`{Name}`"; }

        public override string ToString()
        {
            return FullName;
        }
               
        public bool MatchesIdentifier(string identifier, string defaultSchema = "")
        {
            if(string.IsNullOrWhiteSpace(identifier))
            {
                return false;
            }

            var sanitizedIdentifier = SanitizeIdentifier(identifier);

            var matchesSameSchema = Schema.EqualsIgnoreCase(defaultSchema) && Name.EqualsIgnoreCase(sanitizedIdentifier);
            var matchesAnotherSchema = sanitizedIdentifier.EqualsIgnoreCase(ToString());

            return matchesSameSchema || matchesAnotherSchema;
        }

        private static string SanitizeIdentifier(string identifier)
        {
            return identifier?.Replace("`", string.Empty)
                .Trim();
        }
    }
}