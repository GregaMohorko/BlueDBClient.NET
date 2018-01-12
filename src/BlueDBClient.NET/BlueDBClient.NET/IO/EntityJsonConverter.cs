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
using System.Text;
using System.Threading.Tasks;
using BlueDB.Configuration;
using BlueDB.Entity;
using BlueDB.Entity.Fields;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GM.Utility;
using BlueDB.Utility;

namespace BlueDB.IO
{
	internal sealed class EntityJsonConverter : JsonConverter
	{
		private static readonly object lock_KEYCounter = new object();
		private static volatile int _KEYCounter = 0;
		private static int GetNextKEY()
		{
			lock(lock_KEYCounter) {
				int nextKey = _KEYCounter;
				++_KEYCounter;
				return nextKey;
			}
		}

		/// <summary>
		/// Determines whether this instance can convert the specified object type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		public override bool CanConvert(Type objectType)
		{
			// can only convert if the type is an entity
			return objectType.IsSubclassOf(typeof(BlueDBEntity));
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
			// Load JObject from stream
			JObject jObject = JObject.Load(reader);
			var session = new Dictionary<int, BlueDBEntity>();

			return ReadEntity(jObject, session);
		}

		/// <summary>
		/// Writes the JSON representation of the object.
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

			BlueDBEntity entity = value as BlueDBEntity;
			var session = new Dictionary<Type, Dictionary<int, BlueDBEntity>>();

			WriteEntityPrivate(writer, entity, serializer, session);
		}

