using System;
using System.Text.RegularExpressions;

namespace ChatbotSqlApi.Helpers
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }

        public static ValidationResult Success()
        {
            return new ValidationResult { IsValid = true, ErrorMessage = null };
        }

        public static ValidationResult Failure(string message)
        {
            return new ValidationResult { IsValid = false, ErrorMessage = message };
        }
    }

    public static class SqlQueryValidator
    {
        // Forbidden keywords that modify data or database schema or execute procedural code
        private static readonly string[] ForbiddenKeywords = new string[]
        {
            "INSERT", "UPDATE", "DELETE", "DROP", "ALTER", "TRUNCATE", "CREATE",
            "EXEC", "EXECUTE", "MERGE", "GRANT", "REVOKE", "DENY", "RENAME",
            "SHUTDOWN", "BACKUP", "RESTORE", "XP_", "SP_"
        };

        /// <summary>
        /// Validates that the input query is a safe, read-only SELECT statement.
        /// </summary>
        public static ValidationResult Validate(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return ValidationResult.Failure("SQL query is required.");
            }

            string trimmedQuery = query.Trim();

            // Strip single-line (--...) and multi-line (/*...*/) comments for keyword checking
            string sanitizedQuery = RemoveComments(trimmedQuery).Trim();

            if (string.IsNullOrWhiteSpace(sanitizedQuery))
            {
                return ValidationResult.Failure("SQL query is required.");
            }

            // Reject multiple SQL statements (semicolon separating non-empty statements)
            string[] statements = sanitizedQuery.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (statements.Length > 1)
            {
                return ValidationResult.Failure("Multiple SQL statements are not allowed.");
            }

            // Ensure the sanitized query starts with SELECT or WITH (for CTEs leading to SELECT)
            if (!sanitizedQuery.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) &&
                !sanitizedQuery.StartsWith("WITH", StringComparison.OrdinalIgnoreCase))
            {
                return ValidationResult.Failure("Only SELECT queries are allowed.");
            }

            // Check for SELECT INTO (creating new tables)
            if (Regex.IsMatch(sanitizedQuery, @"\bINTO\b", RegexOptions.IgnoreCase))
            {
                return ValidationResult.Failure("SELECT INTO queries are not allowed.");
            }

            // Check forbidden DDL/DML/DCL keywords using word boundaries
            foreach (string keyword in ForbiddenKeywords)
            {
                string pattern = $@"\b{keyword}\b";
                if (Regex.IsMatch(sanitizedQuery, pattern, RegexOptions.IgnoreCase))
                {
                    return ValidationResult.Failure("Only SELECT queries are allowed.");
                }
            }

            return ValidationResult.Success();
        }

        private static string RemoveComments(string sql)
        {
            // Remove multi-line comments /* ... */
            string blockCommentsPattern = @"/\*.*?\*/";
            string noBlockComments = Regex.Replace(sql, blockCommentsPattern, " ", RegexOptions.Singleline);

            // Remove single-line comments -- ...
            string lineCommentsPattern = @"--.*$";
            string noComments = Regex.Replace(noBlockComments, lineCommentsPattern, " ", RegexOptions.Multiline);

            return noComments;
        }
    }
}
