using System;
using System.Threading.Tasks;

namespace MySql.Data.MySqlClient
{
    public static class MySqlConnectionExtensions
    {
        public async static Task<bool> Test(this MySqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();

                return true;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
    }
}
