using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDBClient.NET.IO;
using Newtonsoft.Json;

namespace BlueDBClient.NET.Entity
{
	/// <summary>
	/// A list of entities. Use it to package a collection of entities to serialize them in a correct JSON format.
	/// </summary>
	/// <typeparam name="T">The type of the entities in the list.</typeparam>
	[JsonConverter(typeof(EntityListJsonConverter))]
	public class EntityList<T> : List<T> where T:BlueDBEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EntityList{T}"/> class that is empty and has the default initial capacity.
		/// </summary>
		public EntityList() : base() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityList{T}"/> class that is empty and has the specified initial capacity.
		/// </summary>
		/// <param name="capacity">The number of entities that the new list can initially store.</param>
		public EntityList(int capacity) : base(capacity) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityList{T}"/> class that contains entities copied from the specified collection and has sufficient capacity to accommodate the number of entities copied.
		/// </summary>
		/// <param name="collection">The collection whose entities are copied to the new entity list.</param>
		public EntityList(List<T> collection) : base(collection) { }
	}
}
