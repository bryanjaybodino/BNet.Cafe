using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BNet.Cafe.Server.Databases.Tables
{
    public class chat_messages
    {
        public void create()
        {
            List<DBMigration.DBColumns> list = new List<DBMigration.DBColumns>();

            // DBId is a primary key and auto-incremented
            // DBIsDeleted is string boolean, default "FALSE"
            // DBDateCreated is a timestamp yyyy-MM-dd
            // DBTimeCreated is a timestamp HH:mm:ss

            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBMessage",
                Length = 1000,
                Type = DBMigration.DBColumns.type.VARCHAR
            });
            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBComputerName",
                Length = 50,
                Type = DBMigration.DBColumns.type.VARCHAR
            });
            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBUserId",
                Length = 50,
                Type = DBMigration.DBColumns.type.VARCHAR
            });

            DBMigration tableCreation = new DBMigration();
            tableCreation.Create_Table(GetType().Name, list);
        }
    }
}