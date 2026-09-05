using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BNet.Cafe.Server.Databases
{
    internal class DBMigration
    {
        public class DBColumns
        {
            public enum type
            {
                VARCHAR,
                TEXT
            }

            public string ColumnName { get; set; }
            public type Type { get; set; }
            public int Length { get; set; }
            public string Default { get; set; } = "";
        }



        public async void Insert(string query)
        {
            DBContext dBContext = new DBContext();
            dBContext.SqlExecuteAsync($"{query}");
        }


        public async void Create_Table(string table, List<DBColumns> columns)
        {
            try
            {
                string databases = DBContext.DBSchema;

                //Console.WriteLine($"PLEASE WAIT INITIALIZING TABLE {table}");

                // AUTO COLUMNS
                columns.Insert(0, new DBColumns
                {
                    ColumnName = "DBId"
                });

                columns.Add(new DBMigration.DBColumns
                {
                    ColumnName = "DBDateCreated",
                    Length = 15,
                    Type = DBMigration.DBColumns.type.VARCHAR
                });
                columns.Add(new DBMigration.DBColumns
                {
                    ColumnName = "DBTimeCreated",
                    Length = 15,
                    Type = DBMigration.DBColumns.type.VARCHAR
                });
                columns.Add(new DBMigration.DBColumns
                {
                    ColumnName = "DBIsDeleted",
                    Length = 5,
                    Type = DBMigration.DBColumns.type.VARCHAR,
                    Default = "FALSE"
                });

                DBContext dBContext = new DBContext();
                string database_schema = databases.ToString();
                dBContext.SqlExecuteAsync($"CREATE SCHEMA IF NOT EXISTS {database_schema};");

                string count_table = dBContext.SqlExecuteScalarAsync($"SELECT COUNT(DISTINCT(table_name)) from information_schema.columns where table_schema = '{database_schema}' AND table_name = '{table}'");
                dBContext.SqlExecuteAsync($"ALTER TABLE {database_schema}.{table} DISABLE KEYS");

                if (count_table == "0")
                {
                    dBContext.SqlExecuteAsync($"ALTER TABLE {database_schema}.{table} ROW_FORMAT = Fixed");
                    dBContext.SqlExecuteAsync($"CREATE TABLE {database_schema}.{table} ( `DBId` INT(250) NOT NULL AUTO_INCREMENT , PRIMARY KEY (`DBId`)) ENGINE = InnoDB;");
                    dBContext.SqlExecuteAsync($"ALTER TABLE {database_schema}.{table} ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");
                }

                // ADD or SKIP existing columns
                for (int i = 1; i < columns.Count; i++)
                {
                    string columnName = columns[i].ColumnName;
                    string dataType = columns[i].Type.ToString();
                    int length = columns[i].Length;
                    bool isIndex = columns[i].ColumnName.EndsWith("Id");
                    string AfterColumn = columns[i - 1].ColumnName;
                    string Default = "";
                    if (columns[i].Default != "")
                    {
                        Default = $"DEFAULT '{columns[i].Default}'";
                    }

                    string count_column = dBContext.SqlExecuteScalarAsync($"SELECT COUNT(COLUMN_NAME) FROM information_schema.columns where table_schema = '{database_schema}' AND table_name = '{table}' AND COLUMN_NAME ='{columnName}'");
                    if (count_column == "0")
                    {
                        bool creationSuccess = dBContext.SqlExecuteAsync($"ALTER TABLE {database_schema}.{table} ADD {columnName} {dataType}({length}) NOT NULL {Default} AFTER {AfterColumn}");
                        if (creationSuccess)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine($"Success {database_schema}.{table}.{columnName}");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Failed {database_schema}.{table}.{columnName}");
                        }

                        if (isIndex)
                        {
                            dBContext.SqlExecuteAsync($"ALTER TABLE {database_schema}.{table} ADD INDEX({columnName})");
                        }
                    }
                    else
                    {
                        // ── Column EXISTS — check if type or length changed ──────────────
                        string currentDef = dBContext.SqlExecuteScalarAsync(
                            $"SELECT CONCAT(DATA_TYPE, '|', CHARACTER_MAXIMUM_LENGTH) " +
                            $"FROM information_schema.columns " +
                            $"WHERE table_schema = '{database_schema}' AND table_name = '{table}' " +
                            $"AND COLUMN_NAME = '{columnName}'"
                        );

                        string expectedType = columns[i].Type.ToString().ToLower();   // "varchar" or "text"
                        string expectedLength = columns[i].Length.ToString();

                        // Parse what MySQL currently has
                        string[] parts = currentDef?.Split('|') ?? new[] { "", "" };
                        string currentType = parts[0].ToLower().Trim();
                        string currentLength = parts.Length > 1 ? parts[1].Trim() : "";

                        bool typeChanged = currentType != expectedType;
                        // For TEXT, MySQL reports NULL length — only compare length for VARCHAR
                        bool lengthChanged = expectedType == "varchar" && currentLength != expectedLength;

                        if (typeChanged || lengthChanged)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"Type change detected on {database_schema}.{table}.{columnName}: " +
                                              $"{currentType}({currentLength}) → {expectedType}({expectedLength})");

                            Default = columns[i].Default != "" ? $"DEFAULT '{columns[i].Default}'" : "";

                            bool alterSuccess = dBContext.SqlExecuteAsync(
                                $"ALTER TABLE {database_schema}.{table} " +
                                $"MODIFY COLUMN {columnName} {columns[i].Type}({columns[i].Length}) NOT NULL {Default}"
                            );

                            if (alterSuccess)
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine($"Altered column type: {database_schema}.{table}.{columnName}");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Failed to alter column: {database_schema}.{table}.{columnName}");
                            }
                            Console.ResetColor();
                        }
                    }


                    // ── Runs for ALL columns (new and existing) ──────────────────
                    if (isIndex)
                    {
                        // Ensure index exists — add only if missing
                        string indexCount = dBContext.SqlExecuteScalarAsync(
                            $"SELECT COUNT(*) FROM information_schema.statistics " +
                            $"WHERE table_schema = '{database_schema}' AND table_name = '{table}' " +
                            $"AND COLUMN_NAME = '{columnName}' AND INDEX_NAME != 'PRIMARY'"
                        );
                        if (indexCount == "0")
                        {
                            dBContext.SqlExecuteAsync($"ALTER TABLE {database_schema}.{table} ADD INDEX({columnName})");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine($"Added index on {database_schema}.{table}.{columnName}");
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        // Column does NOT end with "Id" — drop any stale indexes on it
                        DataTable indexTable = dBContext.SqlDataAdapterAsync(
                            $"SELECT DISTINCT INDEX_NAME FROM information_schema.statistics " +
                            $"WHERE table_schema = '{database_schema}' AND table_name = '{table}' " +
                            $"AND COLUMN_NAME = '{columnName}' AND INDEX_NAME != 'PRIMARY'"
                        );

                        foreach (DataRow indexRow in indexTable.Rows)
                        {
                            string indexName = indexRow["INDEX_NAME"].ToString();
                            bool indexDropped = dBContext.SqlExecuteAsync(
                                $"ALTER TABLE {database_schema}.{table} DROP INDEX `{indexName}`"
                            );

                            if (indexDropped)
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine($"Removed index [{indexName}] from {database_schema}.{table}.{columnName}");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Failed to remove index [{indexName}] from {database_schema}.{table}.{columnName}");
                            }
                            Console.ResetColor();
                        }
                    }
                }

                // ==============================
                // DROP COLUMNS REMOVED FROM CODE
                // ==============================

                // Build a HashSet of column names defined in code (case-insensitive)
                HashSet<string> definedColumns = new HashSet<string>(
                    columns.Select(c => c.ColumnName),
                    StringComparer.OrdinalIgnoreCase
                );

                // Fetch all columns currently in MySQL for this table
                DataTable existingColumnsTable = dBContext.SqlDataAdapterAsync(
                    $"SELECT COLUMN_NAME FROM information_schema.columns WHERE table_schema = '{database_schema}' AND table_name = '{table}'"
                );

                foreach (DataRow row in existingColumnsTable.Rows)
                {
                    string existingColumn = row["COLUMN_NAME"].ToString();

                    // Skip DBId — never drop the primary key
                    if (existingColumn.Equals("DBId", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!definedColumns.Contains(existingColumn))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Dropping removed column: {database_schema}.{table}.{existingColumn}");

                        // Drop any indexes on this column first
                        DataTable indexTable = dBContext.SqlDataAdapterAsync(
                            $"SELECT DISTINCT INDEX_NAME FROM information_schema.statistics " +
                            $"WHERE table_schema = '{database_schema}' AND table_name = '{table}' " +
                            $"AND COLUMN_NAME = '{existingColumn}' AND INDEX_NAME != 'PRIMARY'"
                        );

                        foreach (DataRow indexRow in indexTable.Rows)
                        {
                            string indexName = indexRow["INDEX_NAME"].ToString();
                            bool indexDropped = dBContext.SqlExecuteAsync(
                                $"ALTER TABLE {database_schema}.{table} DROP INDEX `{indexName}`"
                            );

                            if (indexDropped)
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine($"Dropped index [{indexName}] on {existingColumn}");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Failed to drop index [{indexName}] on {existingColumn}");
                            }
                        }

                        // Now drop the column
                        bool dropSuccess = dBContext.SqlExecuteAsync(
                            $"ALTER TABLE {database_schema}.{table} DROP COLUMN `{existingColumn}`"
                        );

                        if (dropSuccess)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine($"Dropped column: {database_schema}.{table}.{existingColumn}");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Failed to drop column: {database_schema}.{table}.{existingColumn}");
                        }

                        Console.ResetColor();
                    }
                }

                // ==============================

                dBContext.SqlExecuteAsync($"ALTER TABLE {database_schema}.{table} ENABLE KEYS");
                dBContext.SqlExecuteAsync($"ALTER TABLE {database_schema}.{table} CHANGE `DBId` `DBId` INT(250) NOT NULL AUTO_INCREMENT");

                Task.Delay(2000);
                Console.WriteLine($"DONE PROCESSING TABLE {table}");
            }
            catch (Exception er)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in Create_Table: {er.Message}");
                Console.ResetColor();
            }
        }

    }
}