using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDBClient.NET.Entity;
using BlueDBClient.NET.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlueDBClient.NET.IO
{
	internal sealed class EntityListJsonConverter : JsonConverter
	{
		/// <summary>
		/// Determines whether this instance can convert the specified object type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		public override bool CanConvert(Type objectType)
		{
			// can only convert if the type is an EntityList
			return ReflectionUtility.IsSubclassOfRawGeneric(typeof(EntityList<>), objectType);
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
