using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDBClient.NET.Utility;

namespace BlueDBClient.NET.Entity.Fields
{
	/// <summary>
	/// Represents a field of a database entity. Use Register method to register a field inside an entity.
	/// </summary>
	public class Field
	{
		/// <summary>
		/// Gets the name of the field.
		/// </summary>
		public string Name { get { return _name; } }
		private readonly string _name;

		/// <summary>
		/// Gets the type of the field.
		/// </summary>
		public Type Type { get { return _type; } }
		private readonly Type _type;

		/// <summary>
		/// Gets the type of the entity field, if it represents an entity. If it is a list of entities, returns the type of entity.
		/// </summary>
		public Type TypeOfEntity { get { return _typeOfEntity; } }
		private readonly Type _typeOfEntity;

		/// <summary>
		/// Gets the type of the entity.
		/// </summary>
		public Type EntityType { get { return _entityType; } }
		private readonly Type _entityType;

		/// <summary>
		/// Gets the field data type (property, entity, list).
		/// </summary>
		public FieldType DataType { get { return _dataType; } }
		private readonly FieldType _dataType;

		/// <summary>
		/// Returns true if this field represents an entity or a list of entities.
		/// </summary>
		public bool IsEntity { get { return _isEntity; } }
		private readonly bool _isEntity;

		protected Field(string name,Type entityType)
		{
			_name = name;
			_entityType = entityType;

			// determine the field type
			_type = ReflectionUtility.GetPropertyType(EntityType, Name);

			// determine the data type
			if(Type.IsSubclassOf(typeof(BlueDBEntity))) {
				_dataType = FieldType.Entity;
				_isEntity = true;
				_typeOfEntity = Type;
			}else if(ReflectionUtility.IsGenericList(Type)) {
				_dataType = FieldType.List;
				Type typeOfList = ReflectionUtility.GetGenericFirst(Type);
				if(typeOfList.IsSubclassOf(typeof(BlueDBEntity))) {
					_isEntity = true;
					_typeOfEntity = typeOfList;
				}
			}else {
				_dataType = FieldType.Property;
				_isEntity = false;
			}

			// register the field for the entity type
			BlueDBEntity.RegisterField(EntityType, this);
		}

		public override string ToString()
		{
			return $"{EntityType?.Name}.{Name}";
		}

		/// <summary>
		/// Registers a new field to the specified entity type.
		/// </summary>
		/// <param name="name">The name of the field.</param>
		/// <param name="entityType">The type of the entity.</param>
		public static Field Register(string name, Type entityType)
		{
			return new Field(name, entityType);
		}
	}
}
