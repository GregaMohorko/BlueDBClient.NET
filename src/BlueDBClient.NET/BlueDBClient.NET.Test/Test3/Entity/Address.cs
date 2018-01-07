using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDB.Entity;
using BlueDB.Entity.Fields;

namespace BlueDB.Test.Test3.Entity
{
	class Address:BlueDBEntity
	{
		public static readonly Field StreetField = Field.Register(nameof(Street));
		public string Street { get; set; }

		public static readonly Field UsersField = Field.Register(nameof(Users));
		public List<User> Users { get; set; }
	}
}
