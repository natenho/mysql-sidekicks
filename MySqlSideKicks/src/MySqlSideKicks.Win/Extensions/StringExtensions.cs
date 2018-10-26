namespace System
{
    public static class StringExtensions
    {
        public static bool EqualsIgnoreCase(this string text, string anotherText)
        {
            return text.Equals(anotherText, StringComparison.OrdinalIgnoreCase);
        }
    }
}