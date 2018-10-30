using Dapper;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MySqlSideKicks.Win
{
    class RoutineRepository
    {
        private static IEnumerable<Routine> _routines;
        private MySqlConnection _connection;

        public RoutineRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<Routine>> SearchByName(string name, string defaulSchema = "")
        {
            var routines = await GetAll();

            var search = Regex.Escape(name);

            return routines.Where(r => r.MatchesIdentifier(name, defaulSchema) || Regex.IsMatch(r.ToString(), search, RegexOptions.IgnoreCase));
        }

        public async Task<IEnumerable<Routine>> SearchByDefinition(string text)
        {
            var routines = await GetAll();

            var search = Regex.Escape(text);

            return routines.Where(r => Regex.IsMatch(r.Definition, search, RegexOptions.IgnoreCase));
        }

        private async Task<IEnumerable<Routine>> GetAll()
        {
            if (_routines != null)
            {
                return _routines;
            }

            try
            {
                _connection.Open();
                _routines = await _connection.QueryAsync<Routine>("SELECT ROUTINE_SCHEMA `Schema`, ROUTINE_NAME `Name`, ROUTINE_TYPE `Type`, ROUTINE_DEFINITION `Definition` FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA NOT IN ('sys') ORDER BY ROUTINE_SCHEMA, ROUTINE_NAME");

                return _routines;
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
            }
        }
    }
}
