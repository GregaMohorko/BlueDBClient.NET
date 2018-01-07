using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDB.Entity;
using BlueDB.Entity.Fields;

namespace BlueDB.Test.Test2.Entity
{
	class Car:BlueDBEntity
	{
		public static readonly Field BrandField = Field.Register(nameof(Brand));
		public string Brand { get; set; }

		public static readonly Field OwnerField = Field.Register(nameof(Owner));
		public User Owner { get; set; }
	}
}
