#region MIT license
////////////////////////////////////////////////////////////////////
// MIT license:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Marc Gravell
////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq.Mapping;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Data.Common;

namespace DbLinq.Vendor.Implementation {

  public abstract partial class Vendor : IVendor {

    // re-use args to minimize GEN0
    readonly ValueConversionEventArgs conversionArgs = new ValueConversionEventArgs();

    public event EventHandler<ValueConversionEventArgs> ConvertValue;

    internal object OnConvertValue(int ordinal, IDataRecord record, object value) {
      if (ConvertValue == null) {
        return value;
      } else {
        conversionArgs.Init(ordinal, record, value);
        ConvertValue(this, conversionArgs);
        return conversionArgs.Value;
      }
    }

    /// <summary>
    /// Compares arrays of objects using the supplied comparer (or default is none supplied)
    /// </summary>
    class ArrayComparer<T> : IEqualityComparer<T[]> {
      private readonly IEqualityComparer<T> comparer;
      public ArrayComparer() : this(null) { }
      public ArrayComparer(IEqualityComparer<T> comparer) {
        this.comparer = comparer ?? EqualityComparer<T>.Default;
      }
      public int GetHashCode(T[] values) {
        if (values == null) return 0;
        int hashCode = 1;
        for (int i = 0; i < values.Length; i++) {
          hashCode = (hashCode * 13) + comparer.GetHashCode(values[i]);
        }
        return hashCode;
      }

      public bool Equals(T[] lhs, T[] rhs) {
        if (ReferenceEquals(lhs, rhs)) return true;
        if (lhs == null || rhs == null || lhs.Length != rhs.Length) return false;
        for (int i = 0; i < lhs.Length; i++) {
          if (!comparer.Equals(lhs[i], rhs[i])) return false;
        }
        return true;
      }
    }

    /// <summary>
    /// Represents a single bindable member of a type
    /// </summary>
    internal class BindingInfo {
      public bool CanBeNull { get; private set; }
      public MemberInfo StorageMember { get; private set; }
      public MemberInfo BindingMember { get; private set; }
      public BindingInfo(bool canBeNull, MemberInfo bindingMember, MemberInfo storageMember) {
        CanBeNull = canBeNull;
        BindingMember = bindingMember;
        StorageMember = storageMember;
      }
      public Type StorageType {
        get {
          switch (StorageMember.MemberType) {
            case MemberTypes.Field:
              return ((FieldInfo)StorageMember).FieldType;
            case MemberTypes.Property:
              return ((PropertyInfo)StorageMember).PropertyType;
            default:
              throw new NotSupportedException(string.Format("Unexpected member-type: {0}", StorageMember.Name));
          }
        }
      }
    }

    /// <summary>
    /// Responsible for creating and caching reader-delegates for compatible
    /// column sets; thread safe.
    /// </summary>
    static class InitializerCache<T> {
      /// <summary>
      /// Cache of all readers for this T (by column sets)
      /// </summary>
      static readonly Dictionary<string[], Func<IDataRecord, Vendor, T>> convertReaders
          = new Dictionary<string[], Func<IDataRecord, Vendor, T>>(
              new ArrayComparer<string>(StringComparer.InvariantCultureIgnoreCase)),
         vanillaReaders = new Dictionary<string[], Func<IDataRecord, Vendor, T>>(
              new ArrayComparer<string>(StringComparer.InvariantCultureIgnoreCase));

      /// <summary>
      /// Cache of all bindable columns for this T (by source-name)
      /// </summary>
      private static readonly SortedList<string, BindingInfo> dataMembers
          = new SortedList<string, BindingInfo>(StringComparer.InvariantCultureIgnoreCase);

      static bool TryGetBinding(string columnName, out BindingInfo binding) {
        return dataMembers.TryGetValue(columnName, out binding);
      }

      const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
      const MemberTypes PROP_FIELD = MemberTypes.Property | MemberTypes.Field;

      private static MemberInfo GetBindingMember(string name) {
        Type type = typeof(T);
        return FirstMember(type.GetMember(name, PROP_FIELD, FLAGS))
            ?? FirstMember(type.GetMember(name, PROP_FIELD, FLAGS | BindingFlags.IgnoreCase));
      }

