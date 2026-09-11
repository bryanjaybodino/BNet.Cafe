using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BNet.Cafe.Server.Databases.Tables
{
    public class users
    {
        public void create()
        {
            List<DBMigration.DBColumns> list = new List<DBMigration.DBColumns>();
            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBEmail",
                Length = 50,
                Type = DBMigration.DBColumns.type.VARCHAR
            });
            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBName",
                Length = 50,
                Type = DBMigration.DBColumns.type.VARCHAR
            });
            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBRole",
                Length = 5,
                Type = DBMigration.DBColumns.type.VARCHAR
            });
            DBMigration tableCreation = new DBMigration();
            tableCreation.Create_Table(GetType().Name, list);
        }
    }
}