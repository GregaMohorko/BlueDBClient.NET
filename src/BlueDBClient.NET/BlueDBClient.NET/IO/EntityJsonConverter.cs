using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDBClient.NET.Configuration;
using BlueDBClient.NET.Entity;
using BlueDBClient.NET.Entity.Fields;
using BlueDBClient.NET.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlueDBClient.NET.IO
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
			Type entityType;
			BlueDBEntity entityObject = ReadType(jObject, key, out entityType, session);
			if(entityObject != null) {
				// was found in the session by key
				return entityObject;
			}

			// create the entity
			entityObject = Activator.CreateInstance(entityType,true) as BlueDBEntity;
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

				List<Field> fields = BlueDBEntity.GetAllFields(currentType, false);
				
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
						case FieldType.Property:
							switch(jPropertyValue.Type) {
								case JTokenType.String:
									propertyValue = jPropertyValue.Value<string>();
									if(field.Type == typeof(TimeSpan)) {
										propertyValue = TimeSpan.Parse((string)propertyValue);
									}
									break;
								case JTokenType.Integer:
									propertyValue = jPropertyValue.Value<int?>();
									if(field.Type.IsEnum) {
										propertyValue = Enum.ToObject(field.Type,propertyValue);
										break;
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
						case FieldType.Entity:
							propertyValue=ReadEntity((JObject)jPropertyValue, session);
							break;
						case FieldType.List:
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

					ReflectionUtility.SetProperty(entityObject, field.Name, propertyValue);
				}
				
				if(currentType.BaseType == typeof(BlueDBEntity)) {
					// no more parent entities
					break;
				}
				if(parentJObject == null) {
					throw new Exception($"Bad JSON format: no object for parent in '{currentType.Name}' entity.");
				}

				int parentKey = ReadKey(parentJObject);
				Type parentType;
				BlueDBEntity parentEntity = ReadType(parentJObject, parentKey, out parentType, session);
				if(parentEntity != null) {
					// what? parent entity should be null!
					// are two different entities sharing the same parent?
					// hmmmm... that is wrong
					throw new Exception("Bad format: two different entities are sharing the same parent. That should not happen, that is a wrong design.");
				}
				if(parentType != currentType.BaseType) {
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
					throw new Exception("Could not read entity from JSON because of bad format: key not found.");
				}
				entityType = null;
				return session[key];
			}

			string fullEntityTypeName = $"{BlueDBClientProperties.Instance.EntityNamespace}.{type}, {BlueDBClientProperties.Instance.EntityAssemblyName}";
			entityType = Type.GetType(fullEntityTypeName);
			if(entityType == null) {
				throw new Exception($"Could not find an entity '{type}' in namespace '{BlueDBClientProperties.Instance.EntityNamespace}' in assembly '{BlueDBClientProperties.Instance.EntityAssemblyName}'.");
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

			List<Field> fields = BlueDBEntity.GetAllFields(currentType);

			int subEntityCount = 0;
			// while current type is a SubEntity
			while(currentType.BaseType != typeof(BlueDBEntity)) {
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

				currentType = currentType.BaseType;
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
				if(field.DataType == FieldType.List) {
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
