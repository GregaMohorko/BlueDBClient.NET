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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDB.Entity;
using BlueDB.Utility;
using GM.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlueDB.IO
{
	/// <summary>
	/// The <see cref="JsonConverter"/> for <see cref="List{T}"/> of <see cref="BlueDBEntity"/> entities.
	/// </summary>
	public sealed class EntityListJsonConverter : JsonConverter
	{
		/// <summary>
		/// Determines whether this instance can convert the specified object type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		public override bool CanConvert(Type objectType)
		{
			// can only convert if the type is a List<> of entities
			return ReflectionUtility.IsSubclassOfRawGeneric(typeof(List<>), objectType) && EntityUtility.IsEntity(objectType.GetGenericFirst());
		}

		/// <summary>
		/// Reads the JSON representation of the object.
		/// </summary>
		/// <param name="reader">The JsonReader to read from.</param>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="existingValue">The existing value of object being read.</param>
		/// <param name="serializer">The calling serializer.</param>
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JArray jArray = JArray.Load(reader);
			var session = new Dictionary<int, BlueDBEntity>();

			IList entityList = Activator.CreateInstance(objectType) as IList;

			foreach(JObject jObject in jArray) {
				BlueDBEntity entity = EntityJsonConverter.ReadEntity(jObject, session);
				entityList.Add(entity);
			}

			return entityList;
		}

		/// <summary>
		/// Writes the JSON representation of the list.
		/// </summary>
		/// <param name="writer">The JsonWriter to write to.</param>
		/// <param name="value">The value.</param>
		/// <param name="serializer">The calling serializer.</param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if(value == null) {
				serializer.Serialize(writer, null);
				return;
			}

			IList entityList = value as IList;
			var session = new Dictionary<Type, Dictionary<int, BlueDBEntity>>();

			writer.WriteStartArray();

			foreach(BlueDBEntity entity in entityList) {
				EntityJsonConverter.WriteEntity(writer, entity, serializer, session);
			}

			writer.WriteEndArray();
		}
	}
}
