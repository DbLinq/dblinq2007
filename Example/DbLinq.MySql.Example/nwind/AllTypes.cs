#region Auto-generated classes for AllTypes database on 2008-04-28 21:26:17Z

//
//  ____  _     __  __      _        _
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from AllTypes on 2008-04-28 21:26:17Z
// Please visit http://linq.to/db for more information

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using DbLinq.Linq;
using DbLinq.Linq.Mapping;

namespace AllTypesExample
{
	public partial class AllTypes : DbLinq.Linq.DataContext
	{
		public AllTypes(System.Data.IDbConnection connection)
		: base(connection, new DbLinq.MySql.MySqlVendor())
		{
		}

		public AllTypes(System.Data.IDbConnection connection, DbLinq.Vendor.IVendor vendor)
		: base(connection, vendor)
		{
		}

		public Table<Allinttypes> Allinttypes { get { return GetTable<Allinttypes>(); } }
		public Table<Floattypes> Floattypes { get { return GetTable<Floattypes>(); } }
		public Table<Othertypes> Othertypes { get { return GetTable<Othertypes>(); } }

	}

	[Table(Name = "AllTypes.allinttypes")]
	public partial class Allinttypes : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region long BigInt

		private long bigInt;
		[DebuggerNonUserCode]
		[Column(Storage = "bigInt", Name = "bigInt", DbType = "bigint(20)", CanBeNull = false, Expression = null)]
		public long BigInt
		{
			get
			{
				return bigInt;
			}
			set
			{
				if (value != bigInt)
				{
					bigInt = value;
					OnPropertyChanged("BigInt");
				}
			}
		}

		#endregion

		#region long? BigIntN

		private long? bigIntN;
		[DebuggerNonUserCode]
		[Column(Storage = "bigIntN", Name = "bigIntN", DbType = "bigint(20)", Expression = null)]
		public long? BigIntN
		{
			get
			{
				return bigIntN;
			}
			set
			{
				if (value != bigIntN)
				{
					bigIntN = value;
					OnPropertyChanged("BigIntN");
				}
			}
		}

		#endregion

		#region byte Boolean

		private byte boolean;
		[DebuggerNonUserCode]
		[Column(Storage = "boolean", Name = "boolean", DbType = "tinyint(1)", CanBeNull = false, Expression = null)]
		public byte Boolean
		{
			get
			{
				return boolean;
			}
			set
			{
				if (value != boolean)
				{
					boolean = value;
					OnPropertyChanged("Boolean");
				}
			}
		}

		#endregion

		#region byte? BoolN

		private byte? boolN;
		[DebuggerNonUserCode]
		[Column(Storage = "boolN", Name = "boolN", DbType = "tinyint(1)", Expression = null)]
		public byte? BoolN
		{
			get
			{
				return boolN;
			}
			set
			{
				if (value != boolN)
				{
					boolN = value;
					OnPropertyChanged("BoolN");
				}
			}
		}

		#endregion

		#region byte Byte

		private byte @byte;
		[DebuggerNonUserCode]
		[Column(Storage = "byte", Name = "byte", DbType = "tinyint(3) unsigned", CanBeNull = false, Expression = null)]
		public byte Byte
		{
			get
			{
				return @byte;
			}
			set
			{
				if (value != @byte)
				{
					@byte = value;
					OnPropertyChanged("Byte");
				}
			}
		}

		#endregion

		#region byte? ByteN

		private byte? byteN;
		[DebuggerNonUserCode]
		[Column(Storage = "byteN", Name = "byteN", DbType = "tinyint(3) unsigned", Expression = null)]
		public byte? ByteN
		{
			get
			{
				return byteN;
			}
			set
			{
				if (value != byteN)
				{
					byteN = value;
					OnPropertyChanged("ByteN");
				}
			}
		}

		#endregion

		#region ushort DbLinqEnumTest

		private ushort dbLinqEnumTest;
		[DebuggerNonUserCode]
		[Column(Storage = "dbLinqEnumTest", Name = "DbLinq_EnumTest", DbType = "smallint(5) unsigned", CanBeNull = false, Expression = null)]
		public ushort DbLinqEnumTest
		{
			get
			{
				return dbLinqEnumTest;
			}
			set
			{
				if (value != dbLinqEnumTest)
				{
					dbLinqEnumTest = value;
					OnPropertyChanged("DbLinqEnumTest");
				}
			}
		}

