using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDB.Entity;
using BlueDB.Entity.Fields;

namespace BlueDB.Test.Test1.Entity
{
	/// <summary>
	/// Example entity class with all possible property field types.
	/// </summary>
	class User:BlueDBEntity
	{
		public static readonly Field StringField = Field.Register(nameof(String));
		public string String { get; set; }

		public static readonly Field IntField = Field.Register(nameof(Int));
		public int? Int { get; set; }

		public static readonly Field FloatField = Field.Register(nameof(Float));
		public float? Float { get; set; }

		public static readonly Field DoubleField = Field.Register(nameof(Double));
		public double? Double { get; set; }

		public static readonly Field DecimalField = Field.Register(nameof(Decimal));
		public decimal? Decimal { get; set; }

		public static readonly Field EnumField = Field.Register(nameof(Enum));
		public UserType? Enum { get; set; }

		public static readonly Field BoolField = Field.Register(nameof(Bool));
		public bool? Bool { get; set; }

		public static readonly Field DateTimeField = Field.Register(nameof(DateTime));
		public DateTime? DateTime { get; set; }

		public static readonly Field TimeSpanField = Field.Register(nameof(TimeSpan));
		public TimeSpan? TimeSpan { get; set; }
	}
}
