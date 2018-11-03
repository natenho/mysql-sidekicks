using MySql.Data.MySqlClient;
using MySqlSideKicks.Win.Models;
using MySqlSideKicks.Win.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MySqlSideKicks.Win
{
    public partial class ConnectionForm : Form, IConnectionView
    {
        public event Func<Task> Initialize;
        public event Action<ConnectionViewModel> ConnectionSelected;
        public event Func<ConnectionViewModel, Task> ConnectionChanged;
        public event Func<Task> New;
        public event Func<Task> Delete;
        public event Func<Task> Test;
        public event Func<Task> Connect;

        public ConnectionForm()
        {
            InitializeComponent();
        }

        private MySqlConnection CreateMySqlConnection()
        {
            var connectionString = CreateConnectionString();
            return new MySqlConnection(connectionString);
        }

        //TODO Move to presenter
        private string CreateConnectionString()
        {
            var connectionStringBuilder = new MySqlConnectionStringBuilder
            {
                Server = hostNameTextBox.Text,
                UserID = userNameTextBox.Text,
                Password = passwordTextBox.Text,
                Database = defaultSchemaTextBox.Text,
                PersistSecurityInfo = false,
                OldGuids = true
            };

            if (uint.TryParse(portTextBox.Text, out var port))
            {
                connectionStringBuilder.Port = port;
            }

            return connectionStringBuilder.ConnectionString;
        }
        
        private void ConnectionForm_Load(object sender, EventArgs e)
        {
            Initialize?.Invoke();
        }

        private void connectionList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConnectionSelected(connectionList.SelectedItem as ConnectionViewModel);
        }

        private void newButton_Click(object sender, EventArgs e)
        {
            New?.Invoke();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            Delete?.Invoke();
        }

        private async void testConnectionButton_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            progressBar.Visible = true;

            if (Test != null)
            {
                await Test();
            }

            progressBar.Visible = false;
            Cursor = Cursors.Default;
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            progressBar.Visible = true;

            Connect?.Invoke();
            StartNewSession();

            progressBar.Visible = false;
            Cursor = Cursors.Default;
        }

        private void connectionBindingSource_CurrentItemChanged(object sender, EventArgs e)
        {
            ConnectionChanged?.Invoke(connectionBindingSource.Current as ConnectionViewModel);
        }

        //TODO Move to presenter
        private void StartNewSession()
        {
            var connection = CreateMySqlConnection();
            var repository = new RoutineRepository(connection);

            var sessionForm = new SessionForm() { Text = $"{nameTextBox.Text} - {userNameTextBox.Text}@{hostNameTextBox.Text}:{portTextBox.Text}" };
            var presenter = new SessionPresenter(sessionForm, repository);

            sessionForm.Show(this);
        }
        
        public void ShowInformationMessage(string text)
        {
            MessageBox.Show(text, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ShowErrorMessage(string text)
        {
            MessageBox.Show(text, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void LoadConnectionList(IList<ConnectionViewModel> connections)
        {
            Cursor = Cursors.WaitCursor;
            connectionList.SuspendLayout();

            var previouslySelected = connectionList.SelectedItem as ConnectionViewModel;

            connectionList.DataSource = new BindingList<ConnectionViewModel>(connections);

            previouslySelected = connections.FirstOrDefault(c => c.Guid == previouslySelected.Guid);

            if (previouslySelected != null)
            {
                connectionList.SelectedItem = previouslySelected;
            }

            connectionList.ResumeLayout();
            Cursor = Cursors.Default;
        }

        public void OpenConnection(ConnectionViewModel connection)
        {
            Cursor = Cursors.WaitCursor;
            SuspendLayout();

            if (connectionBindingSource.DataSource == null)
            {
                connectionBindingSource.DataSource = connection;

                nameTextBox.DataBindings.Add("Text", connectionBindingSource, "Name");
                hostNameTextBox.DataBindings.Add("Text", connectionBindingSource, "Server");
                portTextBox.DataBindings.Add("Text", connectionBindingSource, "Port");
                userNameTextBox.DataBindings.Add("Text", connectionBindingSource, "UserID");
                passwordTextBox.DataBindings.Add("Text", connectionBindingSource, "Password");
                defaultSchemaTextBox.DataBindings.Add("Text", connectionBindingSource, "Database");
            }
            else
            {
                connectionBindingSource.DataSource = connection;
            }

            ResumeLayout();
            Cursor = Cursors.Default;
        }       
    }
}
