using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Windows.Forms;

namespace MySqlSideKicks.Win
{
    public partial class ConnectionForm : Form
    {
        public ConnectionForm()
        {
            InitializeComponent();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            var connection = CreateConnection();
            var sessionForm = new MainForm(connection) { Text = $"{userNameTextBox.Text}@{hostNameTextBox.Text}:{portTextBox.Text}" };

            sessionForm.Show();
            Hide();
        }

        private MySqlConnection CreateConnection()
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

            StoreConnection(connectionStringBuilder.ConnectionString);

            return new MySqlConnection(connectionStringBuilder.ConnectionString);
        }

        private async void testConnectionButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    if (await connection.Test())
                    {
                        MessageBox.Show("Connection succeeded.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RestoreLastConnection()
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
                var section = config.GetSection("connectionStrings") as ConnectionStringsSection;
                var lastConnectionStringSetting = section.ConnectionStrings["lastUsed"];

                var connectionStringBuilder = new MySqlConnectionStringBuilder(lastConnectionStringSetting.ConnectionString);

                hostNameTextBox.Text = connectionStringBuilder.Server;
                portTextBox.Text = connectionStringBuilder.Port.ToString();
                userNameTextBox.Text = connectionStringBuilder.UserID;
                passwordTextBox.Text = connectionStringBuilder.Password;
                defaultSchemaTextBox.Text = connectionStringBuilder.Database;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void StoreConnection(string connectionString)
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
                var section = config.GetSection("connectionStrings") as ConnectionStringsSection;
                section.ConnectionStrings.Add(new ConnectionStringSettings("lastUsed", connectionString));
                //section.SectionInformation.ProtectSection("RsaProtectedConfigurationProvider");
                section.SectionInformation.ForceSave = true;
                config.Save(ConfigurationSaveMode.Modified);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ConnectionForm_Load(object sender, EventArgs e)
        {
            RestoreLastConnection();
        }
    }
}
