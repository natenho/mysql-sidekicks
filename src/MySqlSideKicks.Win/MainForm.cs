using MySql.Data.MySqlClient;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MySqlSideKicks.Win
{
    public partial class MainForm : Form
    {
        private Routine _currentRoutine;

        private readonly Stack<Navigation> _navigationStack = new Stack<Navigation>();
        private readonly List<Routine> _routines = new List<Routine>();

        private class Navigation
        {
            public Routine Routine { get; set; }
            public int Position { get; set; }
        }

        private class Routine
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

        public MainForm()
        {
            InitializeComponent();
            InitializeSyntaxHighlighting();
        }

        private void InitializeSyntaxHighlighting()
        {
            codeScintilla.StyleResetDefault();
            codeScintilla.Styles[Style.Default]
                .Font = "Consolas";
            codeScintilla.Styles[Style.Default]
                .Size = 10;
            codeScintilla.StyleClearAll();

            codeScintilla.Lexer = Lexer.Sql;
            // Used to recognize full qualified identifiers like `schema`.`storedprocedure` as one word
            codeScintilla.WordChars += ".`";

            codeScintilla.Styles[Style.Sql.Comment]
                .ForeColor = Color.Green;
            codeScintilla.Styles[Style.Sql.CommentLine]
                .ForeColor = Color.Green;
            codeScintilla.Styles[Style.Sql.CommentLineDoc]
                .ForeColor = Color.Green;
            codeScintilla.Styles[Style.Sql.Number]
                .ForeColor = Color.Maroon;
            codeScintilla.Styles[Style.Sql.Word]
                .ForeColor = Color.Blue;
            codeScintilla.Styles[Style.Sql.Word]
                .Bold = true;
            codeScintilla.Styles[Style.Sql.Word2]
                .ForeColor = Color.Fuchsia;
            codeScintilla.Styles[Style.Sql.User1]
                .ForeColor = Color.Gray;
            codeScintilla.Styles[Style.Sql.User2]
                .BackColor = Color.Yellow;
            codeScintilla.Styles[Style.Sql.User2]
                .Bold = true;
            codeScintilla.Styles[Style.Sql.String]
                .ForeColor = Color.Red;
            codeScintilla.Styles[Style.Sql.Character]
                .ForeColor = Color.Red;
            codeScintilla.Styles[Style.Sql.Operator]
                .ForeColor = Color.Black;

            // Word = 0
            codeScintilla.SetKeywords(
                0,
                @"interval month day year prepare duplicate elseif ignore out integer leave add alter as authorization backup begin bigint binary bit break browse bulk by cascade case catch check checkpoint close clustered column commit compute constraint containstable continue create current cursor cursor database date datetime datetime2 datetimeoffset dbcc deallocate decimal declare default delete deny desc disk distinct distributed double drop dump else end errlvl escape except exec execute exit external fetch file fillfactor float for foreign freetext freetexttable from full function goto grant group having hierarchyid holdlock identity identity_insert identitycol if image index insert int intersect into key kill lineno load merge money national nchar nocheck nocount nolock nonclustered ntext numeric nvarchar of off offsets on open opendatasource openquery openrowset openxml option order over percent plan precision primary print proc procedure public raiserror read readtext real reconfigure references replication restore restrict return revert revoke rollback rowcount rowguidcol rule save schema securityaudit select set setuser shutdown smalldatetime smallint smallmoney sql_variant statistics table table tablesample text textsize then time timestamp tinyint to top tran transaction trigger truncate try union unique uniqueidentifier update updatetext use user values varbinary varchar varying view waitfor when where while with writetext xml go ");
            // Word2 = 1
            codeScintilla.SetKeywords(
                1,
                @"ifnull left right sum trim least mid date_sub count now concat length replace ascii cast char charindex ceiling coalesce collate contains convert current_date current_time current_timestamp current_user floor isnull max min nullif object_id session_user substring system_user tsequal ");
            // User1 = 4
            codeScintilla.SetKeywords(4, @"all and any between cross exists in inner is join like not null or outer pivot some unpivot ( ) * ");
            // User2 = 5
            codeScintilla.SetKeywords(5, @"call ");
        }
        
        private async Task RefreshObjectExplorer()
        {
            if (_routines.Count == 0)
            {
                await LoadRoutines();
            }

            var search = Regex.Escape(searchBox.Text);

            var filtered = new object[] { };

            if (filterByName.Checked)
            {
                filtered = _routines.Where(r => r.MatchesIdentifier(searchBox.Text, _currentRoutine?.Schema) || Regex.IsMatch(r.ToString(), search, RegexOptions.IgnoreCase))
                    .Cast<object>()
                    .ToArray();
            }

            if (filterByDefinition.Checked)
            {
                filtered = _routines.Where(r => Regex.IsMatch(r.Definition, search, RegexOptions.IgnoreCase))
                    .Cast<object>()
                    .ToArray();
            }

            objectExplorerListBox.SuspendLayout();
            objectExplorerListBox.Items.Clear();
            objectExplorerListBox.Items.AddRange(filtered);
            objectExplorerListBox.ResumeLayout();
        }

        private async Task LoadRoutines()
        {
            var connection = new MySqlConnection(Properties.Settings.Default.ConnectionString);
            var command = connection.CreateCommand();

            try
            {
                connection.Open();
                command.CommandText =
                    "SELECT ROUTINE_SCHEMA, ROUTINE_NAME, ROUTINE_TYPE, ROUTINE_DEFINITION FROM INFORMATION_SCHEMA.ROUTINES ORDER BY ROUTINE_SCHEMA, ROUTINE_NAME";
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var routine = new Routine()
                        {
                            Schema = reader["ROUTINE_SCHEMA"]
                                .ToString(),
                            Name = reader["ROUTINE_NAME"]
                                .ToString(),
                            Type = reader["ROUTINE_TYPE"]
                                .ToString(),
                            Definition = reader["ROUTINE_DEFINITION"]
                                .ToString()
                        };

                        _routines.Add(routine);
                    }
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private async Task OpenRoutine(Routine routine)
        {
            if (routine == null)
            {
                return;
            }

            var connection = new MySqlConnection(Properties.Settings.Default.ConnectionString);
            var command = connection.CreateCommand();

            try
            {
                await connection.OpenAsync();

                command.CommandText = $"SHOW CREATE {routine.Type} `{routine.Schema}`.`{routine.Name}`";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();

                    var code = reader[$"CREATE {routine.Type}"]
                        .ToString();

                    code = FixLexerCompatibility(code);

                    codeScintilla.Text = code;

                    _currentRoutine = routine;
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
        }

        private static string FixLexerCompatibility(string code)
        {
            // Scintilla SQL lexer doesn't recognize # as comment, so, replace by --
            return Regex.Replace(code, @"(?<=[\s\t]+)#", "--");
        }

        private async Task TryNavigate()
        {
            var selectedCode = codeScintilla.GetWordFromPosition(codeScintilla.CurrentPosition);

            var foundRoutine = _routines.Find(routine => routine.MatchesIdentifier(selectedCode, _currentRoutine.Schema));

            if (foundRoutine != null && foundRoutine != _currentRoutine)
            {
                _navigationStack.Push(
                    new Navigation
                    {
                        Position = codeScintilla.CurrentPosition,
                        Routine = _currentRoutine
                    });

                previousButton.Enabled = true;

                codeScintilla.SuspendLayout();

                await OpenRoutine(foundRoutine);
                
                codeScintilla.SelectionStart = 0;
                codeScintilla.SelectionEnd = 0;

                codeScintilla.ResumeLayout();
            }
        }
        
        private async Task GoToPrevious()
        {
            if (_navigationStack.Count == 0)
            {
                return;
            }

            var navigation = _navigationStack.Pop();

            codeScintilla.SuspendLayout();

            await OpenRoutine(navigation.Routine);
            
            codeScintilla.SelectionStart = navigation.Position;
            codeScintilla.SelectionEnd = navigation.Position;
            
            codeScintilla.ResumeLayout();

            previousButton.Enabled = _navigationStack.Count > 0;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            await RefreshObjectExplorer();

            codeScintilla.ClearCmdKey(Keys.Control | Keys.Add);
        }

        private async void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (_navigationStack.Count == 0)
            {
                return;
            }

            if (e.Control && e.KeyCode == Keys.OemMinus)
            {
                await GoToPrevious();
            }
        }
        
        private async void SearchFired(object sender, EventArgs e)
        {
            await RefreshObjectExplorer();
        }

        private async void objectExplorerListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _navigationStack.Clear();
            await OpenRoutine(objectExplorerListBox.SelectedItem as Routine);
        }
        
        private void objectExplorerListBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsLetterOrDigit(e.KeyChar))
            {
                searchBox.Focus();
                searchBox.AppendText(e.KeyChar.ToString());
            }
        }
        
        private async void codeScintilla_DoubleClick(object sender, DoubleClickEventArgs e)
        {
            await TryNavigate();
        }

        private void codeScintilla_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Avoid adding control caracters
            if (e.KeyChar == 31)
            {
                e.Handled = true;
            }
        }
  
        private async void previousButton_Click(object sender, EventArgs e)
        {
            await GoToPrevious();
        }
    }
}