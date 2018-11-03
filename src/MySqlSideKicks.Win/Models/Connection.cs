using MySql.Data.MySqlClient;
using System;

namespace MySqlSideKicks.Win.Models
{
    public class Connection
    {
        private const uint MySqlDefaultPort = 3306;

        public Connection()
        {
            Guid = Guid.NewGuid();
            Port = MySqlDefaultPort;
        }

        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Server { get; set; }
        public uint Port { get; set; }
        public string UserID { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }

        public string ToMySqlConnectionString()
        {
            var connectionStringBuilder = new MySqlConnectionStringBuilder
            {
                Server = Server,
                Port = Port,
                UserID = UserID,
                Password = Password,
                Database = Database,
                PersistSecurityInfo = false,
                OldGuids = true
            };

            return connectionStringBuilder.ConnectionString;
        }
    }
}
