#region Auto-generated classes for demodb database on 2008-03-18 17:50:53Z

//
//  ____  _     __  __      _        _
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from demodb on 2008-03-18 17:50:53Z
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

public partial class demodb : DbLinq.Ingres.IngresDataContext
{
	//public demodb(string connectionString)
	//    : base(connectionString)
	//{
	//}

	public demodb(IDbConnection connection)
	    : base(connection)
	{
	}

	public Table<TZ> TZ { get { return GetTable<TZ>(); } }
	public Table<UserProfile> UserProfile { get { return GetTable<UserProfile>(); } }
	public Table<IIeTabF5F6> IIeTabF5F6 { get { return GetTable<IIeTabF5F6>(); } }
	public Table<Version> Version { get { return GetTable<Version>(); } }
	public Table<Country> Country { get { return GetTable<Country>(); } }
	public Table<Airport> Airport { get { return GetTable<Airport>(); } }
	public Table<Route> Route { get { return GetTable<Route>(); } }
	public Table<Airline> Airline { get { return GetTable<Airline>(); } }
	public Table<FlightDay> FlightDay { get { return GetTable<FlightDay>(); } }
	public Table<FullRoute> FullRoute { get { return GetTable<FullRoute>(); } }

}

[Table(Name = "admin.tz")]
public partial class TZ : IModified
{
	// IModified backing field
	public bool IsModified{ get; set; }

	#region int TZID

