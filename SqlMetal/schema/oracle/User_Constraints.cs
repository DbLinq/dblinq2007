using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OracleClient;

namespace MysqlMetal.schema.oracle
{
    public class User_Constraints_Row
    {
        public string constraint_name;
        public string table_name;
        public string column_name;
        public string constraint_type;
        public string R_constraint_name;
    }

    public class User_Constraints_Sql
    {
        User_Constraints_Row fromRow(OracleDataReader rdr)
        {
            User_Constraints_Row t = new User_Constraints_Row();
            int field = 0;
            t.constraint_name  = rdr.GetString(field++);
            t.table_name    = rdr.GetString(field++);
            t.column_name   = rdr.GetString(field++);
            t.constraint_type = rdr.GetString(field++);
            t.R_constraint_name = rdr.GetNString(field++);
            return t;
        }

        public List<User_Constraints_Row> getConstraints1(OracleConnection conn, string db)
        {
            string sql = @"
SELECT UCC.constraint_name, UCC.table_name, UCC.column_name, UC.constraint_type, UC.R_constraint_name 
FROM user_cons_columns UCC, user_constraints UC
WHERE UCC.constraint_name=UC.constraint_name
AND UCC.table_name=UC.table_name
AND UCC.TABLE_NAME NOT LIKE '%$%' AND UCC.TABLE_NAME NOT LIKE 'LOGMNR%' AND UCC.TABLE_NAME NOT IN ('HELP','SQLPLUS_PRODUCT_PROFILE')
AND UC.CONSTRAINT_TYPE!='C'";

            using(OracleCommand cmd = new OracleCommand(sql, conn))
            {
                //cmd.Parameters.Add("?db", db);
                using(OracleDataReader rdr = cmd.ExecuteReader())
                {
                    List<User_Constraints_Row> list = new List<User_Constraints_Row>();
                    while(rdr.Read())
                    {
                        list.Add( fromRow(rdr) );
                    }
                    return list;
                }
            }

        }

        public List<User_Constraints_Row> getConstraints2(OracleConnection conn, string db)
        {
            string sql = @"
SELECT UCC.constraint_name, UCC.table_name, UCC.column_name, UC.constraint_type 
FROM user_cons_columns UCC, user_constraints UC
WHERE UCC.constraint_name=UC.constraint_name
AND UCC.table_name=UC.table_name
AND UCC.TABLE_NAME NOT LIKE '%$%' AND UCC.TABLE_NAME NOT LIKE 'LOGMNR%'";

            using(OracleCommand cmd = new OracleCommand(sql, conn))
            {
                //cmd.Parameters.Add("?db", db);
                using(OracleDataReader rdr = cmd.ExecuteReader())
                {
                    List<User_Constraints_Row> list = new List<User_Constraints_Row>();
                    while(rdr.Read())
                    {
                        list.Add( fromRow(rdr) );
                    }
                    return list;
                }
            }

        }
    }
}
