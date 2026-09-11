using BNet.Cafe.Server.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Services.Protocols;

namespace BNet.Cafe.Server.Databases
{
    /// <summary>
    /// Builds and processes SQL scripts from template files by substituting
    /// named parameters and cleaning up unused conditions.
    /// </summary>
    public class DBScriptService
    {
        // ── Constants ────────────────────────────────────────────────────────────

        private const string SafeQuote = "ˈ";   // replacement for single-quote in values
        private const string SqlScriptBaseDir = @"C:\BNet.Cafe\SQL Scripts\";
        private const int FileWriteRetries = 3;
        private const int FileWriteDelayMs = 200;

        // Pre-compiled regex patterns for performance.
        private static readonly Regex UnusedConditionRegex = new Regex(
            @"(AND|OR)\s+[^\n]*(=\s*'\{(\w+)\}'|IN\s*\('\{(\w+)\}'\)|BETWEEN\s*'\{(\w+)\}'\s*AND\s*'\{(\w+)\}')",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex UnresolvedPlaceholderRegex = new Regex(
            @"\{([A-Za-z0-9_-]+)\}",
            RegexOptions.Compiled);

        private static readonly Regex ValuesBlockRegex = new Regex(
            @"VALUES\s*\((.*)\)",
            RegexOptions.Singleline | RegexOptions.Compiled);

        // ── Public Parameter Helpers ──────────────────────────────────────────────

        /// <summary>
        /// Splits a comma-separated date range value and adds Start/End keys,
        /// or a plain key if no valid range is found.
        /// </summary>
        public void AddDateRangeToParameters(Dictionary<string, string> parameters, string key, string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            try
            {
                var parts = value.Split(',');
                if (parts.Length >= 2 &&
                    !string.IsNullOrWhiteSpace(parts[0]) &&
                    !string.IsNullOrWhiteSpace(parts[1]))
                {
                    parameters[key + "Start"] = parts[0] + " 00:00:00";
                    parameters[key + "End"] = parts[1] + " 23:59:29";
                }
                else
                {
                    parameters[key] = value == "," ? string.Empty : value;
                }
            }
            catch { /* malformed input is silently ignored */ }
        }


        /// <summary>
        /// Retrieves timezone-aware server time using the provided timezone string
        /// and populates parameters dictionary with DateToday, TimeToday, and FullDateToday keys.
        /// </summary>
        public Dictionary<string, string> AddServerTimeToParameters(string timeZone, Dictionary<string, string> parameters)
        {
            parameters = parameters ?? new Dictionary<string, string>();
            try
            {
                DateTime dateTime = TimeService.Get();
                // Format datetime in different ways
                string dateCreated = dateTime.ToString("yyyy-MM-dd");
                string timeCreated = dateTime.ToString("HH:mm:ss");
                string fullDateToday = dateTime.ToString("yyyy-MM-dd HH:mm");

                // Add to parameters
                parameters["DateToday"] = dateCreated;
                parameters["TimeToday"] = timeCreated;
                parameters["FullDateToday"] = fullDateToday;
            }
            catch { /* timezone parsing errors are silently ignored */ }

            return parameters;
        }


        /// <summary>
        /// Parses <paramref name="value"/> as a date and adds it in
        /// <c>yyyy-MM-dd</c> format, or an empty string if parsing fails.
        /// </summary>
        public void AddDateDefaultEmptyToParameters(
            Dictionary<string, string> parameters, string key, string value)
        {
            if (value is null) return;

            parameters[key] = DateTime.TryParse(value, out var dt)
                ? dt.ToString("yyyy-MM-dd").ToUpperInvariant()
                : string.Empty;
        }

        /// <summary>
        /// Parses <paramref name="value"/> as a time and adds it in
        /// <c>HH:mm:ss</c> format, or an empty string if parsing fails.
        /// </summary>
        public void AddTimeDefaultEmptyToParameters(
            Dictionary<string, string> parameters, string key, string value)
        {
            if (value is null) return;

            parameters[key] = DateTime.TryParse(value, out var dt)
                ? dt.ToString("HH:mm:ss").ToUpperInvariant()
                : string.Empty;
        }

        /// <summary>
        /// Adds <paramref name="key"/> / <paramref name="value"/> to
        /// <paramref name="parameters"/> only when the key is not already present
        /// and <paramref name="value"/> is not null (empty strings are allowed).
        /// </summary>
        public void AddIfNotNullOrEmpty(
            Dictionary<string, string> parameters, string key, string value)
        {
            if (value == "") return;
            if (parameters is null || string.IsNullOrEmpty(key)) return;
            if (parameters.ContainsKey(key)) return;
            if (value != null)
                parameters[key] = value;
        }

        // ── String Utilities ──────────────────────────────────────────────────────

        /// <summary>
        /// Converts <paramref name="data"/> to an upper-case string with common
        /// escape sequences re-lowercased and double-quotes escaped.
        /// Returns <c>null</c> when <paramref name="data"/> is null.
        /// </summary>
        public string CleanUpToUpper(object data = null)
        {
            if (data is null) return null;
            var text = Convert.ToString(data);
            if (string.IsNullOrEmpty(text)) return text;

            // Collapse multiple spaces/tabs into one, and trim ends
            text = Regex.Replace(text, @"\s+", " ").Trim();

            var cleaned = text.ToUpperInvariant()
                              .Replace("\\N", "\\n")
                              .Replace("\\R", "\\r")
                              .Replace("\\T", "\\t")
                              .Replace("\\F", "\\f")
                              .Replace("\\B", "\\b")
                              .Replace("\"", "\\\"");
            return cleaned;
        }

        public string CleanUpToString(object data = null)
        {
            if (data is null) return null;
            var text = Convert.ToString(data);
            if (string.IsNullOrEmpty(text)) return text;

            // Collapse multiple spaces/tabs into one, and trim ends
            text = Regex.Replace(text, @"\s+", " ").Trim();

            var cleaned = text.Replace("\\N", "\\n")
                              .Replace("\\R", "\\r")
                              .Replace("\\T", "\\t")
                              .Replace("\\F", "\\f")
                              .Replace("\\B", "\\b")
                              .Replace("\"", "\\\"");
            return cleaned;
        }




        /// <summary>
        /// Compares two strings after normalising curly apostrophes to a safe
        /// placeholder, avoiding encoding-sensitive mismatches.
        /// </summary>
        public bool IsEqual(string a, string b) =>
            a.Replace("'", SafeQuote) == b.Replace("'", SafeQuote);

        // ── SQL Script Building ───────────────────────────────────────────────────

        /// <summary>
        /// Loads the SQL template at <paramref name="templatePath"/>, substitutes
        /// all known parameters, removes unresolved conditions, and writes the
        /// final SQL to the debug-output directory before returning it.
        /// </summary>
        public string Scripts(Dictionary<string, string> parameters, string templatePath)
        {
            InjectDatabaseSchemaDefaults(parameters);

            string sql = File.ReadAllText(templatePath);

            sql = RemoveUnusedConditions(sql, parameters);
            sql = ApplyLimitClause(sql, parameters);
            sql = SubstitutePlaceholders(sql, parameters);
            sql = RemoveLinesWithUnresolvedPlaceholders(sql, parameters);
            sql = CleanUpSql(sql);

            WriteDebugScript(sql, templatePath, parameters);

            return sql;
        }

        // ── Private: Schema Defaults ──────────────────────────────────────────────

        private static void InjectDatabaseSchemaDefaults(Dictionary<string, string> parameters)
        {
            TryAdd(parameters, "accounts", DBContext.DBSchema);


            void TryAdd(Dictionary<string, string> p, string k, string v)
            {
                if (!p.ContainsKey(k)) p[k] = v;
            }
        }

        // ── Private: SQL Transformation Pipeline ─────────────────────────────────

        /// <summary>
        /// Removes AND/OR conditions whose placeholder values are absent from
        /// <paramref name="parameters"/>.
        /// </summary>
        private static string RemoveUnusedConditions(string sql, Dictionary<string, string> parameters) =>
            UnusedConditionRegex.Replace(sql, m =>
            {
                var keys = new[]
                {
                    m.Groups[3].Value, m.Groups[4].Value,
                    m.Groups[5].Value, m.Groups[6].Value
                }.Where(g => !string.IsNullOrEmpty(g));

                return keys.Any(k => !parameters.ContainsKey(k)) ? string.Empty : m.Value;
            });

        private static string ApplyLimitClause(string sql, Dictionary<string, string> parameters)
        {
            string limitValue = parameters.TryGetValue("LIMIT", out var lv) && !string.IsNullOrWhiteSpace(lv)
                ? "LIMIT " + lv
                : string.Empty;

            return sql.Replace("{LIMIT}", limitValue);
        }

        /// <summary>
        /// Substitutes every parameter placeholder, handling IN-list, multi-row
        /// VALUES, and plain scalar replacements.
        /// </summary>
        private static string SubstitutePlaceholders(string sql, Dictionary<string, string> parameters)
        {
            foreach (var kv in parameters)
            {
                if (kv.Key == "LIMIT") continue;

                string inPattern = @"IN\s*\('\{" + kv.Key + @"\}'\)";
                string valuesPattern = @"VALUES\s*\{" + kv.Key + @"\}";

                if (Regex.IsMatch(sql, inPattern, RegexOptions.IgnoreCase))
                {
                    var items = BuildInList(kv.Value);
                    sql = Regex.Replace(sql, inPattern, $"IN ({string.Join(",", items)})", RegexOptions.IgnoreCase);
                    sql = sql.Replace("{" + kv.Key + "}", kv.Value.Replace("'", SafeQuote));
                }
                else if (Regex.IsMatch(sql, valuesPattern, RegexOptions.IgnoreCase))
                {
                    sql = Regex.Replace(sql, valuesPattern,
                        $"VALUES \n{string.Join(",", SplitValueRows(kv.Value))}", RegexOptions.IgnoreCase);
                    sql = CleanUpInsertValues(sql);
                    sql = NormalizeSafeQuotes(sql);
                }
                else
                {
                    sql = sql.Replace("{" + kv.Key + "}", kv.Value.Replace("'", SafeQuote));
                }
            }

            return sql;
        }

        /// <summary>
        /// Drops any line that still contains an unresolved <c>{placeholder}</c>
        /// after all substitutions have been applied.
        /// </summary>
        private static string RemoveLinesWithUnresolvedPlaceholders(
            string sql, Dictionary<string, string> parameters)
        {
            var lines = sql.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            var kept = lines.Where(line =>
            {
                var missing = UnresolvedPlaceholderRegex
                    .Matches(line)
                    .Cast<Match>()
                    .Select(m => m.Groups[1].Value)
                    .Where(k => !parameters.ContainsKey(k))
                    .ToList();

                return !missing.Any();
            });

            return string.Join(Environment.NewLine, kept);
        }

        private static string CleanUpSql(string sql)
        {
            sql = Regex.Replace(sql, @"\(\s*\)", string.Empty);
            sql = Regex.Replace(sql, @"\(\s*(AND|OR)\s*\)", string.Empty);
            sql = Regex.Replace(sql, @"\s+(AND|OR)\s*(?=\))", string.Empty);
            sql = sql.Replace("ROW_NUMBER", "ROW_NUMBER()"); // preserve special function token
            sql = sql.Replace("CURDATE", "CURDATE()"); // preserve special function token
            return sql;
        }

        // ── Private: Values / IN-List Helpers ─────────────────────────────────────

        private static IEnumerable<string> BuildInList(string value) =>
            value.Replace("'", SafeQuote)
                 .Split(',')
                 .Select(v => v.Trim().Trim('\'').Trim(SafeQuote[0]))
                 .Select(v => $"'{v}'");

        private static List<string> SplitValueRows(string input) =>
            Regex.Split(input, @",(?![^']*')")
                 .Select(p => p.Trim())
                 .ToList();

        /// <summary>
        /// Re-parses the VALUES block to escape embedded single quotes,
        /// producing safe SQL literals.
        /// </summary>
        private static string CleanUpInsertValues(string sql)
        {
            var match = ValuesBlockRegex.Match(sql);
            if (!match.Success) return sql;

            string content = match.Groups[1].Value;
            var values = ParseQuotedValues(content);
            var escaped = values.Select(v => $"'{v.Replace("'", SafeQuote)}'");
            string replaced = $"VALUES ({string.Join(",", escaped)})";

            return ValuesBlockRegex.Replace(sql, replaced);
        }

        /// <summary>
        /// Walks the VALUES content character-by-character and extracts each
        /// single-quoted token, tolerating embedded quotes.
        /// </summary>
        private static List<string> ParseQuotedValues(string content)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool insideQuote = false;

            for (int i = 0; i < content.Length; i++)
            {
                char c = content[i];

                if (c != '\'')
                {
                    if (insideQuote) current.Append(c);
                    continue;
                }

                if (!insideQuote)
                {
                    insideQuote = true;
                    continue;
                }

                bool isLast = i + 1 >= content.Length;
                bool nextIsComma = !isLast && content[i + 1] == ',';

                if (isLast || nextIsComma)
                {
                    insideQuote = false;
                    result.Add(current.ToString());
                    current.Clear();
                    if (nextIsComma) i++; // skip comma
                }
                else
                {
                    current.Append('\''); // embedded quote inside value
                }
            }

            return result;
        }

        /// <summary>
        /// Replaces all <c>SafeQuote</c> artefacts left after VALUES substitution
        /// with the correct SQL row-tuple punctuation.
        /// </summary>
        private static string NormalizeSafeQuotes(string sql)
        {
            // Row boundary: ˈ)','(ˈ → '),('
            sql = sql.Replace($"{SafeQuote})','({SafeQuote}", "'),('")
                     .Replace($"{SafeQuote})' , '({SafeQuote}", "'),('")
                     .Replace($"{SafeQuote})', '({SafeQuote}", "'),('")
                     .Replace($"{SafeQuote})' ,'({SafeQuote}", "'),('");

            // Opening paren: '(ˈ → ('
            sql = sql.Replace($"'({SafeQuote}", "('")
                     .Replace($"' ({SafeQuote}", "('")
                     .Replace($"'( {SafeQuote}", "('")
                     .Replace($"' ( {SafeQuote}", "('");

            // Closing paren: ˈ)' → ')'
            sql = sql.Replace($"{SafeQuote})'", "')")
                     .Replace($"{SafeQuote} )'", "')")
                     .Replace($"{SafeQuote} ) '", "')");

            // Trailing comma: ˈ), → '),
            sql = sql.Replace($"{SafeQuote}),", "'),")
                     .Replace($"{SafeQuote} ),", "'),")
                     .Replace($"{SafeQuote} ) ,", "'),");

            // Leading comma+paren: ,(ˈ → ,('
            sql = sql.Replace($",({SafeQuote}", ",('")
                     .Replace($", ({SafeQuote}", ",('")
                     .Replace($",( {SafeQuote}", ",('")
                     .Replace($", ( {SafeQuote}", ",('");

            // Separate adjacent row tuples onto their own line
            sql = sql.Replace("),(", "),\r\n(");

            return sql;
        }

        // ── Private: File Output ──────────────────────────────────────────────────

        private static void WriteDebugScript(
            string sql, string templatePath, Dictionary<string, string> parameters)
        {
            string fileName = Path.GetFileName(templatePath);
            string lastFolder = Path.GetFileName(Path.GetDirectoryName(templatePath)) ?? string.Empty;
            string dbPrefix = DBContext.DBSchema;
            string directory = Path.Combine(SqlScriptBaseDir, dbPrefix, lastFolder);
            Directory.CreateDirectory(directory);

            SafeWriteAllText(Path.Combine(directory, fileName), sql);
        }

        private static void SafeWriteAllText(string path, string content)
        {
            for (int attempt = 0; attempt < FileWriteRetries; attempt++)
            {
                try
                {
                    using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.Write(content);
                        sw.Flush(); // Ensures all buffered content hits the disk stream
                    }
                    return; // success
                }
                catch (IOException) when (attempt < FileWriteRetries - 1)
                {
                    Thread.Sleep(FileWriteDelayMs);
                }
            }
        }
    }
}