      static InitializerCache() {

        Type type = typeof(T);
        foreach (MemberInfo member in type.GetMembers(FLAGS)) {
          if ((member.MemberType & PROP_FIELD) == 0) continue; // only applies to prop/fields
          ColumnAttribute col = Attribute.GetCustomAttribute(member, typeof(ColumnAttribute)) as ColumnAttribute;
          if (col == null) continue; // not a column
          string name = col.Name;
          if (string.IsNullOrEmpty(name)) { // default to self
            name = member.Name;
          }
          string storage = col.Storage;
          MemberInfo storageMember;
          if (string.IsNullOrEmpty(storage) || storage == name) { // default to self
            storageMember = member;
          } else {
            // locate prop/field: case-sensitive first, then insensitive
            storageMember = GetBindingMember(storage);
            if (storageMember == null) {
              throw new InvalidOperationException("Storage member not found: " + storage);
            }
          }
          if (storageMember.MemberType == MemberTypes.Property && !((PropertyInfo)storageMember).CanWrite) { // write to a r/o prop?
            throw new InvalidOperationException("Cannot write to readonly storage property: " + storage);
          }
          // log it...
          dataMembers.Add(name, new BindingInfo(col.CanBeNull, member, storageMember));
        }
      }

      static MemberInfo FirstMember(MemberInfo[] members) {
        return members != null && members.Length > 0 ? members[0] : null;
      }

      public static Func<IDataRecord, Vendor, T> GetInitializer(string[] names, bool useConversion) {
        if (names == null) throw new ArgumentNullException();
        Func<IDataRecord, Vendor, T> initializer;
        Dictionary<string[], Func<IDataRecord, Vendor, T>> cache =
            useConversion ? convertReaders : vanillaReaders;

        lock (cache) {
          if (!cache.TryGetValue(names, out initializer)) {
            initializer = CreateInitializer(names, useConversion);
            cache.Add((string[])names.Clone(), initializer);
          }
        }
        return initializer;
      }

      static Func<IDataRecord, Vendor, T> CreateInitializer(string[] names, bool useConversion) {
        Trace.WriteLine("Creating initializer for: " + typeof(T).Name);
        if (names == null) throw new ArgumentNullException("names");

        ParameterExpression readerParam = Expression.Parameter(typeof(IDataRecord), "record"),
            ctxParam = Expression.Parameter(typeof(Vendor), "ctx");

        Type entityType = typeof(T),
            underlyingEntityType = Nullable.GetUnderlyingType(entityType) ?? entityType,
            readerType = typeof(IDataRecord);
        List<MemberBinding> bindings = new List<MemberBinding>();

        Type[] byOrdinal = { typeof(int) };
        MethodInfo defaultMethod = readerType.GetMethod("GetValue", byOrdinal),
            isNullMethod = readerType.GetMethod("IsDBNull", byOrdinal),
            convertMethod = typeof(Vendor).GetMethod("OnConvertValue", BindingFlags.Instance | BindingFlags.NonPublic);

        NewExpression ctor = Expression.New(underlyingEntityType); // try this first...
        for (int ordinal = 0; ordinal < names.Length; ordinal++) {
          string name = names[ordinal];
          BindingInfo bindingInfo;
          if (!TryGetBinding(name, out bindingInfo)) { // try implicit binding
            MemberInfo member = GetBindingMember(name);
            if (member == null) continue; // not bound
            bindingInfo = new BindingInfo(true, member, member);
          }
          //Trace.WriteLine(string.Format("Binding {0} to {1} ({2})", name, bindingInfo.Member.Name, bindingInfo.Member.MemberType));
          Type valueType = bindingInfo.StorageType;

          Type underlyingType = Nullable.GetUnderlyingType(valueType) ?? valueType;

          // get the rhs of a binding
          MethodInfo method = readerType.GetMethod("Get" + underlyingType.Name, byOrdinal);
          Expression rhs;
          ConstantExpression ordinalExp = Expression.Constant(ordinal, typeof(int));
          if (method != null && method.ReturnType == underlyingType) {
            rhs = Expression.Call(readerParam, method, ordinalExp);
          } else {
            rhs = Expression.Convert(Expression.Call(readerParam, defaultMethod, ordinalExp), underlyingType);
          }

          if (underlyingType != valueType) {   // Nullable<T>; convert underlying T to T?
            rhs = Expression.Convert(rhs, valueType);
          }

          if (bindingInfo.CanBeNull && (underlyingType.IsClass || underlyingType != valueType)) {
            // reference-type of Nullable<T>; check for null
            // (conditional ternary operator)
            rhs = Expression.Condition(
                Expression.Call(readerParam, isNullMethod, ordinalExp),
                Expression.Constant(null, valueType), rhs);
          }
          if (useConversion) {
            rhs = Expression.Convert(Expression.Call(ctxParam, convertMethod, ordinalExp, readerParam,
                Expression.Convert(rhs, typeof(object))), valueType);
          }
          bindings.Add(Expression.Bind(bindingInfo.StorageMember, rhs));
        }
        Expression body = Expression.MemberInit(ctor, bindings);
        if (entityType != underlyingEntityType) { // entity itself was T? - so convert
          body = Expression.Convert(body, entityType);
        }
        return Expression.Lambda<Func<IDataRecord, Vendor, T>>(body, readerParam, ctxParam).Compile();
      }
    }

