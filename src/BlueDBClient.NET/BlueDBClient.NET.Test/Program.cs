using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BlueDBClient.NET.Configuration;
using BlueDBClient.NET.IO;
using BlueDBClient.NET.Utility;
using BlueDBClient.NET.Entity;
using Newtonsoft.Json.Linq;

namespace BlueDBClient.NET.Test
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

			BlueDBClientProperties.Init<Test1.Entity.User>();

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

			BlueDBClientProperties.Init<Test2.Entity.User>();

			List<Test2.Entity.User> users = Test2.Data.GetDummy();

			string json = JsonConvert.SerializeObject(users);
			//Console.WriteLine(JArray.Parse(json).ToString());
			List<Test2.Entity.User> usersDecoded = JsonConvert.DeserializeObject<List<Test2.Entity.User>>(json);
			AssertEqual(users, usersDecoded);
		}

		/// <summary>
		/// Tests SubEntities.
		/// </summary>
		private static void RunTest3()
		{
			Console.WriteLine();
			Console.WriteLine("Running Test3 ...");
			Console.WriteLine();

			BlueDBClientProperties.Init<Test3.Entity.User>();

			List<Test3.Entity.User> users = Test3.Data.GetDummy();

			string json = JsonConvert.SerializeObject(users);
			//Console.WriteLine(JArray.Parse(json).ToString());
			List<Test3.Entity.User> usersDecoded = JsonConvert.DeserializeObject<List<Test3.Entity.User>>(json);
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
