using ScintillaNET;
using System;
using System.Collections.Generic;
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
        public event Func<Task> NavigatedForward;

        public string Filter => searchBox.Text;
        public FilterMode FilterMode => filterByDefinition.Checked ? FilterMode.ByDefinition : FilterMode.ByName;

        public int CurrentPosition => editor.CurrentPosition;
        public bool NavigateBackwardAllowed { get => navigateBackwardToolStripButton.Enabled; set => navigateBackwardToolStripButton.Enabled = value; }
        public bool NavigateForwardAllowed { get => navigateForwardToolStripButton.Enabled; set => navigateForwardToolStripButton.Enabled = value; }

        const int ScintilaNotFound = -1;

        private readonly HashSet<string> _routineHashSet = new HashSet<string>();

        private bool _isSearching;
        private bool _isOpeningRoutine;

        public SessionForm()
        {
            InitializeComponent();
        }

        public void LoadRoutineList(IList<Routine> routines)
        {
            Cursor = Cursors.WaitCursor;
            objectExplorerListBox.SuspendLayout();

            var selectedRoutine = objectExplorerListBox.SelectedItem as Routine;

            objectExplorerListBox.DataSource = routines;

            //TODO Move to presenter
            foreach (var routine in routines)
            {
                _routineHashSet.Add(routine.Name);
                _routineHashSet.Add(routine.FullName);
                _routineHashSet.Add(routine.QuotedFullName);
            }

            editor.SetKeywords(KeywordSet.User2, string.Join(" ", _routineHashSet).ToLower());

            var reSelectedRoutine = routines.FirstOrDefault(r => r.IdentifierMatches(selectedRoutine?.ToString()));
            if (reSelectedRoutine != null)
            {
                objectExplorerListBox.SelectedItem = reSelectedRoutine;
            }

            objectExplorerListBox.ResumeLayout();
            Cursor = Cursors.Default;
        }

        public void OpenRoutine(Routine routine)
        {
            _isOpeningRoutine = true;

            Cursor = Cursors.WaitCursor;
            editor.SuspendLayout();

            var code = FixLexerCompatibility(routine.Definition);

            editor.Text = code;
                       
            objectExplorerListBox.SelectedItem = routine;
                        
            editor.ResumeLayout();
            editor.Focus();
                       
            Cursor = Cursors.Default;

            _isOpeningRoutine = false;
        }

        public void GoToPosition(int position)
        {
            editor.ClearSelections();

            editor.SelectionStart = position;
            editor.SelectionEnd = position;

            var identifier = editor.GetWordFromPosition(editor.CurrentPosition);

            IdentifierActivated?.Invoke(identifier);
        }

        private static string FixLexerCompatibility(string code)
        {
            // Scintilla's built-in SQL lexer doesn't recognize # as comment, so, replace by --
            return Regex.Replace(code, @"(?<=[\s\t]+)#", "--");
        }

        private void control_SearchPerformed(object sender, EventArgs e)
        {
            _isSearching = true;

            SearchPerformed?.Invoke();

            _isSearching = false;
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

            if (e.Control && e.KeyCode == Keys.F12)
            {
                var identifier = editor.GetWordFromPosition(editor.CurrentPosition);

                await NavigationRequested?.Invoke(identifier);
            }
        }

        private async void objectExplorerListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isSearching || _isOpeningRoutine)
            {
                return;
            }

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

        private async void navigateBackwardSplitButton_Click(object sender, EventArgs e)
        {
            await NavigatedBackward?.Invoke();
        }

        private async void navigateForwardToolStripButton_Click(object sender, EventArgs e)
        {
            await NavigatedForward?.Invoke();
        }

        public void HighlightWord(string word)
        {
            editor.SearchFlags = SearchFlags.WholeWord;

            HighlightCode(word);
        }

        public void HighlightPattern(string pattern)
        {
            if (!pattern.IsValidRegex())
            {
                return;
            }

            editor.SearchFlags = SearchFlags.Regex;

            HighlightCode(pattern, 1);
        }

        private void HighlightCode(string input, int minOccurrences = 2)
        {
            editor.IndicatorCurrent = CustomIndicator.Highlight;
            editor.IndicatorClearRange(0, editor.TextLength);

            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            editor.TargetStart = 0;
            editor.TargetEnd = editor.TextLength;

            int occurencesCount = 0;

            while (editor.SearchInTarget(input) != ScintilaNotFound)
            {
                editor.IndicatorFillRange(editor.TargetStart, editor.TargetEnd - editor.TargetStart);

                editor.TargetStart = editor.TargetEnd;
                editor.TargetEnd = editor.TextLength;

                occurencesCount++;
            }

            if (occurencesCount < minOccurrences)
            {
                editor.IndicatorClearRange(0, editor.TextLength);
            }
        }
    }
}
