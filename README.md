# ChatbotSqlApi - ASP.NET MVC 5 & Web API 2 Backend Service

## 1. Project Purpose
`ChatbotSqlApi` is a lightweight, secure C# ASP.NET MVC 5 and Web API 2 backend service designed to serve as an intermediary between an external Python NLP/Chatbot application and SQL Server.

The external Python application is responsible for user interaction, natural language processing (NLP), SQL query generation, and converting raw SQL JSON results back into natural language answers.

This C# project is **strictly responsible** for:
1. Receiving SQL queries from the external Python service.
2. Validating incoming requests and ensuring queries are strictly read-only (`SELECT`).
3. Executing queries against SQL Server via ADO.NET.
4. Structuring and converting database results into a consistent JSON response.
5. Returning the JSON payload back to the Python application.

---

## 2. Architecture & Execution Flow

```text
External Python Application (NLP / UI)
                 │
                 │ 1. POST /api/chatbot/query { "query": "SELECT ..." }
                 ▼
          ChatbotController (ASP.NET Web API 2)
                 │
                 │ 2. Validate SQL (SqlQueryValidator)
                 ▼
         ISqlQueryService / SqlQueryService (ADO.NET)
                 │
                 │ 3. Execute query
                 ▼
             SQL Server
                 │
                 │ 4. DataTable results
                 ▼
         JsonHelper (Format to JSON array/dictionaries)
                 │
                 │ 5. Return JSON Response (ChatbotQueryResponse)
                 ▼
External Python Application
```

---

## 3. Solution Directory Structure

```text
ChatbotSqlApi/
│
├── App_Start/
│   ├── WebApiConfig.cs         # Web API routing and JSON serializer configuration
│   └── RouteConfig.cs          # MVC routing table definition
│
├── Controllers/
│   ├── ChatbotController.cs    # Web API controller handling POST /api/chatbot/query
│   └── HomeController.cs       # MVC controller serving test UI
│
├── Models/
│   ├── ChatbotQueryRequest.cs  # Incoming request model ({ "query": "..." })
│   ├── ChatbotQueryResponse.cs # Typed API response model
│   └── ApiResponse.cs          # Generic base API response contract
│
├── Services/
│   ├── ISqlQueryService.cs     # Interface for database execution
│   └── SqlQueryService.cs      # ADO.NET implementation (SqlConnection, SqlCommand, SqlDataAdapter)
│
├── Helpers/
│   ├── SqlQueryValidator.cs    # Read-only SELECT validator and security guard
│   └── JsonHelper.cs           # Converts DataTable rows into List<Dictionary<string, object>>
│
├── Views/
│   └── Home/
│       └── Index.cshtml        # Test console UI for executing queries via AJAX
│
├── Content/
│   └── Site.css                # Styling for test console
│
├── Scripts/                    # Client JavaScript libraries
│
├── Global.asax                 # Web application entry point
├── Global.asax.cs              # Web application startup logic
├── Web.config                  # DB connection string & web configuration
└── packages.config             # NuGet package dependencies
```

---

## 4. SQL Server & Web.config Configuration

Configure the SQL Server connection string in `Web.config`.

### Integrated Security (Windows Authentication) Example:
```xml
<connectionStrings>
  <add name="ChatbotDbConnection"
       connectionString="Data Source=YOUR_SERVER;Initial Catalog=YOUR_DATABASE;Integrated Security=True;MultipleActiveResultSets=True;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

### SQL Server Authentication (Username / Password) Example:
```xml
<connectionStrings>
  <add name="ChatbotDbConnection"
       connectionString="Data Source=YOUR_SERVER;Initial Catalog=YOUR_DATABASE;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;MultipleActiveResultSets=True;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

---

## 5. Running the Project

1. Open `ChatbotSqlApi.sln` in Visual Studio 2019 or Visual Studio 2022.
2. Restore NuGet packages.
3. Update `ChatbotDbConnection` in `Web.config` with your SQL Server connection details.
4. Press `F5` or click **Start** in Visual Studio to run using IIS Express.
5. The MVC test page will open automatically at `http://localhost:<port>/`.

