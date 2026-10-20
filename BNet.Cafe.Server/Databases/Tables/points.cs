using System;
using System.Collections.Generic;

namespace BNet.Cafe.Server.Databases.Tables
{
    public class points
    {
        public void create()
        {
            List<DBMigration.DBColumns> list = new List<DBMigration.DBColumns>();
            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBUserId",
                Length = 5,
                Type = DBMigration.DBColumns.type.VARCHAR
            });
            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBPoints",
                Length = 10,
                Type = DBMigration.DBColumns.type.VARCHAR
            });
            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBDescription",
                Length = 1000,
                Type = DBMigration.DBColumns.type.VARCHAR
            });
            DBMigration tableCreation = new DBMigration();
            tableCreation.Create_Table(GetType().Name, list);
        }
    }
}