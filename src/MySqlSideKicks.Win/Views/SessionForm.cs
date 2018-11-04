using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MySqlSideKicks.Win
{
    public partial class SessionForm : Form, ISessionView
    {
        public event Func<Task> Initialize;
        public event Action Search;
        public event Func<Routine, Task> RoutineSelected;
        public event Func<Task> DefinitionRequested;
        public event Func<Task> NavigateBackward;
        public event Action<string> TextSelected;

        public string Filter => searchBox.Text;
        public FilterMode FilterMode => filterByDefinition.Checked ? FilterMode.ByDefinition : FilterMode.ByName;

        public int CurrentPosition => codeScintilla.CurrentPosition;
        public string SelectedIdentifier => codeScintilla.GetWordFromPosition(CurrentPosition);
        public bool CanNavigateBackward { get => navigateBackwardButton.Enabled; set => navigateBackwardButton.Enabled = value; }

        const int HighlightIndicator = 8; // Indicators 0-7 could be in use by a lexer so we'll use indicator 8 to highlight words.
        const int ScintilaNotFound = -1;

        public SessionForm()
        {
            InitializeComponent();
            InitializeSyntaxHighlighting();
        }

        private void InitializeSyntaxHighlighting()
        {
            codeScintilla.StyleResetDefault();
            codeScintilla.Styles[Style.Default].Font = "Consolas";
            codeScintilla.Styles[Style.Default].Size = 10;
            codeScintilla.StyleClearAll();

            codeScintilla.Lexer = Lexer.Sql;
            // Used to recognize full qualified identifiers like `schema`.`storedprocedure` as one word
            codeScintilla.WordChars += ".`";

            codeScintilla.Styles[Style.Sql.Comment].ForeColor = Color.Green;
            codeScintilla.Styles[Style.Sql.CommentLine].ForeColor = Color.Green;
            codeScintilla.Styles[Style.Sql.CommentLineDoc].ForeColor = Color.Green;
            codeScintilla.Styles[Style.Sql.Number].ForeColor = Color.Maroon;
            codeScintilla.Styles[Style.Sql.Word].ForeColor = Color.Blue;
            codeScintilla.Styles[Style.Sql.Word].Bold = true;
            codeScintilla.Styles[Style.Sql.Word2].ForeColor = Color.Fuchsia;
            codeScintilla.Styles[Style.Sql.User1].ForeColor = Color.Gray;
            codeScintilla.Styles[Style.Sql.User2].BackColor = Color.Yellow;
            codeScintilla.Styles[Style.Sql.User2].Bold = true;
            codeScintilla.Styles[Style.Sql.String].ForeColor = Color.Red;
            codeScintilla.Styles[Style.Sql.Character].ForeColor = Color.Red;
            codeScintilla.Styles[Style.Sql.Operator].ForeColor = Color.Black;
                        
            codeScintilla.Indicators[HighlightIndicator].Style = IndicatorStyle.StraightBox;
            codeScintilla.Indicators[HighlightIndicator].Under = true;
            codeScintilla.Indicators[HighlightIndicator].ForeColor = Color.LightGreen;
            codeScintilla.Indicators[HighlightIndicator].OutlineAlpha = int.MaxValue;
            codeScintilla.Indicators[HighlightIndicator].Alpha = int.MaxValue;

            // Word = 0
            codeScintilla.SetKeywords(0, @"interval month day year prepare duplicate elseif ignore out integer leave add alter as authorization backup begin bigint binary bit break browse bulk by cascade case catch check checkpoint close clustered column commit compute constraint containstable continue create current cursor cursor database date datetime datetime2 datetimeoffset dbcc deallocate decimal declare default delete deny desc disk distinct distributed double drop dump else end errlvl escape except exec execute exit external fetch file fillfactor float for foreign freetext freetexttable from full function goto grant group having hierarchyid holdlock identity identity_insert identitycol if image index insert int intersect into key kill lineno load merge money national nchar nocheck nocount nolock nonclustered ntext numeric nvarchar of off offsets on open opendatasource openquery openrowset openxml option order over percent plan precision primary print proc procedure public raiserror read readtext real reconfigure references replication restore restrict return revert revoke rollback rowcount rowguidcol rule save schema securityaudit select set setuser shutdown smalldatetime smallint smallmoney sql_variant statistics table table tablesample text textsize then time timestamp tinyint to top tran transaction trigger truncate try union unique uniqueidentifier update updatetext use user values varbinary varchar varying view waitfor when where while with writetext xml go ");
            // Word2 = 1
            codeScintilla.SetKeywords(1, @"ifnull left right sum trim least mid date_sub count now concat length replace ascii cast char charindex ceiling coalesce collate contains convert current_date current_time current_timestamp current_user floor isnull max min nullif object_id session_user substring system_user tsequal ");
            // User1 = 4
            codeScintilla.SetKeywords(4, @"all and any between cross exists in inner is join like not null or outer pivot some unpivot ( ) * ");
            // User2 = 5
            codeScintilla.SetKeywords(5, @"call ");
        }

        public void LoadRoutineList(IList<Routine> routines)
        {
            Cursor = Cursors.WaitCursor;
            objectExplorerListBox.SuspendLayout();

            objectExplorerListBox.DataSource = routines;

            objectExplorerListBox.ResumeLayout();
            Cursor = Cursors.Default;
        }

        public void OpenRoutine(Routine routine)
        {
            Cursor = Cursors.WaitCursor;
            codeScintilla.SuspendLayout();

            var code = FixLexerCompatibility(routine.Definition);

            codeScintilla.Text = code;

            codeScintilla.ResumeLayout();
            Cursor = Cursors.Default;
        }

        public void GoToPosition(int position)
        {
            codeScintilla.SelectionStart = position;
            codeScintilla.SelectionEnd = position;
        }

        private static string FixLexerCompatibility(string code)
        {
            // Scintilla SQL lexer doesn't recognize # as comment, so, replace by --
            return Regex.Replace(code, @"(?<=[\s\t]+)#", "--");
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            await Initialize();

            codeScintilla.ClearCmdKey(Keys.Control | Keys.Add);
        }

        private async void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.OemMinus)
            {
                await NavigateBackward();
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

        private void SearchFired(object sender, EventArgs e)
        {
            Search();
        }

        private async void codeScintilla_DoubleClick(object sender, DoubleClickEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SelectedIdentifier))
            {
                return;
            }

            await DefinitionRequested();
        }

        private void codeScintilla_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Avoid adding control caracters
            if (e.KeyChar == 31)
            {
                e.Handled = true;
            }
        }

        private async void navigateBackwardButton_Click(object sender, EventArgs e)
        {
            await NavigateBackward();
        }

        private void codeScintilla_UpdateUI(object sender, UpdateUIEventArgs e)
        {
            if (e.Change == UpdateChange.Selection)
            {
                TextSelected(codeScintilla.SelectedText);
            }
        }
                
        public void HighlightText(string text)
        {            
            codeScintilla.IndicatorCurrent = HighlightIndicator;
            codeScintilla.IndicatorClearRange(0, codeScintilla.TextLength);

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }
                     
            codeScintilla.TargetStart = 0;
            codeScintilla.TargetEnd = codeScintilla.TextLength;
            codeScintilla.SearchFlags = SearchFlags.None;
            
            while (codeScintilla.SearchInTarget(text) != ScintilaNotFound)
            {                
                codeScintilla.IndicatorFillRange(codeScintilla.TargetStart, codeScintilla.TargetEnd - codeScintilla.TargetStart);
                             
                codeScintilla.TargetStart = codeScintilla.TargetEnd;
                codeScintilla.TargetEnd = codeScintilla.TextLength;
            }
        }
    }
}