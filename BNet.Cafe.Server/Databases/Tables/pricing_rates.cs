using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BNet.Cafe.Server.Databases.Tables
{
    public class pricing_rates
    {
        public void create()
        {
            List<DBMigration.DBColumns> list = new List<DBMigration.DBColumns>();

            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBCustomerType",
                Length = 20,
                Type = DBMigration.DBColumns.type.VARCHAR
            });

            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBMinutes",
                Length = 11,
                Type = DBMigration.DBColumns.type.VARCHAR
            });

            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBPrice",
                Length = 10,
                Type = DBMigration.DBColumns.type.VARCHAR
            });

            DBMigration tableCreation = new DBMigration();
            tableCreation.Create_Table(GetType().Name, list);
        }
    }
}