		internal static BlueDBEntity ReadEntity(JObject jObject,Dictionary<int,BlueDBEntity> session)
		{
			int key = ReadKey(jObject);
			BlueDBEntity entityObject = ReadType(jObject, key, out Type entityType, session);
			if(entityObject != null) {
				// was found in the session by key
				return entityObject;
			}

			// create the entity
			entityObject = (BlueDBEntity)Activator.CreateInstance(entityType,true);
			// add to lookup table
			session.Add(key, entityObject);

			JObject currentJObject = jObject;
			Type currentType = entityType;
			JObject parentJObject = null;
			while(true) {
				// resolve properties
				JObject propertiesObject;
				{
					JToken properties = currentJObject["Properties"];
					if(properties == null) {
						throw new Exception("Bad JSON format: no Properties.");
					}
					propertiesObject = properties as JObject;
					if(propertiesObject == null) {
						throw new Exception("Bad JSON format: Properties is not an object.");
					}
				}

				List<Field> fields = BlueDBEntity.GetAllFields(currentType, false, true);
				
				foreach(var property in propertiesObject) {
					string propertyName = property.Key;
					JToken jPropertyValue = property.Value;

					Field field = fields.FirstOrDefault(f => f.Name == propertyName);
					if(field == null) {
						// is a SubEntity
						if(jPropertyValue.Type != JTokenType.Object) {
							throw new Exception($"Bad JSON format: Property '{propertyName}' should be the parent entity, but it is not an object.");
						}
						parentJObject = jPropertyValue.Value<JObject>();
						continue;
					}

					object propertyValue;
					switch(field.DataType) {
						case FieldType.PROPERTY:
							switch(jPropertyValue.Type) {
								case JTokenType.String:
									if(field.Type == typeof(TimeSpan)) {
										//propertyValue = TimeSpan.Parse((jPropertyValue.Value<string>()));
										propertyValue = jPropertyValue.Value<TimeSpan?>();
									} else if(field.Type == typeof(DateTime)) {
										propertyValue= jPropertyValue.Value<DateTime?>();
									} else {
										propertyValue = jPropertyValue.Value<string>();
									}
									break;
								case JTokenType.Integer:
									if(field.Type == typeof(int)) {
										propertyValue = jPropertyValue.Value<int?>();
									}else if(field.Type == typeof(long)) {
										propertyValue = jPropertyValue.Value<long?>();
									} else if(field.Type.IsEnum) {
										propertyValue = jPropertyValue.Value<long?>();
										propertyValue = Enum.ToObject(field.Type, propertyValue);
									} else {
										throw new Exception($"Field '{field.Name}' was of wrong type: should be either int, long or enum.");
									}
									break;
								case JTokenType.Float:
									if(field.Type == typeof(float)) {
										propertyValue = jPropertyValue.Value<float?>();
									}else if(field.Type == typeof(double)) {
										propertyValue = jPropertyValue.Value<double?>();
									}else if(field.Type == typeof(decimal)) {
										propertyValue = jPropertyValue.Value<decimal?>();
									}else {
										throw new Exception($"Field '{field.Name}' was of wrong type: should be either float, double or decimal.");
									}
									break;
								case JTokenType.Boolean:
									propertyValue = jPropertyValue.Value<bool?>();
									break;
								case JTokenType.Date:
									propertyValue = jPropertyValue.Value<DateTime?>();
									break;
								case JTokenType.Null:
									propertyValue = null;
									break;
								default:
									throw new Exception($"Unsupported JSON type '{jPropertyValue.Type}'.");
							}
							break;
						case FieldType.ENTITY:
							propertyValue=ReadEntity((JObject)jPropertyValue, session);
							break;
						case FieldType.LIST:
							JArray array = jPropertyValue as JArray;
							IList list = Activator.CreateInstance(field.Type, array.Count) as IList;
							foreach(JToken item in array) {
								BlueDBEntity entityItem = ReadEntity((JObject)item, session);
								list.Add(entityItem);
							}
							propertyValue = list;
							break;
						default:
							throw new Exception($"Unsupported field data type '{field.DataType}'.");
					}
					
					entityObject.SetProperty(field.Name, propertyValue);
				}
				
				if(EntityUtility.IsStrongEntity(currentType)) {
					// no more parent entities
					break;
				}
				if(parentJObject == null) {
					throw new Exception($"Bad JSON format: no object for parent in '{currentType.Name}' entity.");
				}

				int parentKey = ReadKey(parentJObject);
				BlueDBEntity parentEntity = ReadType(parentJObject, parentKey, out Type parentType, session);
				if(parentEntity != null) {
					// what? parent entity should be null!
					// are two different entities sharing the same parent?
					// hmmmm... that is wrong
					throw new Exception("Bad format: two different entities are sharing the same parent. That should not happen, that is a wrong design.");
				}
				if(parentType != EntityUtility.GetParentEntity(currentType)) {
					throw new Exception($"Bad JSON format: parent type of '{currentType.Name}' should be '{currentType.BaseType.Name}', but it is '{parentType.Name}'.");
				}
				currentType = parentType;
				currentJObject = parentJObject;
				parentJObject = null;
			}

			return entityObject;
		}

		private static int ReadKey(JObject jObject)
		{
			JToken jKey = jObject["Key"];
			if(jKey == null) {
				throw new Exception("Bad JSON format: no Key in an entity object.");
			}
			return jKey.Value<int>();
		}

		/// <summary>
		/// If it doesn't return null, that means that an entity of the same type with the same key was already decoded and was found in the session and returned.
		/// </summary>
		private static BlueDBEntity ReadType(JObject jObject, int key, out Type entityType, Dictionary<int, BlueDBEntity> session)
		{
			string type = jObject["Type"]?.Value<string>();
			if(type == null) {
				// is only a key
				// should be already present in the lookup table
				if(!session.ContainsKey(key)) {
					throw new Exception("Could not read entity from JSON because of bad format: key not found. In most cases, this happens because you didn't use the BlueDB.Entity.EntityList collection.");
				}
				entityType = null;
				return session[key];
			}

			string fullEntityTypeName = $"{BlueDBProperties.Instance.EntityNamespace}.{type}, {BlueDBProperties.Instance.EntityAssemblyName}";
			entityType = Type.GetType(fullEntityTypeName);
			if(entityType == null) {
				throw new Exception($"Could not find an entity '{type}' in namespace '{BlueDBProperties.Instance.EntityNamespace}' in assembly '{BlueDBProperties.Instance.EntityAssemblyName}'.");
			}
			if(!entityType.IsSubclassOf(typeof(BlueDBEntity))) {
				// every entity type must be a subclass of BlueDBEntity
				throw new Exception($"Entity type '{type}' is not a subclass of BlueDBEntity.");
			}

			return null;
		}

