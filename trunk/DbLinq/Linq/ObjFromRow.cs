////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
////////////////////////////////////////////////////////////////////

#if USE_REFLECTION_TO_RETRIEVE_DATA
using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace DBLinq.Linq
{
    public interface IObjFromRow<T>
    {
        T FromRow(MySqlDataReader rdr);
    }

    //THESE CLASSES BELOW ARE FOR EXPERIMENTATION.
    //QUESTION ABOUT CASTING INTERFACES HAS BEEN POSTED TO THE LINQ FORUM

    /// <summary>
    /// handles reading of a string or int from SqlDataReader, and returning it immediately
    /// </summary>
    public class ObjFromRow_Simple<T> : IObjFromRow<T>
    {
        public T FromRow(MySqlDataReader rdr)
        {
            if(typeof(T)==typeof(int)){
                int iresult = rdr.GetInt32(0);
                //return (T)iresult; //cannot convert type 'int' to 'T'
                object oresult = iresult;
                return (T)oresult;
            }
            throw new ApplicationException("TODO L18: handle add'l type "+typeof(T));
        }
    }

    /// <summary>
    /// handles reading of a string or int from SqlDataReader, and returning it immediately
    /// </summary>
    public class ObjFromRow_Complex<T> : IObjFromRow<T>
        where T:new()
    {
        public T FromRow(MySqlDataReader rdr)
        {
            throw new ApplicationException("TODO L38: handle fromRow for a complex type");
        }
    }
}
#endif