	private int tZID;
	[Column(Storage = "tZID", Name = "tz_id", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public int TZID
	{
		get
		{
			return tZID;
		}
		set
		{
			if (value != tZID)
			{
				tZID = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string TZCode

	private string tZCode;
	[Column(Storage = "tZCode", Name = "tz_code", DbType = "NCHAR(5)")]
	[DebuggerNonUserCode]
	public string TZCode
	{
		get
		{
			return tZCode;
		}
		set
		{
			if (value != tZCode)
			{
				tZCode = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string TZName

	private string tZName;
	[Column(Storage = "tZName", Name = "tz_name", DbType = "NCHAR(40)")]
	[DebuggerNonUserCode]
	public string TZName
	{
		get
		{
			return tZName;
		}
		set
		{
			if (value != tZName)
			{
				tZName = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region decimal? TZUtCOffset

	private decimal? tZUtCOffset;
	[Column(Storage = "tZUtCOffset", Name = "tz_utc_offset", DbType = "DECIMAL(5)")]
	[DebuggerNonUserCode]
	public decimal? TZUtCOffset
	{
		get
		{
			return tZUtCOffset;
		}
		set
		{
			if (value != tZUtCOffset)
			{
				tZUtCOffset = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#warning L189 table admin.tz has no primary key. Multiple C# objects will refer to the same row.
}

[Table(Name = "admin.user_profile")]
public partial class UserProfile : IModified
{
	// IModified backing field
	public bool IsModified{ get; set; }

	#region int UpID

	private int upID;
	[Column(Storage = "upID", Name = "up_id", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public int UpID
	{
		get
		{
			return upID;
		}
		set
		{
			if (value != upID)
			{
				upID = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string UpLast

	private string upLast;
	[Column(Storage = "upLast", Name = "up_last", DbType = "NVARCHAR(30)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string UpLast
	{
		get
		{
			return upLast;
		}
		set
		{
			if (value != upLast)
			{
				upLast = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string UpFirst

	private string upFirst;
	[Column(Storage = "upFirst", Name = "up_first", DbType = "NVARCHAR(30)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string UpFirst
	{
		get
		{
			return upFirst;
		}
		set
		{
			if (value != upFirst)
			{
				upFirst = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string UpEmail

	private string upEmail;
	[Column(Storage = "upEmail", Name = "up_email", DbType = "NVARCHAR(100)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string UpEmail
	{
		get
		{
			return upEmail;
		}
		set
		{
			if (value != upEmail)
			{
				upEmail = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string UpAirport

	private string upAirport;
	[Column(Storage = "upAirport", Name = "up_airport", DbType = "NCHAR(3)")]
	[DebuggerNonUserCode]
	public string UpAirport
	{
		get
		{
			return upAirport;
		}
		set
		{
			if (value != upAirport)
			{
				upAirport = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#warning L189 table admin.user_profile has no primary key. Multiple C# objects will refer to the same row.
}

[Table(Name = "admin.iietab_f5_f6")]
public partial class IIeTabF5F6 : IModified
{
	// IModified backing field
	public bool IsModified{ get; set; }

	#region string PerKey

	private string perKey;
	[Column(Storage = "perKey", Name = "per_key", DbType = "CHAR(8)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string PerKey
	{
		get
		{
			return perKey;
		}
		set
		{
			if (value != perKey)
			{
				perKey = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region int PerSegment0

	private int perSegment0;
	[Column(Storage = "perSegment0", Name = "per_segment0", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public int PerSegment0
	{
		get
		{
			return perSegment0;
		}
		set
		{
			if (value != perSegment0)
			{
				perSegment0 = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region int PerSegment1

	private int perSegment1;
	[Column(Storage = "perSegment1", Name = "per_segment1", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public int PerSegment1
	{
		get
		{
			return perSegment1;
		}
		set
		{
			if (value != perSegment1)
			{
				perSegment1 = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region int PerNext

	private int perNext;
	[Column(Storage = "perNext", Name = "per_next", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public int PerNext
	{
		get
		{
			return perNext;
		}
		set
		{
			if (value != perNext)
			{
				perNext = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#warning L189 table admin.iietab_f5_f6 has no primary key. Multiple C# objects will refer to the same row.
}

[Table(Name = "admin.version")]
public partial class Version : IModified
{
	// IModified backing field
	public bool IsModified{ get; set; }

	#region int VERID

	private int vERID;
	[Column(Storage = "vERID", Name = "ver_id", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public int VERID
	{
		get
		{
			return vERID;
		}
		set
		{
			if (value != vERID)
			{
				vERID = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region int VERMajor

	private int vERMajor;
	[Column(Storage = "vERMajor", Name = "ver_major", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public int VERMajor
	{
		get
		{
			return vERMajor;
		}
		set
		{
			if (value != vERMajor)
			{
				vERMajor = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region int VERMinor

	private int vERMinor;
	[Column(Storage = "vERMinor", Name = "ver_minor", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public int VERMinor
	{
		get
		{
			return vERMinor;
		}
		set
		{
			if (value != vERMinor)
			{
				vERMinor = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region int VERRelease

	private int vERRelease;
	[Column(Storage = "vERRelease", Name = "ver_release", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public int VERRelease
	{
		get
		{
			return vERRelease;
		}
		set
		{
			if (value != vERRelease)
			{
				vERRelease = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#warning L189 table admin.version has no primary key. Multiple C# objects will refer to the same row.
}

[Table(Name = "admin.country")]
public partial class Country : IModified
{
	// IModified backing field
	public bool IsModified{ get; set; }

	#region int CtID

	private int ctID;
	[Column(Storage = "ctID", Name = "ct_id", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public int CtID
	{
		get
		{
			return ctID;
		}
		set
		{
			if (value != ctID)
			{
				ctID = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string CtCode

	private string ctCode;
	[Column(Storage = "ctCode", Name = "ct_code", DbType = "NCHAR(2)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string CtCode
	{
		get
		{
			return ctCode;
		}
		set
		{
			if (value != ctCode)
			{
				ctCode = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string CtName

	private string ctName;
	[Column(Storage = "ctName", Name = "ct_name", DbType = "NVARCHAR(50)")]
	[DebuggerNonUserCode]
	public string CtName
	{
		get
		{
			return ctName;
		}
		set
		{
			if (value != ctName)
			{
				ctName = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#warning L189 table admin.country has no primary key. Multiple C# objects will refer to the same row.
}

[Table(Name = "admin.airport")]
public partial class Airport : IModified
{
	// IModified backing field
	public bool IsModified{ get; set; }

	#region int ApID

	private int apID;
	[Column(Storage = "apID", Name = "ap_id", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public int ApID
	{
		get
		{
			return apID;
		}
		set
		{
			if (value != apID)
			{
				apID = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string ApIAtAcOde

	private string apIAtAcOde;
	[Column(Storage = "apIAtAcOde", Name = "ap_iatacode", DbType = "NCHAR(3)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string ApIAtAcOde
	{
		get
		{
			return apIAtAcOde;
		}
		set
		{
			if (value != apIAtAcOde)
			{
				apIAtAcOde = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string ApPlace

	private string apPlace;
	[Column(Storage = "apPlace", Name = "ap_place", DbType = "NVARCHAR(30)")]
	[DebuggerNonUserCode]
	public string ApPlace
	{
		get
		{
			return apPlace;
		}
		set
		{
			if (value != apPlace)
			{
				apPlace = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string ApName

	private string apName;
	[Column(Storage = "apName", Name = "ap_name", DbType = "NVARCHAR(50)")]
	[DebuggerNonUserCode]
	public string ApName
	{
		get
		{
			return apName;
		}
		set
		{
			if (value != apName)
			{
				apName = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string ApCcOde

	private string apCcOde;
	[Column(Storage = "apCcOde", Name = "ap_ccode", DbType = "NCHAR(2)")]
	[DebuggerNonUserCode]
	public string ApCcOde
	{
		get
		{
			return apCcOde;
		}
		set
		{
			if (value != apCcOde)
			{
				apCcOde = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#warning L189 table admin.airport has no primary key. Multiple C# objects will refer to the same row.
}

[Table(Name = "admin.route")]
public partial class Route : IModified
{
	// IModified backing field
	public bool IsModified{ get; set; }

	#region int RTID

	private int rTID;
	[Column(Storage = "rTID", Name = "rt_id", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public int RTID
	{
		get
		{
			return rTID;
		}
		set
		{
			if (value != rTID)
			{
				rTID = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string RTAirline

	private string rTAirline;
	[Column(Storage = "rTAirline", Name = "rt_airline", DbType = "NCHAR(3)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string RTAirline
	{
		get
		{
			return rTAirline;
		}
		set
		{
			if (value != rTAirline)
			{
				rTAirline = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region int RTFlightNum

	private int rTFlightNum;
	[Column(Storage = "rTFlightNum", Name = "rt_flight_num", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public int RTFlightNum
	{
		get
		{
			return rTFlightNum;
		}
		set
		{
			if (value != rTFlightNum)
			{
				rTFlightNum = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string RTDepartFrom

	private string rTDepartFrom;
	[Column(Storage = "rTDepartFrom", Name = "rt_depart_from", DbType = "NCHAR(3)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string RTDepartFrom
	{
		get
		{
			return rTDepartFrom;
		}
		set
		{
			if (value != rTDepartFrom)
			{
				rTDepartFrom = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string RTArriveTo

	private string rTArriveTo;
	[Column(Storage = "rTArriveTo", Name = "rt_arrive_to", DbType = "NCHAR(3)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string RTArriveTo
	{
		get
		{
			return rTArriveTo;
		}
		set
		{
			if (value != rTArriveTo)
			{
				rTArriveTo = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region int RTArriveOffset

	private int rTArriveOffset;
	[Column(Storage = "rTArriveOffset", Name = "rt_arrive_offset", DbType = "INTEGER(1)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public int RTArriveOffset
	{
		get
		{
			return rTArriveOffset;
		}
		set
		{
			if (value != rTArriveOffset)
			{
				rTArriveOffset = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string RTFlightDay

	private string rTFlightDay;
	[Column(Storage = "rTFlightDay", Name = "rt_flight_day", DbType = "NCHAR(7)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string RTFlightDay
	{
		get
		{
			return rTFlightDay;
		}
		set
		{
			if (value != rTFlightDay)
			{
				rTFlightDay = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#warning L189 table admin.route has no primary key. Multiple C# objects will refer to the same row.
}

[Table(Name = "admin.airline")]
public partial class Airline : IModified
{
	// IModified backing field
	public bool IsModified{ get; set; }

	#region int ALID

	private int aLID;
	[Column(Storage = "aLID", Name = "al_id", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public int ALID
	{
		get
		{
			return aLID;
		}
		set
		{
			if (value != aLID)
			{
				aLID = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string ALIAtAcOde

	private string aLIAtAcOde;
	[Column(Storage = "aLIAtAcOde", Name = "al_iatacode", DbType = "NCHAR(2)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string ALIAtAcOde
	{
		get
		{
			return aLIAtAcOde;
		}
		set
		{
			if (value != aLIAtAcOde)
			{
				aLIAtAcOde = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string ALIcaOCode

	private string aLIcaOCode;
	[Column(Storage = "aLIcaOCode", Name = "al_icaocode", DbType = "NCHAR(3)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string ALIcaOCode
	{
		get
		{
			return aLIcaOCode;
		}
		set
		{
			if (value != aLIcaOCode)
			{
				aLIcaOCode = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string ALName

	private string aLName;
	[Column(Storage = "aLName", Name = "al_name", DbType = "NVARCHAR(60)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string ALName
	{
		get
		{
			return aLName;
		}
		set
		{
			if (value != aLName)
			{
				aLName = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string ALCcOde

	private string aLCcOde;
	[Column(Storage = "aLCcOde", Name = "al_ccode", DbType = "NCHAR(2)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string ALCcOde
	{
		get
		{
			return aLCcOde;
		}
		set
		{
			if (value != aLCcOde)
			{
				aLCcOde = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#warning L189 table admin.airline has no primary key. Multiple C# objects will refer to the same row.
}

[Table(Name = "admin.flight_day")]
public partial class FlightDay : IModified
{
	// IModified backing field
	public bool IsModified{ get; set; }

	#region string DayMask

	private string dayMask;
	[Column(Storage = "dayMask", Name = "day_mask", DbType = "NCHAR(7)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string DayMask
	{
		get
		{
			return dayMask;
		}
		set
		{
			if (value != dayMask)
			{
				dayMask = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region int DayCode

	private int dayCode;
	[Column(Storage = "dayCode", Name = "day_code", DbType = "INTEGER(2)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public int DayCode
	{
		get
		{
			return dayCode;
		}
		set
		{
			if (value != dayCode)
			{
				dayCode = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string DayName

	private string dayName;
	[Column(Storage = "dayName", Name = "day_name", DbType = "NCHAR(9)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string DayName
	{
		get
		{
			return dayName;
		}
		set
		{
			if (value != dayName)
			{
				dayName = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#warning L189 table admin.flight_day has no primary key. Multiple C# objects will refer to the same row.
}

[Table(Name = "admin.full_route")]
public partial class FullRoute : IModified
{
	// IModified backing field
	public bool IsModified{ get; set; }

	#region string RTAirline

	private string rTAirline;
	[Column(Storage = "rTAirline", Name = "rt_airline", DbType = "NCHAR(3)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string RTAirline
	{
		get
		{
			return rTAirline;
		}
		set
		{
			if (value != rTAirline)
			{
				rTAirline = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string ALIAtAcOde

	private string aLIAtAcOde;
	[Column(Storage = "aLIAtAcOde", Name = "al_iatacode", DbType = "NCHAR(2)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string ALIAtAcOde
	{
		get
		{
			return aLIAtAcOde;
		}
		set
		{
			if (value != aLIAtAcOde)
			{
				aLIAtAcOde = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region int RTFlightNum

	private int rTFlightNum;
	[Column(Storage = "rTFlightNum", Name = "rt_flight_num", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public int RTFlightNum
	{
		get
		{
			return rTFlightNum;
		}
		set
		{
			if (value != rTFlightNum)
			{
				rTFlightNum = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string RTDepartFrom

	private string rTDepartFrom;
	[Column(Storage = "rTDepartFrom", Name = "rt_depart_from", DbType = "NCHAR(3)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string RTDepartFrom
	{
		get
		{
			return rTDepartFrom;
		}
		set
		{
			if (value != rTDepartFrom)
			{
				rTDepartFrom = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string RTArriveTo

	private string rTArriveTo;
	[Column(Storage = "rTArriveTo", Name = "rt_arrive_to", DbType = "NCHAR(3)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string RTArriveTo
	{
		get
		{
			return rTArriveTo;
		}
		set
		{
			if (value != rTArriveTo)
			{
				rTArriveTo = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region int RTArriveOffset

	private int rTArriveOffset;
	[Column(Storage = "rTArriveOffset", Name = "rt_arrive_offset", DbType = "INTEGER(1)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public int RTArriveOffset
	{
		get
		{
			return rTArriveOffset;
		}
		set
		{
			if (value != rTArriveOffset)
			{
				rTArriveOffset = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string RTFlightDay

	private string rTFlightDay;
	[Column(Storage = "rTFlightDay", Name = "rt_flight_day", DbType = "NCHAR(7)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string RTFlightDay
	{
		get
		{
			return rTFlightDay;
		}
		set
		{
			if (value != rTFlightDay)
			{
				rTFlightDay = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string ALName

	private string aLName;
	[Column(Storage = "aLName", Name = "al_name", DbType = "NVARCHAR(60)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string ALName
	{
		get
		{
			return aLName;
		}
		set
		{
			if (value != aLName)
			{
				aLName = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#region string ALCcOde

	private string aLCcOde;
	[Column(Storage = "aLCcOde", Name = "al_ccode", DbType = "NCHAR(2)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string ALCcOde
	{
		get
		{
			return aLCcOde;
		}
		set
		{
			if (value != aLCcOde)
			{
				aLCcOde = value;
				IsModified = true;
			}
		}
	}

	#endregion

	#warning L189 table admin.full_route has no primary key. Multiple C# objects will refer to the same row.
}
