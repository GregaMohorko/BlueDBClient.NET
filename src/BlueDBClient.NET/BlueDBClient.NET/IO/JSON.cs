using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDBClient.NET.Entity;
using Newtonsoft.Json;

namespace BlueDBClient.NET.IO
{
	/// <summary>
	/// Includes utility functions for encoding/decoding entities to/from Json.
	/// </summary>
	public static class JSON
	{
		/// <summary>
		/// Encodes a list of entities to a JSON string.
		/// </summary>
		/// <param name="entities">The entities to be encoded.</param>
		public static string Encode<T>(List<T> entities) where T:BlueDBEntity
		{
			EntityList<T> entityList;
			if(entities is EntityList<T>) {
				entityList = entities as EntityList<T>;
			}else {
				entityList = new EntityList<T>(entities);
			}

			return JsonConvert.SerializeObject(entityList);
		}

		/// <summary>
		/// Encodes provided entity to a JSON string.
		/// </summary>
		/// <param name="entity">Entity to be encoded.</param>
		public static string Encode(BlueDBEntity entity)
		{
			return JsonConvert.SerializeObject(entity);
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
		public static EntityList<T> DecodeList<T>(string json) where T:BlueDBEntity
		{
			return JsonConvert.DeserializeObject<EntityList<T>>(json);
		}
	}
}
