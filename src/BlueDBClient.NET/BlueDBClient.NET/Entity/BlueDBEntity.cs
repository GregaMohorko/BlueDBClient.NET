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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDB.Entity.Fields;
using BlueDB.IO;
using BlueDB.Utility;
using GM.Utility;
using Newtonsoft.Json;

namespace BlueDB.Entity
{
	/// <summary>
	/// Base class for BlueDB entities. It implements <see cref="INotifyPropertyChanged"/> and <see cref="INotifyPropertyChanging"/> interfaces.
	/// <para>
	/// Use the <see cref="Set{T}(string, ref T, T)"/> method to set field values.
	/// </para>
	/// </summary>
	[JsonConverter(typeof(EntityJsonConverter))]
	public abstract class BlueDBEntity : INotifyPropertyChanged, INotifyPropertyChanging
	{
		// Database fields
		/// <summary>
		/// Represents the <see cref="ID"/> field.
		/// </summary>
		public static readonly Field IDField = Field.Register(nameof(ID));
		/// <summary>
		/// The ID of this entity.
		/// </summary>
		public int? ID { get; set; }

		#region PUBLIC PROPERTIES
		/// <summary>
		/// Determines whether this entity is present in the database or not. Basically, it returns true if ID>0.
		/// </summary>
		public bool IsPersistent => ID > 0;
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
			return ID ?? 0;
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
		/// <param name="entity1">The first entity.</param>
		/// <param name="entity2">The second entity.</param>
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
		/// <param name="entity1">The first entity.</param>
		/// <param name="entity2">The second entity.</param>
		public static bool operator !=(BlueDBEntity entity1,BlueDBEntity entity2)
		{
			return !(entity1 == entity2);
		}
		#endregion // Object -> Operators

		#endregion // Object

		#region INOTIFY OBSERVABLE
		/// <summary>
		/// Occurs when a field of this entity changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;
		/// <summary>
		/// Occurs when a field of this entity is about to be changed.
		/// </summary>
		public event PropertyChangingEventHandler PropertyChanging;

		/// <summary>
		/// Raises the PropertyChanged event. You do not need to call this manually if you use the <see cref="Set{T}(string, ref T, T)"/> method.
		/// </summary>
		/// <param name="propertyName">The name of the property that changed.</param>
		public void RaisePropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Raises the PropertyChanging event. You do not need to call this manually if you use the <see cref="Set{T}(string, ref T, T)"/> method.
		/// </summary>
		/// <param name="propertyName">The name of the property that is about to be changed.</param>
		public void RaisePropertyChanging(string propertyName)
		{
			PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
		}

		/// <summary>
		/// Compares the current and new value of the specified field. If it differs, then it raises <see cref="PropertyChanging"/> event, assigns the new value and raises the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <typeparam name="T">The type of the property that is being set.</typeparam>
		/// <param name="propertyName">The name of the property that is being set.</param>
		/// <param name="field">The field that is being set.</param>
		/// <param name="newValue">The new field value that is about to be set.</param>
		protected bool Set<T>(string propertyName, ref T field, T newValue)
		{
			if(EqualityComparer<T>.Default.Equals(field, newValue)) {
				// are the same
				return false;
			}

			RaisePropertyChanging(propertyName);
			field = newValue;
			RaisePropertyChanged(propertyName);
			return true;
		}
		#endregion // INotify observable

