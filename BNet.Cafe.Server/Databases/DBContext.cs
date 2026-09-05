using MySqlConnector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace BNet.Cafe.Server.Databases
{
    public class DBContext
    {
        public long LastInsertedId = 0;
        public static string DBSchema = "BNetCafe";
        private MySqlConnection SetConnection()
        {
            string hostname = "localhost";
            string port = "3306";
            string user = "root";
            string password = "";
            string database = ("" == DBSchema) ? "" : $"database={DBSchema}";

            string connectionString = $"server={hostname};port={port};user id={user};password={password};{database};" +
                "Allow User Variables=true;" +
                "Pooling=true;" +
                "MinimumPoolSize=5;" +
                "MaximumPoolSize=100;" +
                "ConnectionIdleTimeout=3000;";

            MySqlConnection mySQLCon = new MySqlConnection(connectionString);
            if (mySQLCon.State == System.Data.ConnectionState.Open)
                mySQLCon.Close();
            mySQLCon.Open();
            return mySQLCon;
        }

        // Uses the SAME open connection — no extra round-trip
        private void GetLastIdAsync(MySqlConnection con, string sql)
        {
            bool hasInsert = sql.IndexOf("INSERT", StringComparison.OrdinalIgnoreCase) >= 0;
            if (!hasInsert)
            {
                LastInsertedId = 0;
                return;
            }

            // Extract table name from the FIRST INSERT INTO
            var match = Regex.Match(sql, @"\bINSERT\s+INTO\s+[`\[""]?(\w+)[`\]""]?", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                LastInsertedId = 0;
                return;
            }

            string tableName = match.Groups[1].Value;

            // Get the database name from the connection
            string schemaName = con.Database;

            // Query AUTO_INCREMENT - 1 because AUTO_INCREMENT points to the NEXT id
            string autoIncrementSql = $@"
            SELECT AUTO_INCREMENT - 1 
            FROM information_schema.TABLES 
            WHERE TABLE_SCHEMA = '{schemaName}' 
            AND TABLE_NAME = '{tableName}';";

            var cmd = new MySqlCommand(autoIncrementSql, con);
            var result = cmd.ExecuteScalar();
            LastInsertedId = result != null ? Convert.ToInt64(result) : 0;
        }

        public bool SqlExecuteAsync(string sql)
        {
            var con = SetConnection();
            try
            {
                MySqlCommand mySqlCommand = new MySqlCommand(sql, con);
                mySqlCommand.ExecuteNonQuery();
                GetLastIdAsync(con, sql);
                con.Close();
                return true;
            }
            catch
            {
                LastInsertedId = 0;
                return false;
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
        }

        public bool SqlExecuteNonQueryAsync(string sql, Dictionary<string, object> parameters)
        {
            var con = SetConnection();
            try
            {
                using (MySqlCommand mySqlCommand = new MySqlCommand(sql, con))
                {
                    foreach (var param in parameters)
                    {
                        mySqlCommand.Parameters.AddWithValue(param.Key, param.Value);
                    }
                    mySqlCommand.ExecuteNonQuery();
                    GetLastIdAsync(con, sql);
                }
                return true;
            }
            catch
            {
                LastInsertedId = 0;
                return false;
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
        }

        public string SqlExecuteScalarAsync(string sql)
        {
            var con = SetConnection();
            try
            {
                MySqlCommand mySqlCommand = new MySqlCommand(sql, con);
                var result = mySqlCommand.ExecuteScalar();
                con.Close();
                return result != null ? result.ToString() : "0";
            }
            catch
            {
                LastInsertedId = 0;
                return "0";
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
        }

        public Dictionary<string, object> SqlExecuteReaderAsync(string sql)
        {
            var con = SetConnection();
            try
            {
                MySqlCommand mySqlCommand = new MySqlCommand(sql, con);
                Dictionary<string, object> result = null;

                using (var reader = mySqlCommand.ExecuteReader())
                {
                    do
                    {
                        // Only read from result sets that have columns (SELECT statements)
                        if (reader.FieldCount > 0 && result == null)
                        {
                            if (reader.Read())
                            {
                                result = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    result[reader.GetName(i)] = reader[i];
                                }
                            }
                        }
                    }
                    while (reader.NextResult());
                } // reader closed here — connection free for LAST_INSERT_ID()

                GetLastIdAsync(con, sql);
                return result;
            }
            catch
            {
                LastInsertedId = 0;
                return null;
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
        }

        public DataTable SqlDataAdapterAsync(string sql)
        {
            DataTable dataTable = new DataTable();
            var con = SetConnection();
            try
            {
                MySqlCommand mySqlCommand = new MySqlCommand(sql, con);
                using (var reader = mySqlCommand.ExecuteReader())
                {
                    dataTable.Load(reader);
                }
                con.Close();
                LastInsertedId = 0;
                return dataTable;
            }
            catch
            {
                LastInsertedId = 0;
                return dataTable;
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
        }

        private string FirstCharacterToLower(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            return char.ToLower(input[0]) + input.Substring(1);
        }

        public List<Dictionary<string, object>> DataTableToJson(DataTable table)
        {
            var list = new List<Dictionary<string, object>>();
            var seenColumns = new HashSet<string>();
            var columnMap = new List<(DataColumn col, string cleanName)>();

            foreach (DataColumn col in table.Columns)
            {
                string cleanName = FirstCharacterToLower(col.ColumnName.Replace("DB", ""));
                string baseName = Regex.Replace(cleanName, @"\d+$", "");
                if (seenColumns.Contains(baseName))
                    continue;
                seenColumns.Add(baseName);
                columnMap.Add((col, baseName));
            }

            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (var (col, cleanName) in columnMap)
                {
                    string raw = row[col].ToString().Trim();
                    dict[cleanName] = TryParseJson(raw, out var parsed) ? parsed : raw;
                }
                list.Add(dict);
            }

            return list;
        }

        private bool TryParseJson(string value, out object result)
        {
            result = null;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            // quick check so we don't waste time trying to parse normal strings/numbers
            var trimmed = value.Trim();
            bool looksLikeJson = (trimmed.StartsWith("{") && trimmed.EndsWith("}")) ||
                                 (trimmed.StartsWith("[") && trimmed.EndsWith("]"));

            if (!looksLikeJson)
                return false;

            try
            {
                var token = JToken.Parse(trimmed);
                result = token; // JObject, JArray, etc.
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}