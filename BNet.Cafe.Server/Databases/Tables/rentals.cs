using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BNet.Cafe.Server.Databases.Tables
{
    public class rentals
    {
        public void create()
        {
            List<DBMigration.DBColumns> list = new List<DBMigration.DBColumns>();
            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBComputerId",
                Length = 5,
                Type = DBMigration.DBColumns.type.VARCHAR
            });
            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBUserId",
                Length = 5,
                Type = DBMigration.DBColumns.type.VARCHAR
            });
            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBDuration",
                Length = 5,
                Type = DBMigration.DBColumns.type.VARCHAR
            });
            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBAmount",
                Length = 5,
                Type = DBMigration.DBColumns.type.VARCHAR
            });

            DBMigration tableCreation = new DBMigration();
            tableCreation.Create_Table(GetType().Name, list);
        }
    }
}