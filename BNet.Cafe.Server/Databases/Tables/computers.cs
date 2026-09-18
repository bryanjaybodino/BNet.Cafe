using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BNet.Cafe.Server.Databases.Tables
{
    public class computers
    {
        public void create()
        {
            List<DBMigration.DBColumns> list = new List<DBMigration.DBColumns>();
            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBComputerName",
                Length = 100,
                Type = DBMigration.DBColumns.type.VARCHAR
            });

            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBPosX",
                Length = 100,
                Type = DBMigration.DBColumns.type.VARCHAR
            });
            list.Add(new DBMigration.DBColumns
            {
                ColumnName = "DBPosY",
                Length = 100,
                Type = DBMigration.DBColumns.type.VARCHAR
            });

            DBMigration tableCreation = new DBMigration();
            tableCreation.Create_Table(GetType().Name, list);
        }
    }
}