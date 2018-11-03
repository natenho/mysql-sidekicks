using System.Collections.Generic;
using System.Threading.Tasks;

namespace MySqlSideKicks.Win
{
    interface IRoutineRepository
    {
        Task<IEnumerable<Routine>> GetAll();
        Task<string> GetFullDefinition(Routine routine);
    }
}