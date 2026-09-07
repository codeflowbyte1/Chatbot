using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ChatbotSqlApi.Helpers;
using ChatbotSqlApi.Models;
using ChatbotSqlApi.Services;

namespace ChatbotSqlApi.Controllers
{
    [RoutePrefix("api/chatbot")]
    public class ChatbotController : ApiController
    {
        private readonly ISqlQueryService _sqlQueryService;

        public ChatbotController() : this(new SqlQueryService())
        {
        }

        public ChatbotController(ISqlQueryService sqlQueryService)
        {
            _sqlQueryService = sqlQueryService ?? throw new ArgumentNullException(nameof(sqlQueryService));
        }

        [HttpPost]
        [Route("query")]
        public HttpResponseMessage ExecuteQuery([FromBody] ChatbotQueryRequest request)
        {
            Stopwatch timer = Stopwatch.StartNew();

            Trace.WriteLine($"[ChatbotController] Received API query request at {DateTime.UtcNow:O}");

            if (request == null || string.IsNullOrWhiteSpace(request.Query))
            {
                Trace.WriteLine("[ChatbotController] Request validation failed: SQL query is null or empty.");
                var response = ChatbotQueryResponse.ErrorResult("SQL query is required.");
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }

            ValidationResult validationResult = SqlQueryValidator.Validate(request.Query);
            if (!validationResult.IsValid)
            {
                Trace.WriteLine($"[ChatbotController] Query validation failure: {validationResult.ErrorMessage}");
                var response = ChatbotQueryResponse.ErrorResult(validationResult.ErrorMessage);
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }

            try
            {
                var dataTable = _sqlQueryService.ExecuteQuery(request.Query);
                var formattedData = JsonHelper.DataTableToList(dataTable);

                timer.Stop();
                Trace.WriteLine($"[ChatbotController] Query executed successfully in {timer.ElapsedMilliseconds} ms. Returned {formattedData.Count} rows.");

                var response = ChatbotQueryResponse.SuccessResult(formattedData, "Query executed successfully.");
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                timer.Stop();
                // Log internal exception details securely (do not return raw DB errors / credentials to caller)
                Trace.TraceError($"[ChatbotController] Query execution error after {timer.ElapsedMilliseconds} ms: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");

                var response = ChatbotQueryResponse.ErrorResult("Unable to execute the SQL query.");
                return Request.CreateResponse(HttpStatusCode.InternalServerError, response);
            }
        }
    }
}
