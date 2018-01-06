using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDB.Test.Test3.Entity;

namespace BlueDB.Test.Test3
{
	static class Data
	{
		public static List<User> GetDummy()
		{
			Address ljubljana = new Address()
			{
				ID = 1,
				Street = "Ljubljana"
			};
			Address maribor = new Address()
			{
				ID = 2,
				Street = "Maribor"
			};
			Address celje = new Address()
			{
				ID = 3,
				Street = "Celje"
			};

			Student lojzi = new Student()
			{
				ID = 1,
				Name = "Lojzi",
				Address = ljubljana,
				RegistrationNumber = "E1066934"
			};
			ljubljana.Users = new List<User>() { lojzi };
			Student tadej = new Student()
			{
				ID = 2,
				Name = "Tadej",
				Address = maribor,
				RegistrationNumber = "E1068321"
			};
			maribor.Users = new List<User>() { tadej };
			Teacher grega = new Teacher()
			{
				ID=3,
				Name="Grega",
				Address=celje
			};
			celje.Users = new List<User>() { grega };

			return new List<User>() { lojzi, tadej, grega };
		}
	}
}
