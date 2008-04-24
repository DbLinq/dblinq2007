#region MIT license
// 
// Copyright (c) 2007-2008 Jiri Moudry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using DbLinq.Util;

namespace DbLinq.Schema.Dbml
{
    public interface ISimpleList<T> : IEnumerable<T>
    {
        // sort of light IList<>
        int Count { get; }
        void Add(T item);
        // extension
        void Sort(IComparer<T> sorter);
        List<T> FindAll(Predicate<T> match);
    }

    public class ExtendedType
    {
        public enum ExtendedTypeType
        {
            ExtendedTypeSimple,
            ExtendedTypeEnum,
        }

        private object owner;
        private MemberInfo memberInfo;

        public string Member
        {
            get { return (string)memberInfo.GetMemberValue(owner); }
        }

        private class EnumValuesHolder : IDictionary<string, int>
        {
            private string name;
            private IDictionary<string, int> dictionary;
            private object owner;
            private MemberInfo memberInfo;

            public static bool IsEnum(string literalType)
            {
                string enumName;
                IDictionary<string, int> values;
                return Extract(literalType, out enumName, out values);
            }

            public static string GetEnumName(string literalType)
            {
                string enumName;
                IDictionary<string, int> values;
                Extract(literalType, out enumName, out values);
                return enumName;
            }

            /// <summary>
            /// Extracts enum name and value from a given string.
            /// The string is in the following form:
            /// enumName key1[=value1]{,keyN[=valueN]}
            /// if enumName is 'enum', then the enum is anonymous
            /// </summary>
            /// <param name="literalType"></param>
            /// <param name="enumName"></param>
            /// <param name="values"></param>
            /// <returns></returns>
            private static bool Extract(string literalType, out string enumName, out IDictionary<string, int> values)
            {
                enumName = null;
                values = null;

                var nameValues = literalType.Split(new[] { ' ' }, 2);
                if (nameValues.Length == 2)
                {
                    // extract the name
                    string name = nameValues[0].Trim();
                    if (!name.IsIdentifier())
                        return false;

                    // now extract the values
                    IDictionary<string, int> readValues = new Dictionary<string, int>();
                    int currentValue = 1;
                    var keyValues = nameValues[1].Split(',');
                    foreach (var keyValue in keyValues)
                    {
                        // a value may indicate its numeric equivalent, or not (in this case, we work the same way as C# enums, with an implicit counter)
                        var keyValueParts = keyValue.Split(new[] { '=' }, 2);
                        var key = keyValueParts[0].Trim();

                        if (!key.IsIdentifier())
                            return false;

                        if (keyValueParts.Length > 1)
                        {
                            if (!int.TryParse(keyValueParts[1], out currentValue))
                                return false;
                        }
                        readValues[key] = currentValue++;
                    }
                    if (name == "enum")
                        enumName = string.Empty;
                    else
                        enumName = name;
                    values = readValues;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Does the opposite: creates a literal string from values
            /// </summary>
            private void UpdateMember()
            {
                var pairs = from kvp in dictionary orderby kvp.Value select kvp;
                int currentValue = 1;
                var keyValues = new List<string>();
                foreach (var pair in pairs)
                {
                    string keyValue;
                    if (pair.Value == currentValue)
                        keyValue = pair.Key;
                    else
                    {
                        currentValue = pair.Value;
                        keyValue = string.Format("{0}={1}", pair.Key, pair.Value);
                    }
                    keyValues.Add(keyValue);
                    currentValue++;
                }
                string literalType = name ?? "enum";
                literalType += " ";
                literalType += string.Join(", ", keyValues.ToArray());
                memberInfo.SetMemberValue(owner, literalType);
            }

            public EnumValuesHolder(object owner, MemberInfo memberInfo)
            {
                this.owner = owner;
                this.memberInfo = memberInfo;
                Extract((string)memberInfo.GetMemberValue(owner), out name, out dictionary);
            }

            #region IDictionary implementation

            public void Add(KeyValuePair<string, int> item)
            {
                dictionary.Add(item);
                UpdateMember();
            }

            public void Clear()
            {
                dictionary.Clear();
                UpdateMember();
            }

            public bool Contains(KeyValuePair<string, int> item)
            {
                return dictionary.Contains(item);
            }

            public void CopyTo(KeyValuePair<string, int>[] array, int arrayIndex)
            {
                dictionary.CopyTo(array, arrayIndex);
            }

            public bool Remove(KeyValuePair<string, int> item)
            {
                bool removed = dictionary.Remove(item);
                UpdateMember();
                return removed;
            }

            public int Count
            {
                get { return dictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return dictionary.IsReadOnly; }
            }

            public bool ContainsKey(string key)
            {
                return dictionary.ContainsKey(key);
            }

            public void Add(string key, int value)
            {
                dictionary.Add(key, value);
                UpdateMember();
            }

            public bool Remove(string key)
            {
                bool removed = dictionary.Remove(key);
                UpdateMember();
                return removed;
            }

            public bool TryGetValue(string key, out int value)
            {
                return dictionary.TryGetValue(key, out value);
            }

            public int this[string key]
            {
                get { return dictionary[key]; }
                set
                {
                    dictionary[key] = value;
                    UpdateMember();
                }
            }

            public ICollection<string> Keys
            {
                get { return dictionary.Keys; }
            }

            public ICollection<int> Values
            {
                get { return dictionary.Values; }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<KeyValuePair<string, int>>)this).GetEnumerator();
            }

