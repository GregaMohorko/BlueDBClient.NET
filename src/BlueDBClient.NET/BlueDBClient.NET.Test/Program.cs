using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BlueDB.Configuration;
using BlueDB.IO;
using BlueDB.Utility;
using BlueDB.Entity;

namespace BlueDB.Test
{
	class Program
	{
		static void Main(string[] args)
		{
			RunTest1();
			RunTest2();
			RunTest3();

			Console.Write("TESTING FINISHED");
			Console.ReadKey(true);
		}

		/// <summary>
		/// Tests properties.
		/// </summary>
		private static void RunTest1()
		{
			Console.WriteLine();
			Console.WriteLine("Running Test1 ...");
			Console.WriteLine();

			BlueDBProperties.Init<Test1.Entity.User>();

			List<Test1.Entity.User> users = Test1.Data.GetDummy();
			
			string json = JsonConvert.SerializeObject(users);
			//Console.WriteLine(JArray.Parse(json).ToString());
			List<Test1.Entity.User> usersDecoded = JsonConvert.DeserializeObject<List<Test1.Entity.User>>(json);
			AssertEqual(users, usersDecoded);
		}

		/// <summary>
		/// Tests ManyToOne and OneToMany fields.
		/// </summary>
		private static void RunTest2()
		{
			Console.WriteLine();
			Console.WriteLine("Running Test2 ...");
			Console.WriteLine();

			BlueDBProperties.Init<Test2.Entity.User>();

			List<Test2.Entity.User> users = Test2.Data.GetDummy();

			// version 1: uses raw List<>
			string json1 = JsonConvert.SerializeObject(users);
			Console.WriteLine($"Version 1 is {json1.Length} characters long.");
			//Console.WriteLine(JArray.Parse(json).ToString());
			List<Test2.Entity.User> usersDecoded = JsonConvert.DeserializeObject<List<Test2.Entity.User>>(json1);
			AssertEqual(users, usersDecoded);

			// version 2: uses EntityList<>
			EntityList<Test2.Entity.User> usersEntityList = new EntityList<Test2.Entity.User>(users);
			string json2 = JsonConvert.SerializeObject(usersEntityList);
			Console.WriteLine($"Version 2 is {json2.Length} characters long.");
			Console.WriteLine($"Version 2 is {json1.Length-json2.Length} characters shorter than version 1.");
			//Console.WriteLine(JArray.Parse(json2).ToString());
			EntityList<Test2.Entity.User> usersEntityListDecoded = JsonConvert.DeserializeObject<EntityList<Test2.Entity.User>>(json2);
			AssertEqual(users, usersEntityListDecoded);

			// version 3: uses JSON utility class, which does the same as version 2, but the code is shorter. Uses raw List<>
			string json3 = JSON.Encode(users);
			Debug.Assert(json2.Length == json3.Length);
			Console.WriteLine("Version 3 is the same length as version 2.");
			usersEntityListDecoded = JSON.DecodeList<Test2.Entity.User>(json3);
			AssertEqual(users, usersEntityListDecoded);
			
			// the type should be determined from the json
			JArray jsonArray = JArray.Parse(json3);
			BlueDBEntity user1 = JSON.Decode(jsonArray[0].ToString());
			Debug.Assert(user1 is Test2.Entity.User);
			Debug.Assert(EntityUtility.AreEqual(user1, users[0]));
		}

		/// <summary>
		/// Tests SubEntities.
		/// </summary>
		private static void RunTest3()
		{
			Console.WriteLine();
			Console.WriteLine("Running Test3 ...");
			Console.WriteLine();

			BlueDBProperties.Init<Test3.Entity.User>();

			List<Test3.Entity.User> users = Test3.Data.GetDummy();

			string json = JSON.Encode(users);
			//Console.WriteLine(JArray.Parse(json).ToString());
			EntityList<Test3.Entity.User> usersDecoded = JSON.DecodeList<Test3.Entity.User>(json);
			AssertEqual(users, usersDecoded);
		}

		private static void AssertEqual<T>(List<T> list1,List<T> list2) where T:BlueDBEntity
		{
			Debug.Assert(list1 != null);
			Debug.Assert(list2 != null);
			Debug.Assert(list1.Count == list2.Count);
			for(int i = list1.Count - 1; i >= 0; --i) {
				T entity1 = list1[i];
				T entity2 = list2[i];
				Debug.Assert(EntityUtility.AreEqual(entity1, entity2));
			}
		}
	}
}
