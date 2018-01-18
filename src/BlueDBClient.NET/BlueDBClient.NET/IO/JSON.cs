/*
   Copyright 2018 Grega Mohorko

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.

Project: BlueDBClient.NET
Created: 2018-1-5
Author: GregaMohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDB.Entity;
using Newtonsoft.Json;

namespace BlueDB.IO
{
	/// <summary>
	/// Includes utility functions for encoding/decoding entities to/from Json. You don't need to use methods from this class.
	/// </summary>
	public static class JSON
	{
		/// <summary>
		/// Encodes provided entity to a JSON string.
		/// </summary>
		/// <param name="entity">Entity to be encoded.</param>
		public static string Encode(BlueDBEntity entity)
		{
			return JsonConvert.SerializeObject(entity);
		}

		/// <summary>
		/// Encodes a list of entities to a JSON string.
		/// </summary>
		/// <param name="entities">The entities to be encoded.</param>
		public static string Encode<T>(List<T> entities) where T:BlueDBEntity
		{
			return JsonConvert.SerializeObject(entities);
		}

		/// <summary>
		/// Decodes provided JSON string to an entity. The type of the entity will be determined from the JSON.
		/// </summary>
		/// <param name="json">The JSON encoded string.</param>
		public static BlueDBEntity Decode(string json)
		{
			return Decode<BlueDBEntity>(json);
		}

		/// <summary>
		/// Decodes provided JSON string to an entity.
		/// </summary>
		/// <typeparam name="T">The type of the entity to decode to.</typeparam>
		/// <param name="json">A JSON encoded string.</param>
		public static T Decode<T>(string json) where T:BlueDBEntity
		{
			return JsonConvert.DeserializeObject<T>(json);
		}

		/// <summary>
		/// Decodes provided JSON string to a list of entities.
		/// </summary>
		/// <typeparam name="T">The type of the entities in the list.</typeparam>
		/// <param name="json">A JSON encoded string.</param>
		/// <returns></returns>
		public static List<T> DecodeList<T>(string json) where T:BlueDBEntity
		{
			return JsonConvert.DeserializeObject<List<T>>(json);
		}
	}
}
