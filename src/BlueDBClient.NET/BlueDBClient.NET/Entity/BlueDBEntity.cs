using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDBClient.NET.Entity.Fields;
using BlueDBClient.NET.IO;
using Newtonsoft.Json;

namespace BlueDBClient.NET.Entity
{
	/// <summary>
	/// Base class for BlueDB entities.
	/// </summary>
	[JsonConverter(typeof(EntityJsonConverter))]
	public abstract class BlueDBEntity
	{
		// Database fields
		public static readonly Field IDField = Field.Register(nameof(ID), typeof(BlueDBEntity));
		public int? ID { get; set; }

		private static Dictionary<Type, List<Field>> _allFields;
		private static Dictionary<Type,List<Field>> allFields
		{
			get
			{
				if(_allFields == null)
					_allFields = new Dictionary<Type, List<Field>>();
				return _allFields;
			}
		}

		#region PUBLIC PROPERTIES
		/// <summary>
		/// Determines whether this entity is present in the database or not. Basically, it returns true if ID>0.
		/// </summary>
		public bool IsPersistent { get { return ID > 0; } }
		#endregion // Public Properties

		#region OBJECT
		/// <summary>
		/// Determines whether the provided entity is equal to the current entity. True when they are the same type and have the same ID.
		/// </summary>
		/// <param name="entity">The entity to compare with the current entity.</param>
		public override bool Equals(object entity)
		{
			if(entity == null || GetType() != entity.GetType()) {
				return false;
			}

			return this == (entity as BlueDBEntity);
		}
		
		/// <summary>
		/// Gets the ID of the entity.
		/// </summary>
		public override int GetHashCode()
		{
			return ID.HasValue ? ID.Value : 0;
		}

		/// <summary>
		/// Returns a string that represents the current entity.
		/// </summary>
		public override string ToString()
		{
			return $"[{ID}] {GetType().Name}";
		}

		#region OBJECT -> OPERATORS
		/// <summary>
		/// Determines whether both entities are considered equal. True when they are the same type and have the same ID.
		/// </summary>
		public static bool operator ==(BlueDBEntity entity1,BlueDBEntity entity2)
		{
			// if both are null
			if((object)entity1==null && (object)entity2 == null) {
				return true;
			}
			// if any of them is null
			if((object)entity1 == null || (object)entity2 == null) {
				return false;
			}

			// if they are not the same type
			if(!entity1.GetType().Equals(entity2.GetType())) {
				return false;
			}

			// if they are the same instance
			if(ReferenceEquals(entity1, entity2)) {
				return true;
			}

			// if any of them is not persistent, they are not the same (even if both are not, because the check for the same instance was already done)
			if(!entity1.IsPersistent || !entity2.IsPersistent) {
				return false;
			}

			// if IDs are the same
			return entity1.ID == entity2.ID;
		}

		/// <summary>
		/// Determines whether both entities are not considered equal. True when they are not the same type or have a different ID.
		/// </summary>
		public static bool operator !=(BlueDBEntity entity1,BlueDBEntity entity2)
		{
			return !(entity1 == entity2);
		}
		#endregion // Object -> Operators

		#endregion // Object

		#region PUBLIC STATIC UTILITY
		/// <summary>
		/// Gets a list of all fields of the specified type.
		/// </summary>
		/// <typeparam name="T">The type of which to get the fields.</typeparam>
		/// <param name="includeInherited">Determines whether to include fields of parent classes of the specified type.</param>
		public static List<Field> GetAllFields<T>(bool includeInherited=true) where T:BlueDBEntity
		{
			return GetAllFields(typeof(T));
		}

		/// <summary>
		/// Gets a list of all fields of the specified type.
		/// </summary>
		/// <param name="type">The type of which to get the fields.</param>
		/// <param name="includeInherited">Determines whether to include fields of parent classes of the specified type.</param>
		public static List<Field> GetAllFields(Type type,bool includeInherited=true)
		{
			bool isBaseEntityClass = type.Equals(typeof(BlueDBEntity));
			if(!type.IsSubclassOf(typeof(BlueDBEntity)) && !isBaseEntityClass) {
				throw new ArgumentException($"The provided type '{type}' is not a subclass of BlueDBEntity.");
			}
			List<Field> fields;
			if(allFields.ContainsKey(type)) {
				fields = new List<Field>(allFields[type]);
			}else {
				// specified type has no fields
				fields = new List<Field>();
			}
			if(!isBaseEntityClass) {
				if(type.BaseType.Equals(typeof(BlueDBEntity))) {
					// it is a StrongEntity, only add fields of the base entity
					fields.AddRange(GetAllFields<BlueDBEntity>());
				} else if(includeInherited) {
					// is a SubEntity, include fields of parents
					fields.AddRange(GetAllFields(type.BaseType, true));
				}
			}
			return fields;
		}
		#endregion // Public Static Utility

		#region INTERNAL STATIC UTILITY
		internal static void RegisterField(Type entityType, Field field)
		{
			if(!entityType.Equals(typeof(BlueDBEntity)) && !entityType.IsSubclassOf(typeof(BlueDBEntity))) {
				throw new ArgumentException($"Cannot register field '{field.Name}' because the type '{entityType}' is not a subclass of BlueDBEntity.");
			}
			List<Field> fieldsOfEntityType;
			if(allFields.ContainsKey(entityType)) {
				fieldsOfEntityType = allFields[entityType];
			}else {
				fieldsOfEntityType = new List<Field>();
				allFields.Add(entityType, fieldsOfEntityType);
			}
			fieldsOfEntityType.Add(field);
		}

		internal static void ClearAllFields()
		{
			List<Field> baseFields = allFields[typeof(BlueDBEntity)];
			allFields.Clear();
			allFields.Add(typeof(BlueDBEntity), baseFields);
		}
		#endregion // Internal Static Utility
	}
}
