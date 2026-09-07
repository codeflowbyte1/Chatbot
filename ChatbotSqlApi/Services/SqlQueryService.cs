using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ChatbotSqlApi.Services
{
    public class SqlQueryService : ISqlQueryService
    {
        private readonly string _connectionString;
        private const int DefaultCommandTimeoutSeconds = 30;

        public SqlQueryService()
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["ChatbotDbConnection"];
            if (connectionStringSettings == null || string.IsNullOrWhiteSpace(connectionStringSettings.ConnectionString))
            {
                throw new InvalidOperationException("Database connection string 'ChatbotDbConnection' is missing or empty in Web.config.");
            }
            _connectionString = connectionStringSettings.ConnectionString;
        }

        public SqlQueryService(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
            }
            _connectionString = connectionString;
        }

        public DataTable ExecuteQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));
            }

            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.CommandTimeout = DefaultCommandTimeoutSeconds;
                command.CommandType = CommandType.Text;

                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    connection.Open();
                    adapter.Fill(dataTable);
                }
            }

            return dataTable;
        }
    }
}