		internal static void WriteEntity(JsonWriter writer, BlueDBEntity entity, JsonSerializer serializer, Dictionary<Type, Dictionary<int, BlueDBEntity>> session)
		{
			if(entity == null) {
				// do not explicitly serialize null values
				return;
			}

			if(session.ContainsKey(entity.GetType())) {
				foreach(var keyEntity in session[entity.GetType()]) {
					if(ReferenceEquals(keyEntity.Value, entity)) {
						writer.WriteStartObject();
						writer.WritePropertyName("Key");
						serializer.Serialize(writer, keyEntity.Key);
						writer.WriteEndObject();
						return;
					}
				}
			}

			WriteEntityPrivate(writer, entity, serializer, session);
		}

		private static void WriteEntityPrivate(JsonWriter writer,BlueDBEntity entity,JsonSerializer serializer,Dictionary<Type,Dictionary<int,BlueDBEntity>> session)
		{
			Type currentType = entity.GetType();

			if(!session.ContainsKey(currentType)) {
				session.Add(currentType, new Dictionary<int, BlueDBEntity>());
			}

			// add it to the session
			int key = GetNextKEY();
			session[currentType].Add(key, entity);

			List<Field> fields = BlueDBEntity.GetAllFields(currentType,true,true);

			int subEntityCount = 0;
			// while current type is a SubEntity
			while(EntityUtility.IsSubEntity(currentType)) {
				++subEntityCount;
				if(subEntityCount > 1) {
					writer.WritePropertyName(currentType.Name);
				}
				WriteEntityStart(currentType, key, writer, serializer);
				for(int i = 0; i < fields.Count; ++i) {
					Field field = fields[i];
					if(field.EntityType != currentType) {
						// this field is of parents ...
						continue;
					}
					// remove the field, so that it won't be processed again
					fields.RemoveAt(i--);
					WriteField(entity, field, writer, serializer,session);
				}
				
				currentType = EntityUtility.GetParentEntity(currentType);
				key = GetNextKEY();
				writer.WritePropertyName(currentType.Name);
			}

			// remaining StrongEntity base parent
			WriteEntityStart(currentType, key, writer, serializer);
			foreach(Field field in fields) {
				WriteField(entity, field, writer, serializer,session);
			}

			// insert object endings
			while(--subEntityCount >= -1) {
				// 1 for Properties
				writer.WriteEndObject();
				// 1 for Object
				writer.WriteEndObject();
			}
		}

		private static void WriteEntityStart(Type entityType,int key,JsonWriter writer,JsonSerializer serializer)
		{
			writer.WriteStartObject();
			// Type
			writer.WritePropertyName("Type");
			serializer.Serialize(writer, entityType.Name);
			// Key
			writer.WritePropertyName("Key");
			serializer.Serialize(writer, key);
			// Properties
			writer.WritePropertyName("Properties");
			writer.WriteStartObject();
		}

		private static void WriteField(BlueDBEntity entity,Field field, JsonWriter writer, JsonSerializer serializer, Dictionary<Type, Dictionary<int, BlueDBEntity>> session)
		{
			object fieldValue = ReflectionUtility.GetPropertyValue(entity, field.Name);
			if(fieldValue == null) {
				// do not explicitly serialize null values
				return;
			}

			// name
			writer.WritePropertyName(field.Name);

			// value
			if(field.IsEntity) {
				if(field.DataType == FieldType.LIST) {
					IList entityList = fieldValue as IList;

					writer.WriteStartArray();
					foreach(var entityItem in entityList) {
						WriteEntity(writer, entityItem as BlueDBEntity, serializer, session);
					}
					writer.WriteEndArray();
				}else {
					WriteEntity(writer,fieldValue as BlueDBEntity,serializer, session);
				}
			}else {
				serializer.Serialize(writer, fieldValue);
			}
		}
	}
}