		#endregion

		#region uint Int

		[AutoGenId]
		private uint @int;
		[DebuggerNonUserCode]
		[Column(Storage = "int", Name = "int", DbType = "int unsigned", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = null)]
		public uint Int
		{
			get
			{
				return @int;
			}
			set
			{
				if (value != @int)
				{
					@int = value;
					OnPropertyChanged("Int");
				}
			}
		}

		#endregion

		#region uint? IntN

		private uint? intN;
		[DebuggerNonUserCode]
		[Column(Storage = "intN", Name = "intN", DbType = "int unsigned", Expression = null)]
		public uint? IntN
		{
			get
			{
				return intN;
			}
			set
			{
				if (value != intN)
				{
					intN = value;
					OnPropertyChanged("IntN");
				}
			}
		}

		#endregion

		#region uint Short

		private uint @short;
		[DebuggerNonUserCode]
		[Column(Storage = "short", Name = "short", DbType = "mediumint unsigned", CanBeNull = false, Expression = null)]
		public uint Short
		{
			get
			{
				return @short;
			}
			set
			{
				if (value != @short)
				{
					@short = value;
					OnPropertyChanged("Short");
				}
			}
		}

		#endregion

		#region uint? ShortN

		private uint? shortN;
		[DebuggerNonUserCode]
		[Column(Storage = "shortN", Name = "shortN", DbType = "mediumint unsigned", Expression = null)]
		public uint? ShortN
		{
			get
			{
				return shortN;
			}
			set
			{
				if (value != shortN)
				{
					shortN = value;
					OnPropertyChanged("ShortN");
				}
			}
		}

		#endregion

		#region ushort SmallInt

		private ushort smallInt;
		[DebuggerNonUserCode]
		[Column(Storage = "smallInt", Name = "smallInt", DbType = "smallint(5) unsigned", CanBeNull = false, Expression = null)]
		public ushort SmallInt
		{
			get
			{
				return smallInt;
			}
			set
			{
				if (value != smallInt)
				{
					smallInt = value;
					OnPropertyChanged("SmallInt");
				}
			}
		}

		#endregion

		#region ushort? SmallIntN

		private ushort? smallIntN;
		[DebuggerNonUserCode]
		[Column(Storage = "smallIntN", Name = "smallIntN", DbType = "smallint(5) unsigned", Expression = null)]
		public ushort? SmallIntN
		{
			get
			{
				return smallIntN;
			}
			set
			{
				if (value != smallIntN)
				{
					smallIntN = value;
					OnPropertyChanged("SmallIntN");
				}
			}
		}

		#endregion

		#region byte? TinyIntS

		private byte? tinyIntS;
		[DebuggerNonUserCode]
		[Column(Storage = "tinyIntS", Name = "tinyIntS", DbType = "tinyint(1)", Expression = null)]
		public byte? TinyIntS
		{
			get
			{
				return tinyIntS;
			}
			set
			{
				if (value != tinyIntS)
				{
					tinyIntS = value;
					OnPropertyChanged("TinyIntS");
				}
			}
		}

		#endregion

		#region byte TinyIntU

		private byte tinyIntU;
		[DebuggerNonUserCode]
		[Column(Storage = "tinyIntU", Name = "tinyIntU", DbType = "tinyint(1) unsigned", CanBeNull = false, Expression = null)]
		public byte TinyIntU
		{
			get
			{
				return tinyIntU;
			}
			set
			{
				if (value != tinyIntU)
				{
					tinyIntU = value;
					OnPropertyChanged("TinyIntU");
				}
			}
		}

		#endregion

		#region byte? TinyIntUn

		private byte? tinyIntUn;
		[DebuggerNonUserCode]
		[Column(Storage = "tinyIntUn", Name = "tinyIntUN", DbType = "tinyint(1) unsigned", Expression = null)]
		public byte? TinyIntUn
		{
			get
			{
				return tinyIntUn;
			}
			set
			{
				if (value != tinyIntUn)
				{
					tinyIntUn = value;
					OnPropertyChanged("TinyIntUn");
				}
			}
		}

