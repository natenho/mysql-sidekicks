using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySqlSideKicks.Win.Models;
using Newtonsoft.Json;

namespace MySqlSideKicks.Win.Repositories
{
    public class ConnectionRepository : IConnectionRepository
    {
        public Task<IList<Connection>> GetAll()
        {
            IList<Connection> connections = new List<Connection>();

            string json = Properties.Settings.Default.ConnectionStrings;
            if (string.IsNullOrWhiteSpace(json))
            {
                return Task.FromResult(connections);
            }

            connections = JsonConvert.DeserializeObject<IList<Connection>>(json.DecryptToString());
            
            return Task.FromResult(connections);
        }

        public Task<Connection> GetLastUsed()
        {
            var json = Properties.Settings.Default.LastUsed;
            if (string.IsNullOrWhiteSpace(json))
            {
                return Task.FromResult(default(Connection));
            }

            var connection = JsonConvert.DeserializeObject<Connection>(json.DecryptToString());

            return Task.FromResult(connection);
        }

        public void SaveAll(IList<Connection> connections)
        {
            var json = JsonConvert.SerializeObject(connections);
            Properties.Settings.Default.ConnectionStrings = json.ToEncryptedBase64();
            Properties.Settings.Default.Save();
        }

        public void SaveLastUsed(Connection connection)
        {
            var json = JsonConvert.SerializeObject(connection);
            Properties.Settings.Default.LastUsed = json.ToEncryptedBase64();
            Properties.Settings.Default.Save();
        }
    }
}
