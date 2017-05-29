using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDBClient.NET.Entity;
using BlueDBClient.NET.Entity.Fields;

namespace BlueDBClient.NET.Test.Test2.Entity
{
	class Car:BlueDBEntity
	{
		public static readonly Field BrandField = Field.Register(nameof(Brand), typeof(Car));
		public string Brand { get; set; }

		public static readonly Field OwnerField = Field.Register(nameof(Owner), typeof(Car));
		public User Owner { get; set; }
	}
}
