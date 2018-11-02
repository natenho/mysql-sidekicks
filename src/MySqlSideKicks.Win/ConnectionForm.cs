using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MySqlSideKicks.Win
{
    public partial class ConnectionForm : Form
    {
        private SortedDictionary<string, string> _connections = new SortedDictionary<string, string>();

        private string _currentConnection;

        public ConnectionForm()
        {
            InitializeComponent();
        }

        private MySqlConnection CreateMySqlConnection()
        {
            var connectionString = CreateConnectionString();

            StoreLastUsedConnectionString(nameTextBox.Text, connectionString);

            return new MySqlConnection(connectionString);
        }

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

        private void OpenConnection(string name, string connectionString)
        {
            try
            {
                var connectionStringBuilder = new MySqlConnectionStringBuilder(connectionString);

                nameTextBox.Text = name;
                hostNameTextBox.Text = connectionStringBuilder.Server;
                portTextBox.Text = connectionStringBuilder.Port.ToString();
                userNameTextBox.Text = connectionStringBuilder.UserID;
                passwordTextBox.Text = connectionStringBuilder.Password;
                defaultSchemaTextBox.Text = connectionStringBuilder.Database;

                _currentConnection = name;
            }
            catch (Exception ex)
            {
                ShowError(ex.ToString());
            }
        }

        private void StoreLastUsedConnectionString(string name, string connectionString)
        {
            try
            {
                Properties.Settings.Default.LastUsed = connectionString.ToEncryptedBase64();
                Properties.Settings.Default.Save();

                if (!string.IsNullOrWhiteSpace(name))
                {
                    _connections.AddOrUpdate(name, connectionString);
                    SaveConnections();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.ToString());
            }
        }

        private void LoadConnections()
        {
            string json = Properties.Settings.Default.ConnectionStrings;

            connectionList.DataSource = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            _connections = JsonConvert.DeserializeObject<SortedDictionary<string, string>>(json.DecryptToString());

            if (_connections.Count == 0)
            {
                return;
            }

            connectionList.DataSource = new BindingSource(_connections, null);
            connectionList.DisplayMember = "Key";
            connectionList.ValueMember = "Value";
        }

        private void SaveConnections()
        {
            var json = JsonConvert.SerializeObject(_connections);
            Properties.Settings.Default.ConnectionStrings = json.ToEncryptedBase64();
            Properties.Settings.Default.Save();
        }

        private void SaveCurrentConnection()
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                ShowInformation("Missing conneciton name.");
                return;
            }

            _connections.AddOrUpdate(nameTextBox.Text, CreateConnectionString());

            SaveConnections();
            LoadConnections();

            connectionList.SelectedItem = _connections.FirstOrDefault(i => i.Key == nameTextBox.Text);
        }

        private void ShowInformation(string text)
        {
            MessageBox.Show(text, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowError(string text)
        {
            MessageBox.Show(text, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ConnectionForm_Load(object sender, EventArgs e)
        {
            OpenConnection(string.Empty, Properties.Settings.Default.LastUsed.DecryptToString());
            LoadConnections();
        }

        private void connectionList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (connectionList.Items.Count == 0)
            {
                return;
            }

            if (connectionList.SelectedItem is KeyValuePair<string, string> selectedItem)
            {
                OpenConnection(selectedItem.Key, selectedItem.Value);
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            SaveCurrentConnection();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (connectionList.SelectedItem == null)
            {
                return;
            }

            var selectedItem = (KeyValuePair<string, string>)connectionList.SelectedItem;

            _connections.Remove(selectedItem.Key);

            SaveConnections();
            LoadConnections();
        }

        private void duplicateButton_Click(object sender, EventArgs e)
        {
            if (connectionList.SelectedItem == null)
            {
                return;
            }

            var selectedItem = (KeyValuePair<string, string>)connectionList.SelectedItem;

            string generatedName = null;
            int index = 0;

            do
            {
                generatedName = $"{selectedItem.Key}{++index:00}";
            }
            while (_connections.ContainsKey(generatedName));

            _connections.Add(generatedName, selectedItem.Value);

            SaveConnections();
            LoadConnections();
        }

        private async void testConnectionButton_Click(object sender, EventArgs e)
        {
            bool succeeded;
            string message;

            Cursor = Cursors.WaitCursor;
            progressBar.Visible = true;

            try
            {
                using (var connection = CreateMySqlConnection())
                {
                    await connection.Test();
                }

                succeeded = true;
                message = "Connection succeeded.";
            }
            catch (Exception ex)
            {
                succeeded = false;
                message = ex.Message;
            }

            progressBar.Visible = false;
            Cursor = Cursors.Default;

            if (succeeded)
            {
                ShowInformation(message);
            }
            else
            {
                ShowError(message);
            }
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            var connection = CreateMySqlConnection();
            var sessionForm = new MainForm(connection) { Text = $"{nameTextBox.Text} - {userNameTextBox.Text}@{hostNameTextBox.Text}:{portTextBox.Text}" };

            sessionForm.Show(this);
        }

        private void textBox_Validated(object sender, EventArgs e)
        {
            _connections.Remove(_currentConnection);
            SaveCurrentConnection();
        }
    }
}
