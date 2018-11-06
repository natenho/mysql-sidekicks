using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MySqlSideKicks.Win
{
    interface ISessionView
    {
        event Func<Task> Initialized;
        event Action SearchPerformed;
        event Func<Routine, Task> RoutineSelected;
        event Func<Task> DefinitionRequested;
        event Func<Task> NavigatedBackward;
        event Action<string> IdentifierActivated;

        string Filter { get; }
        FilterMode FilterMode { get; }

        int CurrentPosition { get; }
        string SelectedIdentifier { get; }

        bool CanNavigateBackward { get; set; }
        
        void LoadRoutineList(IList<Routine> routines);
        void OpenRoutine(Routine routine);

        void GoToPosition(int position);
        void HighlightText(string text);
    }
}
