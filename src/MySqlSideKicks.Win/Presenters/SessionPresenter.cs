using System.Collections.Generic;
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
            _view.IdentifierActivated += IdentifierActivated;
        }

        private async Task NavigateBackward()
        {
            if (_navigationLinkedList.Count == 0)
            {
                _view.NavigateBackwardAllowed = true;
                return;
            }

            var previous = _navigationLinkedList.Last;
            var navigation = previous.Value;

            await OpenRoutine(navigation.Routine, navigation.Position);
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
                _navigationLinkedList.AddLast(new Navigation
                {
                    Position = _view.CurrentPosition,
                    Routine = _currentRoutine
                });

                _view.NavigateBackwardAllowed = true;

                await OpenRoutine(foundRoutine);
            }

            if (_navigationLinkedList.Count == 0)
            {
                _view.NavigateBackwardAllowed = false;
            }
        }

        private async Task Initialized()
        {
            _routines = await _routineRepository.GetAll();

            _view.NavigateBackwardAllowed = false;

            _view.LoadRoutineList(_routines.ToList());
        }

        private async Task RoutineSelected(Routine routine)
        {
            if (routine == null)
            {
                return;
            }

            _navigationLinkedList.Clear();

            _view.NavigateBackwardAllowed = false;

            await OpenRoutine(routine);

            _view.HighlightText(_view.Filter);
        }

        private async Task OpenRoutine(Routine routine, int position = 0)
        {
            routine.Definition = await _routineRepository.GetFullDefinition(routine);

            _currentRoutine = routine;

            _view.OpenRoutine(routine);
            _view.GoToPosition(position);
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