		#endregion

	}

	[Table(Name = "AllTypes.floattypes")]
	public partial class Floattypes : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region decimal Decimal

		private decimal @decimal;
		[DebuggerNonUserCode]
		[Column(Storage = "decimal", Name = "decimal", DbType = "decimal", CanBeNull = false, Expression = null)]
		public decimal Decimal
		{
			get
			{
				return @decimal;
			}
			set
			{
				if (value != @decimal)
				{
					@decimal = value;
					OnPropertyChanged("Decimal");
				}
			}
		}

		#endregion

		#region decimal? DecimalN

		private decimal? decimalN;
		[DebuggerNonUserCode]
		[Column(Storage = "decimalN", Name = "decimalN", DbType = "decimal", Expression = null)]
		public decimal? DecimalN
		{
			get
			{
				return decimalN;
			}
			set
			{
				if (value != decimalN)
				{
					decimalN = value;
					OnPropertyChanged("DecimalN");
				}
			}
		}

		#endregion

		#region double Double

		private double @double;
		[DebuggerNonUserCode]
		[Column(Storage = "double", Name = "double", DbType = "double", CanBeNull = false, Expression = null)]
		public double Double
		{
			get
			{
				return @double;
			}
			set
			{
				if (value != @double)
				{
					@double = value;
					OnPropertyChanged("Double");
				}
			}
		}

		#endregion

		#region double? DoubleN

		private double? doubleN;
		[DebuggerNonUserCode]
		[Column(Storage = "doubleN", Name = "doubleN", DbType = "double", Expression = null)]
		public double? DoubleN
		{
			get
			{
				return doubleN;
			}
			set
			{
				if (value != doubleN)
				{
					doubleN = value;
					OnPropertyChanged("DoubleN");
				}
			}
		}

		#endregion

		#region float Float

		private float @float;
		[DebuggerNonUserCode]
		[Column(Storage = "float", Name = "float", DbType = "float", CanBeNull = false, Expression = null)]
		public float Float
		{
			get
			{
				return @float;
			}
			set
			{
				if (value != @float)
				{
					@float = value;
					OnPropertyChanged("Float");
				}
			}
		}

		#endregion

		#region float? FloatN

		private float? floatN;
		[DebuggerNonUserCode]
		[Column(Storage = "floatN", Name = "floatN", DbType = "float", Expression = null)]
		public float? FloatN
		{
			get
			{
				return floatN;
			}
			set
			{
				if (value != floatN)
				{
					floatN = value;
					OnPropertyChanged("FloatN");
				}
			}
		}

		#endregion

		#region int Id1

		[AutoGenId]
		private int id1;
		[DebuggerNonUserCode]
		[Column(Storage = "id1", Name = "id1", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = null)]
		public int Id1
		{
			get
			{
				return id1;
			}
			set
			{
				if (value != id1)
				{
					id1 = value;
					OnPropertyChanged("Id1");
				}
			}
		}

		#endregion

		#region decimal Numeric

		private decimal numeric;
		[DebuggerNonUserCode]
		[Column(Storage = "numeric", Name = "numeric", DbType = "decimal", CanBeNull = false, Expression = null)]
		public decimal Numeric
		{
			get
			{
				return numeric;
			}
			set
			{
				if (value != numeric)
				{
					numeric = value;
					OnPropertyChanged("Numeric");
				}
			}
		}

		#endregion

		#region decimal? NumericN

		private decimal? numericN;
		[DebuggerNonUserCode]
		[Column(Storage = "numericN", Name = "numericN", DbType = "decimal", Expression = null)]
		public decimal? NumericN
		{
			get
			{
				return numericN;
			}
			set
			{
				if (value != numericN)
				{
					numericN = value;
					OnPropertyChanged("NumericN");
				}
			}
		}

		#endregion

		#region double Real

		private double real;
		[DebuggerNonUserCode]
		[Column(Storage = "real", Name = "real", DbType = "double", CanBeNull = false, Expression = null)]
		public double Real
		{
			get
			{
				return real;
			}
			set
			{
				if (value != real)
				{
					real = value;
					OnPropertyChanged("Real");
				}
			}
		}

		#endregion

		#region double? RealN

		private double? realN;
		[DebuggerNonUserCode]
		[Column(Storage = "realN", Name = "realN", DbType = "double", Expression = null)]
		public double? RealN
		{
			get
			{
				return realN;
			}
			set
			{
				if (value != realN)
				{
					realN = value;
					OnPropertyChanged("RealN");
				}
			}
		}

		#endregion

	}

	[Table(Name = "AllTypes.othertypes")]
	public partial class Othertypes : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region Custom type definition for RainbowType

		public enum RainbowType
		{
			red,
			orange,
			yellow,
		}

		#endregion

		#region System.Byte[] Blob

		private System.Byte[] blob;
		[DebuggerNonUserCode]
		[Column(Storage = "blob", Name = "blob", DbType = "blob", CanBeNull = false, Expression = null)]
		public System.Byte[] Blob
		{
			get
			{
				return blob;
			}
			set
			{
				if (value != blob)
				{
					blob = value;
					OnPropertyChanged("Blob");
				}
			}
		}

		#endregion

		#region System.Byte[] BlobN

		private System.Byte[] blobN;
		[DebuggerNonUserCode]
		[Column(Storage = "blobN", Name = "blobN", DbType = "blob", Expression = null)]
		public System.Byte[] BlobN
		{
			get
			{
				return blobN;
			}
			set
			{
				if (value != blobN)
				{
					blobN = value;
					OnPropertyChanged("BlobN");
				}
			}
		}

		#endregion

		#region string Char

		private string @char;
		[DebuggerNonUserCode]
		[Column(Storage = "char", Name = "char", DbType = "char(1)", CanBeNull = false, Expression = null)]
		public string Char
		{
			get
			{
				return @char;
			}
			set
			{
				if (value != @char)
				{
					@char = value;
					OnPropertyChanged("Char");
				}
			}
		}

		#endregion

		#region string CharN

		private string charN;
		[DebuggerNonUserCode]
		[Column(Storage = "charN", Name = "charN", DbType = "char(1)", Expression = null)]
		public string CharN
		{
			get
			{
				return charN;
			}
			set
			{
				if (value != charN)
				{
					charN = value;
					OnPropertyChanged("CharN");
				}
			}
		}

		#endregion

		#region System.DateTime DateTime

		private System.DateTime dateTime;
		[DebuggerNonUserCode]
		[Column(Storage = "dateTime", Name = "DateTime", DbType = "datetime", CanBeNull = false, Expression = null)]
		public System.DateTime @DateTime
		{
			get
			{
				return dateTime;
			}
			set
			{
				if (value != dateTime)
				{
					dateTime = value;
					OnPropertyChanged("DateTime");
				}
			}
		}

		#endregion

		#region System.DateTime? DateTimeN

		private System.DateTime? dateTimeN;
		[DebuggerNonUserCode]
		[Column(Storage = "dateTimeN", Name = "DateTimeN", DbType = "datetime", Expression = null)]
		public System.DateTime? DateTimeN
		{
			get
			{
				return dateTimeN;
			}
			set
			{
				if (value != dateTimeN)
				{
					dateTimeN = value;
					OnPropertyChanged("DateTimeN");
				}
			}
		}

		#endregion

		#region int Id1

		[AutoGenId]
		private int id1;
		[DebuggerNonUserCode]
		[Column(Storage = "id1", Name = "id1", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = null)]
		public int Id1
		{
			get
			{
				return id1;
			}
			set
			{
				if (value != id1)
				{
					id1 = value;
					OnPropertyChanged("Id1");
				}
			}
		}

		#endregion

		#region RainbowType Rainbow

		private RainbowType rainbow;
		[DebuggerNonUserCode]
		[Column(Storage = "rainbow", Name = "rainbow", DbType = "enum('red','orange','yellow')", CanBeNull = false, Expression = null)]
		public RainbowType Rainbow
		{
			get
			{
				return rainbow;
			}
			set
			{
				if (value != rainbow)
				{
					rainbow = value;
					OnPropertyChanged("Rainbow");
				}
			}
		}

		#endregion

		#region string Text

		private string text;
		[DebuggerNonUserCode]
		[Column(Storage = "text", Name = "text", DbType = "text", CanBeNull = false, Expression = null)]
		public string Text
		{
			get
			{
				return text;
			}
			set
			{
				if (value != text)
				{
					text = value;
					OnPropertyChanged("Text");
				}
			}
		}

		#endregion

		#region string TextN

		private string textN;
		[DebuggerNonUserCode]
		[Column(Storage = "textN", Name = "textN", DbType = "text", Expression = null)]
		public string TextN
		{
			get
			{
				return textN;
			}
			set
			{
				if (value != textN)
				{
					textN = value;
					OnPropertyChanged("TextN");
				}
			}
		}

		#endregion

	}
}