    // Crude benchmarks show it slightly faster than raw LINQ-to-SQL, but it is doing a lot less:
    // TODO: add change tracking
    // TODO: add composability
    // TODO: only homogeneous sets supported
    // TODO: merge this with RowEnumeratorCompiler code.
    public virtual IEnumerable<TResult> ExecuteQuery<TResult>(DbLinq.Linq.DataContext context, string sql, params object[] parameters) {
      using (IDbCommand command = context.DatabaseContext.CreateCommand()) {
        command.CommandText = ExecuteCommand_PrepareParams(command, sql, parameters);
        command.Connection.Open();
        using (IDataReader reader = command.ExecuteReader(
            CommandBehavior.CloseConnection | CommandBehavior.SingleResult)) {
          if (reader.Read()) {
            string[] names = new string[reader.FieldCount];
            for (int i = 0; i < names.Length; i++) {
              names[i] = reader.GetName(i);
            }
            Func<IDataRecord, Vendor, TResult> objInit = InitializerCache<TResult>.GetInitializer(names,
               ConvertValue != null );
            do { // walk the data 
              yield return objInit(reader, this);
            } while (reader.Read());
          }
          while (reader.NextResult()) { } // ensure any trailing errors caught 
        }
      }
    }

#if NET2Version
    // Alternate implementation which uses .NET 2 features. Uses optional HyperDescriptor for speed.
    public virtual IEnumerable<TResult> 
    ExecuteQuery<TResult>(DbLinq.Linq.DataContext context, string sql, params 
    object[] parameters)
                                                                    where 
    TResult : new() {
              using (IDbCommand command = 
    context.DatabaseContext.CreateCommand()) {
                string sql2 = ExecuteCommand_PrepareParams(command, sql, 
    parameters);
                command.CommandText = sql2;
                command.Connection.Open();
                using (IDataReader reader = command.ExecuteReader(
                         CommandBehavior.CloseConnection | 
    CommandBehavior.SingleResult)) {
                  if (reader.Read()) {
                    // prepare a buffer and look at the properties
                    object[] values = new object[reader.FieldCount];
                    PropertyDescriptor[] props = new 
    PropertyDescriptor[values.Length];
#if HyperDescriptor
                // Using Marc Gravell HyperDescriptor gets significantly better 
    reflection performance (~100 x faster)
                // http://www.codeproject.com/KB/cs/HyperPropertyDescriptor.aspx
                PropertyDescriptorCollection allProps = 
    PropertyHelper<TResult>.GetProperties();
#else
                    PropertyDescriptorCollection allProps = 
    TypeDescriptor.GetProperties(typeof(TResult));
#endif
                    for (int i = 0; i < props.Length; i++) {
                      string name = reader.GetName(i);
                      props[i] = allProps.Find(name, true);
                    }
                      do { // walk the data
                        reader.GetValues(values);
                        TResult t = new TResult();
                        for (int i = 0; i < props.Length; i++) {
                          // TODO: use char type conversion delegate.
                          if (props[i] != null) props[i].SetValue(t, values[i]);
                        }
                        yield return t;
                      } while (reader.Read());
                    }
                    while (reader.NextResult()) { } // ensure any trailing 
    errors caught
                  }
                }
              }

#if HyperDescriptor
             static class PropertyHelper<T>
        {
            private static readonly PropertyDescriptorCollection
    properties;
            public static PropertyDescriptorCollection GetProperties()
            {
                return properties;
            }
            static PropertyHelper()
            {
                // add HyperDescriptor (optional) and get the properties
                HyperTypeDescriptionProvider.Add(typeof(T));
                properties = TypeDescriptor.GetProperties(typeof(T));
                // ensure we have a readonly collection
                PropertyDescriptor[] propArray = new
    PropertyDescriptor[properties.Count];
                properties.CopyTo(propArray, 0);
                properties = new PropertyDescriptorCollection(propArray,
    true);
            }
        }
#endif
#endif

  }

}


