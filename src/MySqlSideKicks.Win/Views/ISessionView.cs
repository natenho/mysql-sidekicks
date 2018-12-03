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
        event Func<string, Task> NavigationRequested;
        event Func<Task> NavigatedBackward;
        event Func<Task> NavigatedForward;
        event Action<string> IdentifierActivated;

        string Filter { get; }
        FilterMode FilterMode { get; }

        int CurrentPosition { get; }

        bool NavigateBackwardAllowed { get; set; }
        bool NavigateForwardAllowed { get; set; }

        void LoadRoutineList(IList<Routine> routines);
        void OpenRoutine(Routine routine);

        void GoToPosition(int position);
        void HighlightWord(string word);
        void HighlightPattern(string pattern);
    }
}