		#region PUBLIC UTILITY
		/// <summary>
		/// Merges this instance of the BlueDBEntity with the provided entity as a takeover: buyers properties are set to this instance properties (ID is of course ignored).
		/// <para>
		/// Note: Non BlueDBEntity properties are set as a reference. The same applies to lists (ONE_TO_MANY and MANY_TO_MANY fields).
		/// </para>
		/// <para>
		/// If a hidden field if null in the buyer and not in this instance, it will not be nullified.
		/// </para>
		/// </summary>
		/// <param name="buyer">The entity whose fields will be set to this instance.</param>
		/// <param name="mergeInnerEntityProperties">If true, the inner BlueDBEntity properties (when the ID is the same in this instance and the buyer) will also get merged. If false, they will only be changed if the buyers property value is null.</param>
		public void Merge(BlueDBEntity buyer,bool mergeInnerEntityProperties=true)
		{
			Type thisInstanceType = GetType();

			List<Field> databaseFieldsWithoutID = GetAllFields(thisInstanceType, true,true);
			databaseFieldsWithoutID.Remove(IDField);
			List<Field> hiddenDatabaseFields = GetAllHiddenFields(thisInstanceType, true);

			foreach(Field databaseField in databaseFieldsWithoutID) {
				object buyerValue = buyer.GetPropertyValue(databaseField.Name);
				object thisValue = this.GetPropertyValue(databaseField.Name);

				if(buyerValue==null && thisValue == null) {
					continue;
				}

				if(buyerValue==null && hiddenDatabaseFields.Contains(databaseField)) {
					// don't nullify hidden properties
					continue;
				}

				if(!databaseField.IsEntity || databaseField.DataType==FieldType.LIST) {
					// simply set the property to the buyers value
					this.SetProperty(databaseField.Name, buyerValue);
				} else {
					// is an entity
					if(mergeInnerEntityProperties) {
						BlueDBEntity buyerEntityValue = buyerValue as BlueDBEntity;
						BlueDBEntity thisEntityValue = thisValue as BlueDBEntity;

						if(buyerEntityValue == thisEntityValue) {
							// same ID, check inside
							thisEntityValue.Merge(buyerEntityValue);
						} else {
							// is not the same entity
							this.SetProperty(databaseField.Name, buyerValue);
						}
					}
				}
			}
		}
		#endregion // Public Utility

		private static Dictionary<Type, List<Field>> _allFields;
		private static Dictionary<Type,List<Field>> AllFields
		{
			get
			{
				// this has to be done like this, because this field is called before the static initialization of this class
				// don't really know why that is ...
				if(_allFields == null) {
					_allFields = new Dictionary<Type, List<Field>>();
				}
				return _allFields;
			}
		}

		private static Dictionary<Type, List<Field>> _allFieldsWithHidden;
		private static Dictionary<Type,List<Field>> AllFieldsWithHidden
		{
			get
			{
				if(_allFieldsWithHidden == null) {
					_allFieldsWithHidden = new Dictionary<Type, List<Field>>();
				}
				return _allFieldsWithHidden;
			}
		}

		#region PUBLIC STATIC UTILITY
		/// <summary>
		/// Gets a list of all fields of the specified type.
		/// </summary>
		/// <typeparam name="T">The type of which to get the fields.</typeparam>
		/// <param name="includeInherited">Determines whether to include fields of parent classes of the specified type.</param>
		/// <param name="includeHidden">Determines whether to include hidden fields.</param>
		public static List<Field> GetAllFields<T>(bool includeInherited=true,bool includeHidden=false) where T:BlueDBEntity
		{
			return GetAllFields(typeof(T),includeInherited, includeHidden);
		}

		/// <summary>
		/// Gets a list of all fields of the specified type.
		/// </summary>
		/// <param name="type">The type of which to get the fields.</param>
		/// <param name="includeInherited">Determines whether to include fields of parent classes of the specified type.</param>
		/// <param name="includeHidden">Determines whether to include hidden fields.</param>
		public static List<Field> GetAllFields(Type type,bool includeInherited=true, bool includeHidden = false)
		{
			bool isBaseEntityClass = type==typeof(BlueDBEntity);
			bool isStrongEntity = EntityUtility.IsStrongEntity(type);
			if(!EntityUtility.IsEntity(type) && !isBaseEntityClass) {
				throw new ArgumentException($"The provided type '{type}' is not a subclass of BlueDBEntity.");
			}

			Dictionary<Type, List<Field>> fieldDictionary = includeHidden ? AllFieldsWithHidden : AllFields;

			List<Field> fields;
			if(fieldDictionary.ContainsKey(type)) {
				fields = new List<Field>(fieldDictionary[type]);
			}else {
				// specified type has no fields
				fields = new List<Field>();
			}

			if(!isBaseEntityClass && !isStrongEntity && includeInherited) {
				// is a SubEntity, include fields of parents
				Type parentType = EntityUtility.GetParentEntity(type);
				fields.AddRange(GetAllFields(parentType, true,includeHidden));
			}
			return fields;
		}

