// ============================================================
//  Paralogamadha.Data / Infrastructure / BaseRepository.cs
// ============================================================

using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Paralogamadha.Data.Infrastructure
{
    public abstract class BaseRepository
    {
        protected readonly string ConnectionString;

        protected BaseRepository()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["ParalogamadhaDB"].ConnectionString;
        }

        protected IDbConnection CreateConnection()
        {
            var conn = new SqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }
    }
}
