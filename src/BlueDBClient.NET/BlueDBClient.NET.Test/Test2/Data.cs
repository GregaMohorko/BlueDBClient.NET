using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDBClient.NET.Test.Test2.Entity;

namespace BlueDBClient.NET.Test.Test2
{
	static class Data
	{
		public static List<User> GetDummy()
		{
			Address rapture = new Address()
			{
				ID = 1,
				Street = "Rapture"
			};
			Address gotham = new Address()
			{
				ID = 2,
				Street = "Gotham"
			};
			Address citadel = new Address()
			{
				ID=3,
				Street="Citadel"
			};

			User ryan = new User()
			{
				ID = 1,
				Name = "Ryan",
				Address = rapture,
				BestFriendTo=new List<User>()
			};
			rapture.Users = new List<User>() { ryan };
			User bruce = new User()
			{
				ID = 2,
				Name = "Bruce",
				Address = gotham,
				BestFriend = ryan,
				BestFriendTo = new List<User>()
			};
			gotham.Users = new List<User>() { bruce };
			ryan.BestFriendTo.Add(bruce);
			User john = new User()
			{
				ID = 3,
				Name = "John",
				Address = citadel,
				BestFriend= ryan,
				BestFriendTo = new List<User>() { ryan }
			};
			ryan.BestFriend = john;
			citadel.Users = new List<User>() { john };
			ryan.BestFriendTo.Add(john);

			Car ford = new Car()
			{
				ID = 1,
				Brand = "Ford",
				Owner = ryan
			};
			ryan.Car = ford;
			Car tank = new Car()
			{
				ID = 2,
				Brand = "Tank",
				Owner = bruce
			};
			bruce.Car = tank;
			Car normandy = new Car()
			{
				ID = 3,
				Brand = "Normandy",
				Owner = john
			};
			john.Car = normandy;

			return new List<User>() { ryan, bruce, john };
		}
	}
}
