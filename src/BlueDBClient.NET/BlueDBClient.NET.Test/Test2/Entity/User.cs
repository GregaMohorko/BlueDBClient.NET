using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDB.Entity;
using BlueDB.Entity.Fields;

namespace BlueDB.Test.Test2.Entity
{
	class User : BlueDBEntity
	{
		public static readonly Field NameField = Field.Register(nameof(Name), typeof(User));
		public string Name { get; set; }

		public static readonly Field AddressField = Field.Register(nameof(Address), typeof(User));
		public Address Address { get; set; }

		public static readonly Field CarField = Field.Register(nameof(Car), typeof(User));
		public Car Car { get; set; }

		public static readonly Field BestFriendField = Field.Register(nameof(BestFriend), typeof(User));
		public User BestFriend { get; set; }

		public static readonly Field BestFriendToField = Field.Register(nameof(BestFriendTo), typeof(User));
		public List<User> BestFriendTo { get; set; }
	}
}
