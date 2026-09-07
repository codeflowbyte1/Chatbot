using System.Collections.Generic;

namespace ChatbotSqlApi.Models
{
    public class ChatbotQueryResponse : ApiResponse<List<Dictionary<string, object>>>
    {
        public static ChatbotQueryResponse SuccessResult(List<Dictionary<string, object>> data, string message = "Query executed successfully.")
        {
            return new ChatbotQueryResponse
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ChatbotQueryResponse ErrorResult(string message)
        {
            return new ChatbotQueryResponse
            {
                Success = false,
                Message = message,
                Data = null
            };
        }
    }
}
