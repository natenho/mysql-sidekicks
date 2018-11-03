using MySqlSideKicks.Win.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MySqlSideKicks.Win.Repositories
{
    interface IConnectionRepository
    {
        Task<IList<Connection>> GetAll();
        Task<Connection> GetLastUsed();
        void SaveLastUsed(Connection connection);        
        void SaveAll(IList<Connection> connections);
    }
}
