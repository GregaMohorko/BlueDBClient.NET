using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDB.Entity;
using BlueDB.Entity.Fields;

namespace BlueDB.Test.Test3.Entity
{
	class User:BlueDBEntity
	{
		public static readonly Field NameField = Field.Register(nameof(Name));
		public string Name { get; set; }

		public static readonly Field AddressField = Field.Register(nameof(Address));
		public Address Address { get; set; }
	}
}
