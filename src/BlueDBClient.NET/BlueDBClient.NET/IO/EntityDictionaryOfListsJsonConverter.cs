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
Created: 2018-1-17
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
	/// The <see cref="JsonConverter"/> for <see cref="Dictionary{TKey, TValue}"/>, where TKey is <see cref="string"/> and TValue is a <see cref="List{T}"/> of <see cref="BlueDBEntity"/>.
	/// </summary>
	public sealed class EntityDictionaryOfListsJsonConverter : JsonConverter
	{
		private Type listType;

		/// <summary>
		/// Determines whether this instance can convert the specified object type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		public override bool CanConvert(Type objectType)
		{
			// can only convert if the type is a Dictionary<> of (string, List<BlueDBEntity>)

			if(!ReflectionUtility.IsSubclassOfRawGeneric(typeof(Dictionary<,>), objectType)) {
				// is not a dictionary
				return false;
			}
			Type[] genericTypes = objectType.GetGenericArguments();
			if(genericTypes[0] != typeof(string)) {
				// the key type is not string
				return false;
			}
			if(!genericTypes[1].IsGenericList()) {
				// the value type is not a generic list
				return false;
			}
			listType = genericTypes[1];
			Type listGenericType = listType.GetGenericFirst();
			return listGenericType == typeof(BlueDBEntity) || EntityUtility.IsEntity(listGenericType);
		}

		/// <summary>
		/// Reads the JSON representation of the dictionary of entity lists.
		/// </summary>
		/// <param name="reader">The JsonReader to read from.</param>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="existingValue">The existing value of object being read.</param>
		/// <param name="serializer">The calling serializer.</param>
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jDictionary = JObject.Load(reader);
			var session = new Dictionary<int, BlueDBEntity>();

			var entityListDictionary = (IDictionary)Activator.CreateInstance(objectType);

			foreach(KeyValuePair<string,JToken> entry in jDictionary) {
				var jArray = (JArray)entry.Value;
				IList entityList = EntityListJsonConverter.ReadEntityList(jArray, listType, session);
				entityListDictionary.Add(entry.Key, entityList);
			}

			return entityListDictionary;
		}

		/// <summary>
		/// Writes the JSON representation of the dictionary of entity lists.
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

			var entityListDictionary = (IDictionary)value;
			var session = new Dictionary<Type, Dictionary<int, BlueDBEntity>>();

			writer.WriteStartObject();

			foreach(DictionaryEntry entry in entityListDictionary) {
				var key = (string)entry.Key;
				var entityList = (IList)entry.Value;

				writer.WritePropertyName(key);
				EntityListJsonConverter.WriteEntityList(writer, entityList, serializer, session);
			}

			writer.WriteEndObject();
		}
	}
}
