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

	internal class ArrayHelper<T> : ISimpleList<T>
	{
		private object owner;
		private FieldInfo fieldInfo;
		private PropertyInfo propertyInfo;

		protected IEnumerable GetValue()
		{
			if (fieldInfo != null)
				return (IEnumerable)fieldInfo.GetValue(owner);
			return (IEnumerable)propertyInfo.GetGetMethod().Invoke(owner, new object[0]);
		}

		protected System.Type GetValueType()
		{
			if (fieldInfo != null)
				return fieldInfo.FieldType;
			return propertyInfo.PropertyType;
		}

		protected void SetValue(IEnumerable value)
		{
			if (fieldInfo != null)
				fieldInfo.SetValue(owner, value);
			else
				propertyInfo.GetSetMethod().Invoke(owner, new object[] { value });
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
			fieldInfo = o.GetType().GetField(fieldName);
			propertyInfo = o.GetType().GetProperty(fieldName);
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
		public Column()
		{
			SpecifiedHelper.Register(this);
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
