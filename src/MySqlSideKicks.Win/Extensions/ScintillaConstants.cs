namespace ScintillaNET
{
    public static class CustomIndicator
    {
        // Indicators 0-7 could be in use by a lexer so we'll use indicator 8+ for other specific usages
        public const int Highlight = 8;
    }

    public static class CustomStyle
    {
        public const int Routine = Style.Sql.User2;
    }

    public static class KeywordSet
    {
        public const int Word = 0;
        public const int Word2 = 1;
        public const int User1 = 4;
        public const int User2 = 5;
    }
}