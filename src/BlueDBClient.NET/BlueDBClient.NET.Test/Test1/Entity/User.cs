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
		public static readonly Field StringField = Field.Register(nameof(String), typeof(User));
		public string String { get; set; }

		public static readonly Field IntField = Field.Register(nameof(Int), typeof(User));
		public int? Int { get; set; }

		public static readonly Field FloatField = Field.Register(nameof(Float), typeof(User));
		public float? Float { get; set; }

		public static readonly Field DoubleField = Field.Register(nameof(Double), typeof(User));
		public double? Double { get; set; }

		public static readonly Field DecimalField = Field.Register(nameof(Decimal), typeof(User));
		public decimal? Decimal { get; set; }

		public static readonly Field EnumField = Field.Register(nameof(Enum), typeof(User));
		public UserType? Enum { get; set; }

		public static readonly Field BoolField = Field.Register(nameof(Bool), typeof(User));
		public bool? Bool { get; set; }

		public static readonly Field DateTimeField = Field.Register(nameof(DateTime), typeof(User));
		public DateTime? DateTime { get; set; }

		public static readonly Field TimeSpanField = Field.Register(nameof(TimeSpan), typeof(User));
		public TimeSpan? TimeSpan { get; set; }
	}
}
