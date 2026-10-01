using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BNet.Cafe.Server.Databases.Tables
{
    public class inventory_transactions
    {
        public void create()
        {
            List<DBMigration.DBColumns> list = new List<DBMigration.DBColumns>();
            
            //DBId is a primary key and auto-incremented, so we don't need to define it here.
            //DBIsDeleted is string boolean, so we don't need to define it here. defaut value is "FALSE"
            //DBDateCreated is a timestamp yyyy-MM-dd
            //DBTimeCreated is a timestamp HH:mm:ss

            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBItemId",
                Length = 10,
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
                ColumnName = "DBTransactionType", // IN, OUT, ADJUSTMENT
                Length = 15,
                Type = DBMigration.DBColumns.type.VARCHAR
            });
            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBQuantity",
                Length = 10,
                Type = DBMigration.DBColumns.type.VARCHAR
            });

            DBMigration tableCreation = new DBMigration();
            tableCreation.Create_Table(GetType().Name, list);
        }
    }
}