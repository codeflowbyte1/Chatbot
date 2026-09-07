using System.Data;

namespace ChatbotSqlApi.Services
{
    public interface ISqlQueryService
    {
        DataTable ExecuteQuery(string query);
    }
}
