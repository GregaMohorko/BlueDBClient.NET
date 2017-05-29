using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDBClient.NET.Entity;
using BlueDBClient.NET.Entity.Fields;

namespace BlueDBClient.NET.Utility
{
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
			if(entity1==null || entity2 == null) {
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
				object value1 = ReflectionUtility.GetPropertyValue(entity1, field.Name);
				object value2 = ReflectionUtility.GetPropertyValue(entity2, field.Name);
				if(value1==null && value2 == null) {
					continue;
				}
				if(value2==null || value2 == null) {
					return false;
				}
				switch(field.DataType) {
					case FieldType.Property:
						if(!value1.Equals(value2)) {
							return false;
						}
						break;
					case FieldType.Entity:
						if(!AreEqualInternal((BlueDBEntity)value1, (BlueDBEntity)value2, session)) {
							return false;
						}
						break;
					case FieldType.List:
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
	}
}
