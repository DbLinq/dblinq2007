using System;
using System.Collections.Generic;
using System.Text;
//using MySql.Data.MySqlClient;

#if UNUSED
namespace DBLinq.Linq.clause
{
    class DeleteClauseBuilder
    {
        /// <summary>
        /// given type Employee, return 'INSERT Employee (ID, Name) VALUES (?p1,?p2)'
        /// (by examining [Table] and [Column] attribs)
        /// </summary>
        public static MySqlCommand GetClause(object objectToInsert, ProjectionData projData)
        {
            if(objectToInsert==null || projData==null)
                throw new ArgumentNullException("InsertClauseBuilder has null args");
            if(projData.fields.Count<1 || projData.fields[0].columnAttribute==null)
                throw new ApplicationException("InsertClauseBuilder need to receive types that have ColumnAttributes");

            StringBuilder sb = new StringBuilder("DELETE ");
            sb.Append(projData.tableAttribute.Name).Append(" WHERE ");

            sb.Append(") ");
            return null;
        }
    }
}
#endif