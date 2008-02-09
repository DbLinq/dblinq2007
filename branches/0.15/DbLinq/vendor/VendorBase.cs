using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Mapping;
//using System.Data.OracleClient;
using DBLinq.util;
using DBLinq.Linq;

namespace DBLinq.vendor
{
    /// <summary>
    /// some IVendor functionality is the same for many vendors,
    /// implemented here as virtual functions.
    /// </summary>
    public abstract class VendorBase //: IVendor
    {
        //public abstract string VendorName { get; }
        //{
        //    get { throw new NotImplementedException(); }
        //}

        public virtual string SqlPingCommand
        {
            get { return "SELECT 11"; }
        }

        //public void ProcessPkField(DBLinq.Linq.ProjectionData projData, System.Data.Linq.Mapping.ColumnAttribute colAtt, StringBuilder sb, StringBuilder sbValues, StringBuilder sbIdentity, ref int numFieldsAdded)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// string concatenation, eg 'a||b' on Oracle.
        /// Customized in Postgres to add casting to varchar.
        /// Customized in Mysql to use CONCAT().
        /// </summary>
        /// <param name="parts"></param>
        /// <returns></returns>
        public virtual string Concat(List<DBLinq.util.ExpressionAndType> parts)
        {
            string[] arr = parts.Select(p => p.expression).ToArray();
            return string.Join("||", arr);
        }

        /// <summary>
        /// on Postgres or Oracle, return eg. ':P1'.
        /// Mysql needs to override to return '?P1'
        /// </summary>
        public virtual string ParamName(int index)
        {
            return ":P" + index;
        }

        public virtual void ProcessInsertedId(IDbCommand cmd1, ref object returnedId)
        {
            //only Oracle does anything
        }

        public virtual string String_Length_Function()
        {
            return "LENGTH";
        }

        //public string FieldName_Safe(string name)
        //{
        //    throw new NotImplementedException();
        //}

        //public int ExecuteCommand(DBLinq.Linq.MContext context, string sql, params object[] parameters)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
