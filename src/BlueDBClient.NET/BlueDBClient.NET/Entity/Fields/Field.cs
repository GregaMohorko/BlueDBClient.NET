/*
   Copyright 2020 Gregor Mohorko

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
Author: Gregor Mohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BlueDB.Utility;
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
		public string Name { get; }
		/// <summary>
		/// Gets the type of the field.
		/// </summary>
		public Type Type { get; }
		/// <summary>
		/// Gets the type of the entity field, if it represents an entity. If it is a list of entities, returns the type of entity.
		/// </summary>
		public Type TypeOfEntity { get; }
		/// <summary>
		/// Gets the type of the entity in which this field is defined.
		/// </summary>
		public Type EntityType { get; }
		/// <summary>
		/// Gets the field data type (property, entity, list).
		/// </summary>
		public FieldType DataType { get; }
		/// <summary>
		/// Returns true if this field represents an entity or a list of entities.
		/// </summary>
		public bool IsEntity { get; }
		/// <summary>
		/// Determines whether this field is hidden. Hidden fields are almost always ignored when loading etc.. (use this for passwords and so on).
		/// </summary>
		public bool IsHidden { get; }

		private Field(string name,Type entityType, bool isHidden)
		{
			Name = name;
			EntityType = entityType;
			IsHidden = isHidden;

			// determine the field type
			Type = ReflectionUtility.GetPropertyType(EntityType, Name);

			// determine the data type
			if(EntityUtility.IsEntity(Type)) {
				DataType = FieldType.ENTITY;
				IsEntity = true;
				TypeOfEntity = Type;
			}else if(ReflectionUtility.IsGenericList(Type)) {
				DataType = FieldType.LIST;
				Type typeOfList = ReflectionUtility.GetGenericFirst(Type);
				if(typeOfList.IsSubclassOf(typeof(BlueDBEntity))) {
					IsEntity = true;
					TypeOfEntity = typeOfList;
				}
			}else {
				DataType = FieldType.PROPERTY;
				IsEntity = false;
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
			Type entityType = ReflectionUtility.GetCallingType();
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
			Type entityType = ReflectionUtility.GetCallingType();
			return new Field(name,entityType, true);
		}
	}
}
