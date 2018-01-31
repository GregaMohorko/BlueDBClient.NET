using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

			Console.WriteLine("TESTING FINISHED");
			if(Debugger.IsAttached) {
				Console.ReadKey(true);
			}
		}

		/// <summary>
		/// Tests properties.
		/// </summary>
		private static void RunTest1()
		{
			Console.WriteLine();
			Console.WriteLine("Running Test1 ...");
			Console.WriteLine();
			
			BlueDBLib.Init<Test1.Entity.User>();

			List<Test1.Entity.User> users = Test1.Data.GetDummy();
			
			string json = JsonConvert.SerializeObject(users);
			//Console.WriteLine(JArray.Parse(json).ToString());
			List<Test1.Entity.User> usersDecoded = JsonConvert.DeserializeObject<List<Test1.Entity.User>>(json);
			AssertEqual(users, usersDecoded);
		}

		/// <summary>
		/// Tests ManyToOne and OneToMany fields. Tests serializing <see cref="List{T}"/> and <see cref="Dictionary{TKey, TValue}"/> of entities.
		/// </summary>
		private static void RunTest2()
		{
			Console.WriteLine();
			Console.WriteLine("Running Test2 ...");
			Console.WriteLine();
			
			BlueDBLib.Init<Test2.Entity.User>();

			List<Test2.Entity.User> users = Test2.Data.GetDummy();

			EntityJsonConverter.ResetKeys();

			// version 1: serializing each entity by itself
			var jsonUsers = new List<string>();
			foreach(Test2.Entity.User user in users) {
				string jsonUser = JSON.Encode(user);
				jsonUsers.Add(jsonUser);
			}
			string json1 = $"[{string.Join(",", jsonUsers)}]";
			Console.WriteLine($"Version 1 is {json1.Length} characters long.");
			//Console.WriteLine(JArray.Parse(json1).ToString());
			List<Test2.Entity.User> usersDecoded = JsonConvert.DeserializeObject<List<Test2.Entity.User>>(json1);
			AssertEqual(users, usersDecoded);

			EntityJsonConverter.ResetKeys();

			// version 2: uses List<>
			string json2 = JsonConvert.SerializeObject(users);
			Console.WriteLine($"Version 2 is {json2.Length} characters long.");
			Debug.Assert(json2.Length == 1008);
			Console.WriteLine($"Version 2 is {json1.Length-json2.Length} shorter than version 1.");
			//Console.WriteLine(JArray.Parse(json2).ToString());
			List<Test2.Entity.User> usersDecoded2 = JsonConvert.DeserializeObject<List<Test2.Entity.User>>(json2);
			AssertEqual(users, usersDecoded2);

			EntityJsonConverter.ResetKeys();

			// version 3: uses JSON utility class, which does the same as version 2, but the code is shorter
			string json3 = JSON.Encode(users);
			Debug.Assert(json2.Length == json3.Length);
			Console.WriteLine("Version 3 is the same length as version 2.");
			List<Test2.Entity.User> usersDecoded3 = JSON.DecodeList<Test2.Entity.User>(json3);
			AssertEqual(users, usersDecoded3);
			
			// the type should be determined from the json
			JArray jsonArray = JArray.Parse(json2);
			BlueDBEntity user1 = JSON.Decode(jsonArray[0].ToString());
			Debug.Assert(user1 is Test2.Entity.User);
			Debug.Assert(EntityUtility.AreEqual(user1, users[0]));

			// tests dictionary
			var dictionaryOfUsers = new Dictionary<string, Test2.Entity.User>();
			foreach(var user in users) {
				dictionaryOfUsers.Add(user.Name, user);
			}
			string jsonOfDict = JsonConvert.SerializeObject(dictionaryOfUsers);
			//Console.WriteLine(JObject.Parse(jsonOfDict).ToString());
			Debug.Assert(jsonOfDict.Length == 1045);
			var dictionaryOfUsersDecoded =JsonConvert.DeserializeObject<Dictionary<string,Test2.Entity.User>>(jsonOfDict);
			AssertEqual(dictionaryOfUsers, dictionaryOfUsersDecoded);

			// tests dictionary of entity lists
			var usersCopy = new List<Test2.Entity.User>(users);
			var carsOfUsers = users.Select(user => user.Car).ToList();
			var dictionaryOfLists = new Dictionary<string, List<BlueDBEntity>>
			{
				{ nameof(usersCopy), usersCopy.Cast<BlueDBEntity>().ToList() },
				{ nameof(carsOfUsers), carsOfUsers.Cast<BlueDBEntity>().ToList() }
			};
			jsonOfDict = JsonConvert.SerializeObject(dictionaryOfLists);
			//Console.WriteLine(JObject.Parse(jsonOfDict).ToString());
			Debug.Assert(jsonOfDict.Length == 1110);
			var dictionaryOfListsDecoded = JsonConvert.DeserializeObject<Dictionary<string, List<BlueDBEntity>>>(jsonOfDict);
			AssertEqual(dictionaryOfLists, dictionaryOfListsDecoded);
		}

		/// <summary>
		/// Tests SubEntities.
		/// </summary>
		private static void RunTest3()
		{
			Console.WriteLine();
			Console.WriteLine("Running Test3 ...");
			Console.WriteLine();
			
			BlueDBLib.Init<Test3.Entity.User>();

			List<Test3.Entity.User> users = Test3.Data.GetDummy();

			string json = JSON.Encode(users);
			//Console.WriteLine(JArray.Parse(json).ToString());
			List<Test3.Entity.User> usersDecoded = JSON.DecodeList<Test3.Entity.User>(json);
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

		private static void AssertEqual<T>(Dictionary<string,T> dict1, Dictionary<string,T> dict2) where T:BlueDBEntity
		{
			Debug.Assert(dict1 != null);
			Debug.Assert(dict2 != null);
			Debug.Assert(dict1.Count == dict2.Count);
			for(int i = dict1.Count-1; i >= 0; --i) {
				string key1 = dict1.Keys.ElementAt(i);
				string key2 = dict2.Keys.ElementAt(i);
				Debug.Assert(key1 == key2);
				T entity1 = dict1[key1];
				T entity2 = dict2[key2];
				Debug.Assert(EntityUtility.AreEqual(entity1, entity2));
			}
		}

		private static void AssertEqual<T>(Dictionary<string,List<T>> dict1,Dictionary<string,List<T>> dict2) where T:BlueDBEntity
		{
			Debug.Assert(dict1 != null);
			Debug.Assert(dict2 != null);
			Debug.Assert(dict1.Count == dict2.Count);
			for(int i = dict1.Count - 1; i >= 0; --i) {
				string key1 = dict1.Keys.ElementAt(i);
				string key2 = dict2.Keys.ElementAt(i);
				Debug.Assert(key1 == key2);
				List<T> entityList1 = dict1[key1];
				List<T> entityList2 = dict2[key2];
				AssertEqual(entityList1, entityList2);
			}
		}
	}
}
