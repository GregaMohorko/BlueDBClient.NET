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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BlueDB.Entity;
using BlueDB.Entity.Fields;
using GM.Utility;

namespace BlueDB.Utility
{
	/// <summary>
	/// A class with various entity utilities.
	/// </summary>
	public static class EntityUtility
	{
		/// <summary>
		/// Determines whether both entities are equal. They must be of the same type. This function searches in depth to the end, so it can be slow.
		/// </summary>
		/// <param name="entity1">First entity.</param>
		/// <param name="entity2">Second entity.</param>
		public static bool AreEqual(BlueDBEntity entity1,BlueDBEntity entity2)
		{
			var session = new Dictionary<Type, List<Tuple<BlueDBEntity, BlueDBEntity>>>();
			return AreEqualInternal(entity1, entity2, session);
		}

		private static bool AreEqualInternal(BlueDBEntity entity1,BlueDBEntity entity2,Dictionary<Type,List<Tuple<BlueDBEntity,BlueDBEntity>>> session)
		{
			if(entity1==null && entity2 == null) {
				return true;
			}
			if(entity1==null || entity2 == null) {
				return false;
			}
			if(ReferenceEquals(entity1, entity2)) {
				// the same reference
				return true;
			}
			Type type1 = entity1.GetType();
			Type type2 = entity2.GetType();
			if(type1 != type2) {
				return false;
			}

			// check if they were already compared
			if(session.ContainsKey(type1)) {
				foreach(var pair in session[type1]) {
					if((ReferenceEquals(pair.Item1,entity1) && ReferenceEquals(pair.Item2,entity2)) ||
						(ReferenceEquals(pair.Item1,entity2) && ReferenceEquals(pair.Item2, entity1))) {
						// found the pair
						return true;
					}
				}
			}else {
				session.Add(type1, new List<Tuple<BlueDBEntity, BlueDBEntity>>());
			}

			// add the pair to the session
			session[type1].Add(Tuple.Create(entity1, entity2));

			List<Field> fieldList = BlueDBEntity.GetAllFields(type1, true);

			foreach(Field field in fieldList) {
				object value1 = entity1.GetPropertyValue(field.Name);
				object value2 = entity2.GetPropertyValue(field.Name);
				if(value1==null && value2 == null) {
					continue;
				}
				if(value2==null || value2 == null) {
					return false;
				}
				switch(field.DataType) {
					case FieldType.PROPERTY:
						if(!value1.Equals(value2)) {
							return false;
						}
						break;
					case FieldType.ENTITY:
						if(!AreEqualInternal((BlueDBEntity)value1, (BlueDBEntity)value2, session)) {
							return false;
						}
						break;
					case FieldType.LIST:
						IList list1 = (IList)value1;
						IList list2 = (IList)value2;
						if(list1.Count != list2.Count) {
							return false;
						}
						for(int i = list1.Count - 1; i >= 0; --i) {
							if(!AreEqualInternal((BlueDBEntity)list1[i], (BlueDBEntity)list2[i],session)) {
								return false;
							}
						}
						break;
					default:
						throw new Exception($"Unsupported field data type '{field.DataType}'.");
				}
			}

			return true;
		}

		/// <summary>
		/// Determines whether the specified type is an entity (a subclass of <see cref="BlueDBEntity"/>).
		/// </summary>
		/// <param name="type">The type to check.</param>
		public static bool IsEntity(Type type)
		{
			return type.IsSubclassOf(typeof(BlueDBEntity));
		}

		/// <summary>
		/// Determines whether the specified entity type is a strong entity.
		/// </summary>
		/// <param name="entityType">The entity type to determine whether it is a strong entity.</param>
		public static bool IsStrongEntity(Type entityType)
		{
			if(entityType.BaseType == typeof(BlueDBEntity)) {
				return true;
			}
			if(IsEntity(entityType)) {
				// is esentually an entity
				// check if it has anly non-table entity classes before the BlueDBEntity
				Type currentBaseType = entityType.BaseType;
				while(true) {
					if(currentBaseType == typeof(BlueDBEntity)) {
						return true;
					}
					if(!IsNonTableEntityClass(currentBaseType)) {
						break;
					}
					currentBaseType = currentBaseType.BaseType;
				}
			}
			return false;
		}

		/// <summary>
		/// Determines whether the specified entity type is a sub entity.
		/// </summary>
		/// <param name="entityType">The entity type to determine whether it is a sub entity.</param>
		public static bool IsSubEntity(Type entityType)
		{
			return IsEntity(entityType) && !IsStrongEntity(entityType);
		}

		/// <summary>
		/// Determines whether the specified type is a non-table entity class (implements <see cref="INonTableEntityClass{TNonTableEntity}"/>).
		/// </summary>
		/// <param name="type">The type to determine whether it is a non-table entity class.</param>
		public static bool IsNonTableEntityClass(Type type)
		{
			Type nonTableInterface = typeof(INonTableEntityClass<>).MakeGenericType(type);
			return nonTableInterface.IsAssignableFrom(type);
		}

		/// <summary>
		/// Gets the parent entity type of the specified sub entity type.
		/// </summary>
		/// <param name="subEntityType">The sub entity type of which to get the parent entity type.</param>
		public static Type GetParentEntity(Type subEntityType)
		{
			Type currentType = subEntityType.BaseType;
			while(IsNonTableEntityClass(currentType)) {
				currentType = currentType.BaseType;
			}
			return currentType;
		}
	}
}
