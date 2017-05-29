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
			// TODO serialize list
			// below line serializes every object on itself
			// has to be done in a way so that there is a common session while serializing all objects of a list
			return JsonConvert.SerializeObject(entities);
		}

		/// <summary>
		/// Encodes provided entity to a JSON string.
		/// </summary>
		/// <param name="entity">Entity to be encoded.</param>
		public static string Encode(BlueDBEntity entity)
		{
			return JsonConvert.SerializeObject(entity);
		}
	}
}
