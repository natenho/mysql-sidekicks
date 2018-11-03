using MySqlSideKicks.Win.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MySqlSideKicks.Win.Views
{
    public interface IConnectionView
    {
        event Func<Task> Initialize;
        event Action<ConnectionViewModel> ConnectionSelected;
        event Func<ConnectionViewModel, Task> ConnectionChanged;
        event Func<Task> New;
        event Func<Task> Delete;
        event Func<Task> Test;
        event Func<Task> Connect;

        void LoadConnectionList(IList<ConnectionViewModel> connections);
        void OpenConnection(ConnectionViewModel connection);

        void ShowInformationMessage(string message);
        void ShowErrorMessage(string message);
    }
}