#if ComparePerfomanceWithLinqToSQL
static class Program 
{ 
    [STAThread] 
    static void Main() 
    {
        // not fair to compare 1st hit of either; whichever
        // goes first will look slower due to Fusion
        Go(1, false);
        // but the rest are fair game...
        Go(100, true);
    }
    static void Check(string caption, int count, bool showResults, Func<IEnumerable<Order>> func)
    {
        int chk = 0, rowcount = 0;
        Stopwatch watch = new Stopwatch();
        watch.Start();
        string val = "";
        for (int i = 0; i < count; i++)
        {
            foreach (var row in func())
            {
                if (rowcount == 0)
                {
                    val = row.ShipCountry;
                }
                chk += row.OrderID;
                rowcount++;
            }
        }
        watch.Stop();
        if (showResults)
        {
            Console.WriteLine("{0}: {1} in {2}ms (chk: {3}) - {4}", caption, rowcount, watch.ElapsedMilliseconds, chk, val);
        }
    }
    const string SQL = @"
SELECT [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate], [t0].[RequiredDate], [t0].[ShippedDate], [t0].[ShipVia], [t0].[Freight], [t0].[ShipName], [t0].[ShipAddress], [t0].[ShipCity], [t0].[ShipRegion], [t0].[ShipPostalCode], [t0].[ShipCountry]
FROM [dbo].[Orders] AS [t0]
--WHERE [t0].ShipCountry = {0}";
    static IEnumerable<Order> ReadExpressionConvert()
    {

        MyDataContext ctx = new MyDataContext();
        ctx.ConvertValue += (sender, args) =>
        {
            if (args.Value != null && args.Value is string)
            {
                args.Value = ((string)args.Value) + "###";
            }
        };
        foreach (var row in ctx.ExecuteQuery<Order>(SQL, "UK"))
        {
            yield return row;
        }
    }
    static IEnumerable<Order> ReadExpressionVanilla()
    {

        MyDataContext ctx = new MyDataContext();
        foreach (var row in ctx.ExecuteQuery<Order>(SQL, "UK"))
        {
            yield return row;
        }
    }
    static IEnumerable<Order> ReadLinq1()
    {
        using (NorthwindDataContext ctx = new NorthwindDataContext(CS))
        {
            foreach (var row in ctx.Orders)
            {
                yield return row;
            }
        }
    }
    static IEnumerable<Order> ReadLinq2()
    {
        using (NorthwindDataContext ctx = new NorthwindDataContext(CS))
        {
            foreach (var row in ctx.ExecuteQuery<Order>(SQL, "UK"))
            {
                yield return row;
            }
        }
    }

    static void Go(int count, bool showResults)
    {
        Check("LINQ IQueryable", count, showResults, ReadLinq1);
        Check("LINQ ExecuteQuery", count, showResults, ReadLinq2);
        Check("MyDataContext (vanilla)", count, showResults, ReadExpressionVanilla);
        Check("MyDataContext (convert)", count, showResults, ReadExpressionConvert);
        
    }
    internal const string CS = @"Data Source=WO51950201XPLAP\SQLEXPRESS;Initial Catalog=Northwind;Integrated Security=True";

}

public IEnumerable<T> ExecuteQuery<T>(string command, params object[] parameters)
    {
        if (parameters == null) throw new ArgumentNullException("parameters");
        
        using (DbConnection conn = new SqlConnection(Program.CS)) 
        using (DbCommand cmd = conn.CreateCommand()) 
        {
            string[] paramNames = new string[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                paramNames[i] = "@p" + i.ToString();
                DbParameter param = cmd.CreateParameter();
                param.ParameterName = paramNames[i];
                param.Value = parameters[i] ?? DBNull.Value;
                cmd.Parameters.Add(param);
            }
            cmd.CommandType = CommandType.Text; 
            cmd.CommandText = string.Format(command, paramNames);
            conn.Open();
            using (DbDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection | CommandBehavior.SingleResult)) 
            { 
                if (reader.Read()) 
                {
                    string[] names = new string[reader.FieldCount];
                    for(int i = 0 ; i < names.Length ; i++) {
                        names[i] = reader.GetName(i);
                    }
                    Func<IDataRecord, MyDataContext, T> objInit = InitializerCache<T>.GetInitializer(names, ConvertValue != null);
                    do 
                    { // walk the data 
                        yield return objInit(reader, this); 
                    } while (reader.Read()); 
                } 
                while (reader.NextResult()) { } // ensure any trailing errors caught 
            }
        } 
    } 
}
#endif