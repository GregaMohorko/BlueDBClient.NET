using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDBClient.NET.Entity;
using BlueDBClient.NET.Entity.Fields;

namespace BlueDBClient.NET.Test.Test3.Entity
{
	class User:BlueDBEntity
	{
		public static readonly Field NameField = Field.Register(nameof(Name), typeof(User));
		public string Name { get; set; }

		public static readonly Field AddressField = Field.Register(nameof(Address), typeof(User));
		public Address Address { get; set; }
	}
}
