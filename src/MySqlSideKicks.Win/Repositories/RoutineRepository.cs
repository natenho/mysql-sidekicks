using Dapper;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MySqlSideKicks.Win
{
    class RoutineRepository : IRoutineRepository
    {

        private MySqlConnection _connection;

        public RoutineRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<Routine>> GetAll()
        {
            try
            {
                _connection.Open();
                return await _connection.QueryAsync<Routine>("SELECT ROUTINE_SCHEMA `Schema`, ROUTINE_NAME `Name`, ROUTINE_TYPE `Type`, ROUTINE_DEFINITION `Definition` FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA NOT IN ('sys') ORDER BY ROUTINE_SCHEMA, ROUTINE_NAME");
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
            }
        }

        public async Task<string> GetFullDefinition(Routine routine)
        {
            var command = _connection.CreateCommand();

            try
            {
                await _connection.OpenAsync();

                command.CommandText = $"SHOW CREATE {routine.Type} `{routine.Schema}`.`{routine.Name}`";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();

                    var code = reader[$"CREATE {routine.Type}"]
                        .ToString();

                    return code;
                }
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                {
                    await _connection.CloseAsync();
                }
            }
        }
    }
}
