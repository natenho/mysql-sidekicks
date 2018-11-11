using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MySqlSideKicks.Win
{
    public partial class SessionForm : Form, ISessionView
    {
        public event Func<Task> Initialized;
        public event Action SearchPerformed;
        public event Func<Routine, Task> RoutineSelected;
        public event Func<string, Task> NavigationRequested;
        public event Func<Task> NavigatedBackward;
        public event Action<string> IdentifierActivated;

        public string Filter => searchBox.Text;
        public FilterMode FilterMode => filterByDefinition.Checked ? FilterMode.ByDefinition : FilterMode.ByName;

        public int CurrentPosition => editor.CurrentPosition;
        public string SelectedIdentifier => editor.GetWordFromPosition(CurrentPosition);
        public bool CanNavigateBackward { get => navigateBackwardButton.Enabled; set => navigateBackwardButton.Enabled = value; }

        const int ScintilaNotFound = -1;

        private string _routineKeywords;

        private static class CustomIndicator
        {
            // Indicators 0-7 could be in use by a lexer so we'll use indicator 8+ for other specific usages
            public const int Highlight = 8;
        }

        private static class CustomStyle
        {
            public const int Routine = Style.Sql.User2;
        }

        private static class KeywordSet
        {
            public const int Word = 0;
            public const int Word2 = 1;
            public const int User1 = 4;            
        }

        public SessionForm()
        {
            InitializeComponent();
            InitializeSyntaxHighlighting();
        }

        private void InitializeSyntaxHighlighting()
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

        public void LoadRoutineList(IList<Routine> routines)
        {
            Cursor = Cursors.WaitCursor;
            objectExplorerListBox.SuspendLayout();

            objectExplorerListBox.DataSource = routines;

            var routineKeywordsBare = string.Join(" ", routines.Select(r => r.Name.ToLower()));
            var routineKeywordsWithSchema = string.Join(" ", routines.Select(r => $"{r.Schema.ToLower()}.{r.Name.ToLower()}"));
            var routineKeywordsWithSchemaFullyQualified = string.Join(" ", routines.Select(r => $"`{r.Schema.ToLower()}`.`{r.Name.ToLower()}`"));
            _routineKeywords = string.Join(" ", routineKeywordsBare, routineKeywordsWithSchema, routineKeywordsWithSchemaFullyQualified);

            editor.SetKeywords(5, _routineKeywords);

            objectExplorerListBox.ResumeLayout();
            Cursor = Cursors.Default;
        }

        public void OpenRoutine(Routine routine)
        {
            Cursor = Cursors.WaitCursor;
            editor.SuspendLayout();

            var code = FixLexerCompatibility(routine.Definition);

            editor.Text = code;

            editor.ResumeLayout();
            Cursor = Cursors.Default;
        }

        public void GoToPosition(int position)
        {
            editor.SelectionStart = position;
            editor.SelectionEnd = position;
        }

        private static string FixLexerCompatibility(string code)
        {
            // Scintilla SQL lexer doesn't recognize # as comment, so, replace by --
            return Regex.Replace(code, @"(?<=[\s\t]+)#", "--");
        }

        private void SearchFired(object sender, EventArgs e)
        {
            SearchPerformed();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            await Initialized();

            editor.ClearCmdKey(Keys.Control | Keys.Add);
        }

        private async void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.OemMinus)
            {
                await NavigatedBackward();
            }
        }

        private async void objectExplorerListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            await RoutineSelected(objectExplorerListBox.SelectedItem as Routine);
        }

        private void objectExplorerListBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsLetterOrDigit(e.KeyChar))
            {
                searchBox.Focus();
                searchBox.AppendText(e.KeyChar.ToString());
            }
        }

        private void editor_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Avoid adding control caracters
            if (e.KeyChar < 32)
            {
                e.Handled = true;
            }
        }

        private void editor_KeyDown(object sender, KeyEventArgs e)
        {
            editor.Styles[CustomStyle.Routine].Hotspot = e.Control;
        }

        private void editor_MouseMove(object sender, MouseEventArgs e)
        {
            var isControlKeyDown = (ModifierKeys & Keys.Control) != 0;

            editor.Styles[CustomStyle.Routine].Hotspot = isControlKeyDown;
        }

        private async void editor_HotspotClick(object sender, HotspotClickEventArgs e)
        {
            var isControlKeyDown = (e.Modifiers & Keys.Control) != 0;

            var identifier = editor.GetWordFromPosition(e.Position);

            await NavigationRequested(identifier);
        }

        private void editor_UpdateUI(object sender, UpdateUIEventArgs e)
        {
            if (e.Change == UpdateChange.Selection)
            {
                var identifier = editor.SelectedText;

                if (identifier == string.Empty)
                {
                    identifier = editor.GetWordFromPosition(editor.CurrentPosition);
                }

                IdentifierActivated(identifier);
            }
        }

        private async void navigateBackwardButton_Click(object sender, EventArgs e)
        {
            await NavigatedBackward();
        }

        public void HighlightText(string text)
        {
            editor.IndicatorCurrent = CustomIndicator.Highlight;
            editor.IndicatorClearRange(0, editor.TextLength);

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            editor.TargetStart = 0;
            editor.TargetEnd = editor.TextLength;
            editor.SearchFlags = SearchFlags.WholeWord;

            int occurencesCount = 0;

            while (editor.SearchInTarget(text) != ScintilaNotFound)
            {
                editor.IndicatorFillRange(editor.TargetStart, editor.TargetEnd - editor.TargetStart);

                editor.TargetStart = editor.TargetEnd;
                editor.TargetEnd = editor.TextLength;

                occurencesCount++;
            }

            if (occurencesCount <= 1)
            {
                editor.IndicatorClearRange(0, editor.TextLength);
            }
        }
    }
}
