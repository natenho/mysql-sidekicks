using System.Drawing;

namespace ScintillaNET
{
    public static class ScintillaExtensions
    {
        public static void InitializeMySqlSyntaxHighlighting(this Scintilla editor)
        {
            editor.StyleResetDefault();
            editor.Styles[Style.Default].Font = "Consolas";
            editor.Styles[Style.Default].Size = 10;
            editor.StyleClearAll();

            editor.Lexer = Lexer.Sql;

            // Used to recognize full qualified identifiers like `schema`.`storedprocedure` as one word
            editor.WordChars += ".`";

            editor.Styles[Style.Sql.Comment].ForeColor = Color.Green;
            editor.Styles[Style.Sql.CommentLine].ForeColor = Color.Green;
            editor.Styles[Style.Sql.CommentLineDoc].ForeColor = Color.Green;
            editor.Styles[Style.Sql.Number].ForeColor = Color.Maroon;
            editor.Styles[Style.Sql.String].ForeColor = Color.Red;
            editor.Styles[Style.Sql.Character].ForeColor = Color.Red;
            editor.Styles[Style.Sql.Operator].ForeColor = Color.Black;

            // SELECT, INSERT, ...
            editor.Styles[Style.Sql.Word].ForeColor = Color.Blue;
            editor.Styles[Style.Sql.Word].Bold = true;

            // FUNCTIONS
            editor.Styles[Style.Sql.Word2].ForeColor = Color.Fuchsia;

            // INNER JOIN, NOT NULL, ...
            editor.Styles[Style.Sql.User1].ForeColor = Color.Gray;

            // ROUTINES
            editor.Styles[CustomStyle.Routine].Bold = true;
            editor.Styles[CustomStyle.Routine].ForeColor = Color.Maroon;

            editor.Indicators[CustomIndicator.Highlight].Style = IndicatorStyle.StraightBox;
            editor.Indicators[CustomIndicator.Highlight].Under = true;
            editor.Indicators[CustomIndicator.Highlight].ForeColor = Color.Lime;
            editor.Indicators[CustomIndicator.Highlight].OutlineAlpha = 255;
            editor.Indicators[CustomIndicator.Highlight].Alpha = 30;

            editor.SetKeywords(KeywordSet.Word, @"call start mediumint interval month day year prepare duplicate elseif ignore out integer leave add alter as authorization backup begin bigint binary bit break browse bulk by cascade case catch check checkpoint close clustered column commit compute constraint containstable continue create current cursor cursor database date datetime datetime2 datetimeoffset dbcc deallocate decimal declare default delete deny desc disk distinct distributed double drop dump else end errlvl escape except exec execute exit external fetch file fillfactor float for foreign freetext freetexttable from full function goto grant group having hierarchyid holdlock identity identity_insert identitycol if image index insert int intersect into key kill lineno load merge money national nchar nocheck nocount nolock nonclustered ntext numeric nvarchar of off offsets on open opendatasource openquery openrowset openxml option order over percent plan precision primary print proc procedure public raiserror read readtext real reconfigure references replication restore restrict return revert revoke rollback rowcount rowguidcol rule save schema securityaudit select set setuser shutdown smalldatetime smallint smallmoney sql_variant statistics table table tablesample text textsize then time timestamp tinyint to top tran transaction trigger truncate try union unique uniqueidentifier update updatetext use user values varbinary varchar varying view waitfor when where while with writetext xml go ");
            editor.SetKeywords(KeywordSet.Word2, @"last_insert_id ifnull left right sum trim least mid date_sub count now concat length replace ascii cast char charindex ceiling coalesce collate contains convert current_date current_time current_timestamp current_user floor isnull max min nullif object_id session_user substring system_user tsequal ");
            editor.SetKeywords(KeywordSet.User1, @"all and any between cross exists in inner is join like not null or outer pivot some unpivot ( ) * ");
        }
    }
}
