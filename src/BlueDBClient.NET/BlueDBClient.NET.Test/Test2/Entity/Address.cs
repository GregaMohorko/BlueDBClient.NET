using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDBClient.NET.Entity;
using BlueDBClient.NET.Entity.Fields;

namespace BlueDBClient.NET.Test.Test2.Entity
{
	class Address:BlueDBEntity
	{
		public static readonly Field StreetField = Field.Register(nameof(Street), typeof(Address));
		public string Street { get; set; }

		public static readonly Field UsersField = Field.Register(nameof(Users), typeof(Address));
		public List<User> Users { get; set; }
	}
}