		/// <summary>
		/// Gets a list of all hidden fields of the specified type.
		/// </summary>
		/// <typeparam name="T">The type of which to get the hidden fields.</typeparam>
		/// <param name="includeInherited">Determines whether to include fields of parent classes of the specified type.</param>
		public static List<Field> GetAllHiddenFields<T>(bool includeInherited=true)
		{
			return GetAllHiddenFields(typeof(T), includeInherited);
		}

		/// <summary>
		/// Gets a list of all hidden fields of the specified type.
		/// </summary>
		/// <param name="type">The type of which to get the hidden fields.</param>
		/// <param name="includeInherited">Determines whether to include fields of parent classes of the specified type.</param>
		public static List<Field> GetAllHiddenFields(Type type,bool includeInherited=true)
		{
			return GetAllFields(type, includeInherited, true)
				.Where(f => f.IsHidden)
				.ToList();
		}

		/// <summary>
		/// Gets a list of all fields of the specified type that are of type <see cref="FieldType.ENTITY"/>.
		/// </summary>
		/// <typeparam name="T">The type of which to get the fields.</typeparam>
		/// <param name="includeInherited">Determines whether to include fields of parent classes.</param>
		/// <param name="includeHidden">Determines whether to include hidden fields.</param>
		public static List<Field> GetAllEntityFields<T>(bool includeInherited=true,bool includeHidden=false) where T:BlueDBEntity
		{
			return GetAllEntityFields(typeof(T), includeInherited,includeHidden);
		}

		/// <summary>
		/// Gets a list of all fields of the specified type that are of type <see cref="FieldType.ENTITY"/>.
		/// </summary>
		/// <param name="type">The type of which to get the fields.</param>
		/// <param name="includeInherited">Determines whether to include fields of parent classes.</param>
		/// <param name="includeHidden">Determines whether to include hidden fields.</param>
		public static List<Field> GetAllEntityFields(Type type,bool includeInherited=true, bool includeHidden = false)
		{
			return GetAllFields(type, includeInherited,includeHidden)
				.Where(f => f.DataType == FieldType.ENTITY)
				.ToList();
		}
		#endregion // Public Static Utility

		#region INTERNAL STATIC UTILITY
		internal static void RegisterField(Type entityType, Field field)
		{
			if(entityType != typeof(BlueDBEntity) && !EntityUtility.IsEntity(entityType)) {
				throw new ArgumentException($"Cannot register field '{field.Name}' because the type '{entityType}' is not a subclass of BlueDBEntity.");
			}

			RegisterEntityType(entityType);
			
			AllFieldsWithHidden[entityType].Add(field);
			if(!field.IsHidden) {
				AllFields[entityType].Add(field);
			}
		}

		internal static void RegisterEntityType(Type entityType)
		{
			if(entityType != typeof(BlueDBEntity) && !EntityUtility.IsEntity(entityType)) {
				throw new ArgumentException($"Cannot register type '{entityType}' because it is not a subclass of BlueDBEntity.");
			}
			
			if(AllFields.ContainsKey(entityType)) {
				// already registered
				return;
			} else {
				var fieldsOfEntityType = new List<Field>();
				var fieldsOfEntityTypeWithHidden = new List<Field>();
				AllFields.Add(entityType, fieldsOfEntityType);
				AllFieldsWithHidden.Add(entityType, fieldsOfEntityTypeWithHidden);
				// if it is a StrongEntity, add the fields of the BlueDBEntity
				if(EntityUtility.IsStrongEntity(entityType)) {
					List<Field> bluedbFields = GetAllFields<BlueDBEntity>();
					fieldsOfEntityType.AddRange(bluedbFields);
					fieldsOfEntityTypeWithHidden.AddRange(bluedbFields);
				}
			}
		}

		internal static void ClearAllFields()
		{
			List<Field> baseFields = AllFields[typeof(BlueDBEntity)];
			List<Field> baseFieldsWithHidden = AllFieldsWithHidden[typeof(BlueDBEntity)];
			AllFields.Clear();
			AllFieldsWithHidden.Clear();
			AllFields.Add(typeof(BlueDBEntity), baseFields);
			AllFieldsWithHidden.Add(typeof(BlueDBEntity), baseFieldsWithHidden);
		}
		#endregion // Internal Static Utility
	}
}