---

## 6. API Endpoint Contract

### Request

* **URL:** `/api/chatbot/query`
* **Method:** `POST`
* **Content-Type:** `application/json`

```json
{
    "query": "SELECT COUNT(*) AS TotalOrders FROM Orders"
}
```

### Success Response (`200 OK`)

```json
{
    "success": true,
    "message": "Query executed successfully.",
    "data": [
        {
            "TotalOrders": 125
        }
    ]
}
```

### Multiple Rows Response (`200 OK`)

```json
{
    "success": true,
    "message": "Query executed successfully.",
    "data": [
        {
            "OrderId": 1,
            "CustomerName": "Customer A",
            "OrderDate": "2026-09-01T00:00:00"
        },
        {
            "OrderId": 2,
            "CustomerName": "Customer B",
            "OrderDate": "2026-09-02T00:00:00"
        }
    ]
}
```

---

## 7. Error Responses

### Empty Query (`400 Bad Request`)

```json
{
    "success": false,
    "message": "SQL query is required.",
    "data": null
}
```

### Non-SELECT or Forbidden Query (`400 Bad Request`)

```json
{
    "success": false,
    "message": "Only SELECT queries are allowed.",
    "data": null
}
```

### Database Execution Error (`500 Internal Server Error`)

```json
{
    "success": false,
    "message": "Unable to execute the SQL query.",
    "data": null
}
```

---

## 8. Python Integration Example

Here is a Python code snippet illustrating how an external Python NLP app calls this C# API:

```python
import requests

API_URL = "http://localhost:5000/api/chatbot/query"

def get_total_orders():
    # 1. Python NLP generates SQL query
    generated_sql = "SELECT COUNT(*) AS TotalOrders FROM Orders"

    # 2. Call C# Backend API
    payload = {
        "query": generated_sql
    }
    headers = {
        "Content-Type": "application/json"
    }

    response = requests.post(API_URL, json=payload, headers=headers)

    if response.status_code == 200:
        result = response.json()
        if result.get("success"):
            data = result.get("data")
            total_orders = data[0]["TotalOrders"]
            # 3. Python converts JSON to natural language response
            return f"There are currently {total_orders} total orders."

    return "Sorry, I was unable to retrieve the order information."

# Example Usage
if __name__ == "__main__":
    answer = get_total_orders()
    print("Chatbot Answer:", answer)
```

---

## 9. Security Considerations

1. **Read-Only Validation (`SqlQueryValidator`):** Rejects any non-`SELECT` statement (`INSERT`, `UPDATE`, `DELETE`, `DROP`, `ALTER`, `TRUNCATE`, `CREATE`, `EXEC`, etc.), multiple statements separated by semicolons, and comment tricks.
2. **Credential Safety:** Connection strings are maintained securely in `Web.config` and never exposed to API callers or source files.
3. **Information Disclosure Prevention:** Raw database exception details (e.g., table names, column names, server names) are hidden behind generic error messages (`Unable to execute the SQL query.`) and logged internally.
4. **Command Timeout:** Queries are configured with a reasonable command timeout (30 seconds) to prevent server hangs on heavy dynamic queries.

---

## 10. How to Test Using Postman

1. Open Postman.
2. Set HTTP Method to `POST`.
3. Set Request URL to `http://localhost:<your-port>/api/chatbot/query`.
4. Select **Headers** tab and add `Content-Type: application/json`.
5. Select **Body** tab, select **raw**, choose **JSON**, and paste:
   ```json
   {
       "query": "SELECT COUNT(*) AS TotalOrders FROM Orders"
   }
   ```
6. Click **Send** and inspect the returned JSON response.

---

## 11. How to Test Using the MVC Test Page

1. Open a browser and navigate to `http://localhost:<your-port>/` (or `/Home/Index`).
2. Type or paste your `SELECT` query into the textarea provided.
3. Click `[ Execute Query ]`.
4. View the formatted API JSON response rendered below the button.
