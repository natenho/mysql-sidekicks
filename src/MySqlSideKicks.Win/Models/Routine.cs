using System;
using System.Text.RegularExpressions;

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
               
        public bool IdentifierMatches(string pattern, string defaultSchema = "")
        {
            if(string.IsNullOrWhiteSpace(pattern))
            {
                return false;
            }

            var sanitizedPattern = SanitizeIdentifier(pattern);
            
            var matchesSameSchema = Schema.EqualsIgnoreCase(defaultSchema) && Name.EqualsIgnoreCase(sanitizedPattern);
            var matchesAnotherSchema = sanitizedPattern.EqualsIgnoreCase(FullName);
            var matchesAsRegex = pattern.IsValidRegex() && Regex.IsMatch(FullName, pattern, RegexOptions.IgnoreCase);

            return matchesSameSchema || matchesAnotherSchema || matchesAsRegex;
        }

        private static string SanitizeIdentifier(string identifier)
        {
            return identifier?.Replace("`", string.Empty)
                .Trim();
        }

        internal bool DefinitionMatches(string pattern)
        {
            return Regex.IsMatch(Definition, pattern, RegexOptions.IgnoreCase);
        }
    }
}