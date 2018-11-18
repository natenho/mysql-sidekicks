using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MySqlSideKicks.Win
{
    class SessionPresenter
    {
        private ISessionView _view;
        private IRoutineRepository _routineRepository;
        private IEnumerable<Routine> _routines;
        private Routine _currentRoutine;

        private readonly LinkedList<Navigation> _navigationLinkedList = new LinkedList<Navigation>();
        private LinkedListNode<Navigation> _currentNavigationNode;

        [DebuggerDisplay("{Routine.ToString()}@{Position}")]
        private class Navigation
        {
            public Routine Routine { get; set; }
            public int Position { get; set; }
        }

        public SessionPresenter(ISessionView view, IRoutineRepository repository)
        {
            _view = view;
            _routineRepository = repository;

            _view.Initialized += Initialized;
            _view.RoutineSelected += RoutineSelected;
            _view.SearchPerformed += SearchPerformed;
            _view.NavigationRequested += NavigationRequested;
            _view.NavigatedBackward += NavigateBackward;
            _view.NavigatedForward += NavigateForward;
            _view.IdentifierActivated += IdentifierActivated;
        }

        private async Task NavigateBackward()
        {
            await NavigateToHistoryItem(_currentNavigationNode.Previous);
        }

        private async Task NavigateForward()
        {
            await NavigateToHistoryItem(_currentNavigationNode.Next);
        }

        private async Task NavigationRequested(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return;
            }

            var foundRoutine = _routines.FirstOrDefault(routine => routine.MatchesIdentifier(identifier, _currentRoutine.Schema));

            if (foundRoutine != null && foundRoutine != _currentRoutine)
            {
                await NavigateToSpecificRoutine(foundRoutine);
            }
        }

        private async Task Initialized()
        {
            _routines = await _routineRepository.GetAll();

            _view.NavigateBackwardAllowed = false;
            _view.NavigateForwardAllowed = false;

            _view.LoadRoutineList(_routines.ToList());
        }

        private async Task RoutineSelected(Routine routine)
        {
            if (routine == null)
            {
                return;
            }

            await NavigateToSpecificRoutine(routine);

            _view.HighlightText(_view.Filter);
        }

        private async Task NavigateToHistoryItem(LinkedListNode<Navigation> navigationNode)
        {
            if(navigationNode == null)
            {
                return;
            }

            var navigation = navigationNode.Value;

            _currentNavigationNode = navigationNode;

            await OpenRoutine(navigation.Routine);

            _view.GoToPosition(navigation.Position);
        }

        private async Task NavigateToSpecificRoutine(Routine routine)
        {
            if (_currentNavigationNode != null)
            {
                _currentNavigationNode.Value.Position = _view.CurrentPosition;
            }

            while (_currentNavigationNode?.Next != null)
            {
                _navigationLinkedList.Remove(_currentNavigationNode.Next);
            }

            if (_currentRoutine != routine)
            {
                _currentNavigationNode = _navigationLinkedList.AddLast(new Navigation { Routine = routine });
            }

            await OpenRoutine(routine);
        }

        private async Task OpenRoutine(Routine routine)
        {
            routine.Definition = await _routineRepository.GetFullDefinition(routine);

            _currentRoutine = routine;

            _view.OpenRoutine(routine);

            _view.NavigateForwardAllowed = _currentNavigationNode.Next != null;
            _view.NavigateBackwardAllowed = _currentNavigationNode.Previous != null;
        }

        private void SearchPerformed()
        {
            var filteredRoutines = Enumerable.Empty<Routine>();
            var regexEscaptedFilter = Regex.Escape(_view.Filter);

            switch (_view.FilterMode)
            {
                case FilterMode.ByName:

                    filteredRoutines = _routines.Where(r => r.MatchesIdentifier(_view.Filter)
                        || Regex.IsMatch(r.ToString(), regexEscaptedFilter, RegexOptions.IgnoreCase));

                    break;

                case FilterMode.ByDefinition:

                    filteredRoutines = _routines.Where(r => Regex.IsMatch(r.Definition, regexEscaptedFilter, RegexOptions.IgnoreCase));
                    break;
            }

            _view.LoadRoutineList(filteredRoutines.ToList());
        }

        private void IdentifierActivated(string identifier)
        {
            _view.HighlightText(identifier);
        }
    }
}