            public IEnumerator<KeyValuePair<string, int>> GetEnumerator()
            {
                return dictionary.GetEnumerator();
            }

            #endregion
        }

        public ExtendedTypeType Type
        {
            get
            {
                if (EnumValuesHolder.IsEnum(Member))
                    return ExtendedTypeType.ExtendedTypeEnum;
                return ExtendedTypeType.ExtendedTypeSimple;
            }
        }

        public IDictionary<string, int> EnumValues
        {
            get
            {
                return new EnumValuesHolder(owner, memberInfo);
            }
        }

        public string EnumName
        {
            get { return EnumValuesHolder.GetEnumName(Member); }
        }

        public string SimpleType
        {
            get { return Member; }
        }

        // required (and unused) by the serializer
        public ExtendedType()
        {
        }

        public ExtendedType(object o, string fieldName)
        {
            owner = o;
            memberInfo = o.GetType().GetMember(fieldName)[0];
        }
    }

    internal class ArrayHelper<T> : ISimpleList<T>
    {
        private object owner;
        private MemberInfo memberInfo;

        protected IEnumerable GetValue()
        {
            return (IEnumerable)memberInfo.GetMemberValue(owner);
        }

        protected System.Type GetValueType()
        {
            return memberInfo.GetMemberType();
        }

        protected void SetValue(IEnumerable value)
        {
            memberInfo.SetMemberValue(owner, value);
        }

        protected List<T> GetDynamic()
        {
            List<T> list = new List<T>();
            var fieldValue = GetValue();
            if (fieldValue != null)
            {
                foreach (object o in fieldValue)
                {
                    if (o is T)
                        list.Add((T)o);
                }
            }
            return list;
        }

        protected void SetStatic(IList<T> list)
        {
            var others = new ArrayList();
            var fieldValue = GetValue();
            if (fieldValue != null)
            {
                foreach (object o in fieldValue)
                {
                    if (!(o is T))
                        others.Add(o);
                }
            }
            var array = Array.CreateInstance(GetValueType().GetElementType(), others.Count + list.Count);
            others.CopyTo(array);
            for (int listIndex = 0; listIndex < list.Count; listIndex++)
            {
                array.SetValue(list[listIndex], others.Count + listIndex);
            }
            SetValue(array);
        }

        public ArrayHelper(object o, string fieldName)
        {
            owner = o;
            memberInfo = o.GetType().GetMember(fieldName)[0];
        }

        #region IList<T> Members

        public int IndexOf(T item)
        {
            return GetDynamic().IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            IList<T> dynamicArray = GetDynamic();
            dynamicArray.Insert(index, item);
            SetStatic(dynamicArray);
        }

        public void RemoveAt(int index)
        {
            IList<T> dynamicArray = GetDynamic();
            dynamicArray.RemoveAt(index);
            SetStatic(dynamicArray);
        }

        public T this[int index]
        {
            get
            {
                return GetDynamic()[index];
            }
            set
            {
                IList<T> dynamicArray = GetDynamic();
                dynamicArray[index] = value;
                SetStatic(dynamicArray);
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            IList<T> dynamicArray = GetDynamic();
            dynamicArray.Add(item);
            SetStatic(dynamicArray);
        }

        public void Clear()
        {
            SetStatic(new T[0]);
        }

        public bool Contains(T item)
        {
            return GetDynamic().Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            GetDynamic().CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return GetDynamic().Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            IList<T> dynamicArray = GetDynamic();
            bool removed = dynamicArray.Remove(item);
            SetStatic(dynamicArray);
            return removed;
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return GetDynamic().GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetDynamic().GetEnumerator();
        }

        #endregion

        public void Sort(IComparer<T> sorter)
        {
            List<T> list = GetDynamic();
            list.Sort(sorter);
            SetStatic(list);
        }

        public List<T> FindAll(Predicate<T> match)
        {
            return GetDynamic().FindAll(match);
        }
    }

    /// <summary>
    /// The schema generates *Specified properties that we must set when the related property changes
    /// </summary>
    internal static class SpecifiedHelper
    {
        public static void Register(INotifyPropertyChanged notify)
        {
            notify.PropertyChanged += Notify_PropertyChanged;
        }

