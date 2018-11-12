using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public bool NavigateBackwardAllowed { get => navigateBackwardButton.Enabled; set => navigateBackwardButton.Enabled = value; }

        const int ScintilaNotFound = -1;

        private string _routineKeywords;        

        public SessionForm()
        {
            InitializeComponent();                   
        }        

        public void LoadRoutineList(IList<Routine> routines)
        {
            Cursor = Cursors.WaitCursor;
            objectExplorerListBox.SuspendLayout();

            objectExplorerListBox.DataSource = routines;

            //TODO Move to presenter
            var routineKeywordsBare = string.Join(" ", routines.Select(r => r.Name.ToLower()));
            var routineKeywordsWithSchema = string.Join(" ", routines.Select(r => $"{r.Schema.ToLower()}.{r.Name.ToLower()}"));
            var routineKeywordsWithSchemaFullyQualified = string.Join(" ", routines.Select(r => $"`{r.Schema.ToLower()}`.`{r.Name.ToLower()}`"));
            _routineKeywords = string.Join(" ", routineKeywordsBare, routineKeywordsWithSchema, routineKeywordsWithSchemaFullyQualified);
                        
            editor.SetKeywords(KeywordSet.User2, _routineKeywords);

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
            // Scintilla's built-in SQL lexer doesn't recognize # as comment, so, replace by --
            return Regex.Replace(code, @"(?<=[\s\t]+)#", "--");
        }

        private void control_SearchPerformed(object sender, EventArgs e)
        {
            SearchPerformed?.Invoke();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            editor.InitializeMySqlSyntaxHighlighting();

            await Initialized?.Invoke();                    
        }

        private async void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.OemMinus)
            {
                await NavigatedBackward?.Invoke();
            }
        }

        private async void objectExplorerListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            await RoutineSelected?.Invoke(objectExplorerListBox.SelectedItem as Routine);
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

            await NavigationRequested?.Invoke(identifier);
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

                IdentifierActivated?.Invoke(identifier);
            }
        }

        private async void navigateBackwardButton_Click(object sender, EventArgs e)
        {
            await NavigatedBackward?.Invoke();
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
