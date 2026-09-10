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
                Length = 25,
                Type = DBMigration.DBColumns.type.VARCHAR
            });
            DBMigration tableCreation = new DBMigration();
            tableCreation.Create_Table(GetType().Name, list);
        }
    }
}