        private static void Notify_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.EndsWith("Specified"))
                return;

            PropertyInfo propertyInfo = sender.GetType().GetProperty(e.PropertyName);
#if NO
            bool? changed = null;
            object newValue = propertyInfo.GetGetMethod().Invoke(sender, new object[0]); ;
            if (!propertyInfo.PropertyType.IsValueType)
            {
                changed = newValue != null;
            }
            else if (propertyInfo.PropertyType.IsEnum)
            {
                changed = (int)newValue != 0;
            }
            else if (propertyInfo.PropertyType == typeof(bool))
            {
                changed = (bool)newValue;
            }
#else
            bool? changed = true;
#endif
            if (changed.HasValue)
            {
                PropertyInfo specifiedPropertyInfo = sender.GetType().GetProperty(e.PropertyName + "Specified");
                if (specifiedPropertyInfo != null)
                    specifiedPropertyInfo.GetSetMethod().Invoke(sender, new object[] { changed.Value });
            }
        }
    }

    public partial class Database
    {
        [XmlIgnore]
        public ISimpleList<Table> Tables;
        [XmlIgnore]
        public ISimpleList<Function> Functions;

        public Database()
        {
            SpecifiedHelper.Register(this);
            Tables = new ArrayHelper<Table>(this, "Table");
            Functions = new ArrayHelper<Function>(this, "Function");
        }
    }

    public partial class Table
    {
        public Table()
        {
            Type = new Type();
            SpecifiedHelper.Register(this);
        }

        [XmlIgnore]
        public bool _isChild;

        public override string ToString()
        {
            return String.Format("{0} ({1}), {2}", Member, Name, Type);
        }
    }

    public partial class Type
    {
        [XmlIgnore]
        public ISimpleList<Column> Columns;
        [XmlIgnore]
        public ISimpleList<Association> Associations;

        public Type()
        {
            SpecifiedHelper.Register(this);
            Columns = new ArrayHelper<Column>(this, "Items");
            Associations = new ArrayHelper<Association>(this, "Items");
        }

        public override string ToString()
        {
            return Columns.Count + " Columns";
        }
    }

    public partial class Function
    {
        [XmlIgnore]
        public bool BodyContainsSelectStatement;

        [XmlIgnore]
        public ISimpleList<Parameter> Parameters;
        [XmlIgnore]
        public Return Return
        {
            get
            {
                if (Items == null)
                    return null;
                foreach (object item in Items)
                {
                    var r = item as Return;
                    if (r != null)
                        return r;
                }
                return null;
            }
            set
            {
                if (Items == null)
                {
                    Items = new[] { value };
                    return;
                }
                for (int index = 0; index < Items.Length; index++)
                {
                    if (Items[index] is Return)
                    {
                        Items[index] = value;
                        return;
                    }
                }
                List<object> items = new List<object>(Items);
                items.Add(value);
                Items = items.ToArray();
            }
        }

        [XmlIgnore]
        public object ElementType;

        public Function()
        {
            SpecifiedHelper.Register(this);
            Parameters = new ArrayHelper<Parameter>(this, "Parameter");
        }
    }

    public partial class Parameter
    {
        [XmlIgnore]
        public bool DirectionIn
        {
            get { return Direction == ParameterDirection.In || Direction == ParameterDirection.InOut; }
        }

        [XmlIgnore]
        public bool DirectionOut
        {
            get { return Direction == ParameterDirection.Out || Direction == ParameterDirection.InOut; }
        }
    }

    public partial class Association
    {
        public Association()
        {
            SpecifiedHelper.Register(this);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public partial class Column
    {
        public ExtendedType ExtendedType;
        /// <summary>
        /// ReflectedType is used at generation time for an extended type name (or replacement if extended type name is anonymous)
        /// </summary>
        public string ExtendedTypeName;

        public Column()
        {
            SpecifiedHelper.Register(this);
            ExtendedType = new ExtendedType(this, "Type");
        }

        public override string ToString()
        {
            return String.Format("{0} ({1}): {2} ({3})", Member, Name, Type, DbType);
        }
    }

    public partial class Connection
    {
        public Connection()
        {
            SpecifiedHelper.Register(this);
        }
    }

    public partial class Parameter
    {
        public Parameter()
        {
            SpecifiedHelper.Register(this);
        }
    }
    public partial class Return
    {
        public Return()
        {
            SpecifiedHelper.Register(this);
        }
    }

    public partial class TableFunction
    {
        public TableFunction()
        {
            SpecifiedHelper.Register(this);
        }
    }
    public partial class TableFunctionParameter
    {
        public TableFunctionParameter()
        {
            SpecifiedHelper.Register(this);
        }
    }
    public partial class TableFunctionReturn
    {
        public TableFunctionReturn()
        {
            SpecifiedHelper.Register(this);
        }
    }
}
