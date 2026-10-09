using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BNet.Cafe.Server.Databases.Tables
{
    public class client_config
    {
        public void create()
        {
            List<DBMigration.DBColumns> list = new List<DBMigration.DBColumns>();

            // DBId is a primary key and auto-incremented
            // DBIsDeleted is string boolean, default value is "FALSE"
            // DBDateCreated is a timestamp yyyy-MM-dd
            // DBTimeCreated is a timestamp HH:mm:ss

            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBAccountCreationAllowed",
                Length = 10,
                Type = DBMigration.DBColumns.type.VARCHAR
            });
            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBAutoShutDownInterval",
                Length = 10,
                Type = DBMigration.DBColumns.type.VARCHAR
            });
            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBDesktopSlideShow",
                Length = 10,
                Type = DBMigration.DBColumns.type.VARCHAR
            });
            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBResetShutdownCountdown",
                Length = 10,
                Type = DBMigration.DBColumns.type.VARCHAR
            });

            DBMigration tableCreation = new DBMigration();
            tableCreation.Create_Table(GetType().Name, list);
        }
    }
}