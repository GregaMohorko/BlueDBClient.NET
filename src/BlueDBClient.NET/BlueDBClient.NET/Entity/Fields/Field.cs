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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GM.Utility;

namespace BlueDB.Entity.Fields
{
	/// <summary>
	/// Represents a field of a database entity. Use Register method to register a field inside an entity.
	/// </summary>
	public class Field
	{
		/// <summary>
		/// Gets the name of the field.
		/// </summary>
		public string Name => _name;
		private readonly string _name;

		/// <summary>
		/// Gets the type of the field.
		/// </summary>
		public Type Type => _type;
		private readonly Type _type;

		/// <summary>
		/// Gets the type of the entity field, if it represents an entity. If it is a list of entities, returns the type of entity.
		/// </summary>
		public Type TypeOfEntity => _typeOfEntity;
		private readonly Type _typeOfEntity;

		/// <summary>
		/// Gets the type of the entity in which this field is defined.
		/// </summary>
		public Type EntityType => _entityType;
		private readonly Type _entityType;

		/// <summary>
		/// Gets the field data type (property, entity, list).
		/// </summary>
		public FieldType DataType => _dataType;
		private readonly FieldType _dataType;

		/// <summary>
		/// Returns true if this field represents an entity or a list of entities.
		/// </summary>
		public bool IsEntity => _isEntity;
		private readonly bool _isEntity;

		/// <summary>
		/// Determines whether this field is hidden. Hidden fields are almost always ignored when loading etc.. (use this for passwords and so on).
		/// </summary>
		public bool IsHidden => _isHidden;
		private readonly bool _isHidden;

		private Field(string name,Type entityType, bool isHidden)
		{
			_name = name;
			_entityType = entityType;
			_isHidden = isHidden;

			// determine the field type
			_type = ReflectionUtility.GetPropertyType(EntityType, Name);

			// determine the data type
			if(Type.IsSubclassOf(typeof(BlueDBEntity))) {
				_dataType = FieldType.ENTITY;
				_isEntity = true;
				_typeOfEntity = Type;
			}else if(ReflectionUtility.IsGenericList(Type)) {
				_dataType = FieldType.LIST;
				Type typeOfList = ReflectionUtility.GetGenericFirst(Type);
				if(typeOfList.IsSubclassOf(typeof(BlueDBEntity))) {
					_isEntity = true;
					_typeOfEntity = typeOfList;
				}
			}else {
				_dataType = FieldType.PROPERTY;
				_isEntity = false;
			}

			// register the field for the entity type
			BlueDBEntity.RegisterField(EntityType, this);
		}

		/// <summary>
		/// Returns a <see cref="string"/> represeting this field.
		/// </summary>
		public override string ToString()
		{
			return $"{EntityType?.Name}.{Name}";
		}

		/// <summary>
		/// Registers a new field to the specified entity type.
		/// <para>This should only be called inside the entity in which this field is in, because the entity type will be read using reflection.</para>
		/// </summary>
		/// <param name="name">The name of the field.</param>
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Field Register(string name)
		{
			Type entityType = Utility.ReflectionUtility.GetCallingType();
			return new Field(name, entityType, false);
		}

		/// <summary>
		/// Registers a new hidden field to the specified entity type. Hidden fields are almost always ignored when loading etc.. (use this for passwords and so on).
		/// <para>This should only be called inside the entity in which this field is in, because the entity type will be read using reflection.</para>
		/// </summary>
		/// <param name="name">The name of the field.</param>
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Field RegisterHidden(string name)
		{
			Type entityType = Utility.ReflectionUtility.GetCallingType();
			return new Field(name,entityType, true);
		}
	}
}
