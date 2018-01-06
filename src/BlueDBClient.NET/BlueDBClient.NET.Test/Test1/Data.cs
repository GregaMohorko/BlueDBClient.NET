using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDB.Test.Test1.Entity;

namespace BlueDB.Test.Test1
{
	static class Data
	{
		public static List<User> GetDummy()
		{
			return new List<User>()
			{
				new User()
				{
					ID=123,
					String="This is some text",
					Int=45678,
					Float=90.12f,
					Double=345.6789,
					Decimal=10.234m,
					Enum=UserType.Admin,
					Bool=true,
					DateTime=DateTime.Parse("2017-5-27 00:12:00"),
					TimeSpan=TimeSpan.Parse("00:13:00")
				},
				new User()
				{
					ID=null,
					String=null,
					Int=null,
					Float=null,
					Double=null,
					Decimal=null,
					Enum=UserType.Editor,
					Bool=false,
					DateTime=DateTime.Parse("2017-5-27"),
					TimeSpan=null
				},
				new User()
				{
					ID=-123,
					String="This is some text again",
					Int=-45678,
					Float=-90.12f,
					Double=-345.6789,
					Decimal=-10.234m,
					Enum=null,
					Bool=null,
					DateTime=null
				}
			};
		}
	}
}
