using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlueDBClient.NET.Utility
{
	public static class ReflectionUtility
	{
		/// <summary>
		/// Sets the specified property to the provided value in the provided object.
		/// </summary>
		/// <param name="obj">The object with the property.</param>
		/// <param name="propertyName">The name of the property to set.</param>
		/// <param name="value">The value to set the property to.</param>
		public static void SetProperty(object obj, string propertyName, object value)
		{
			GetPropertyInfo(obj, propertyName).SetValue(obj, value);
		}

		/// <summary>
		/// Gets the value the specified property in the provided object.
		/// </summary>
		/// <param name="obj">The object that has the property.</param>
		/// <param name="propertyName">The name of the property.</param>
		public static object GetPropertyValue(object obj, string propertyName)
		{
			PropertyInfo property = GetPropertyInfo(obj, propertyName);
			return property.GetValue(obj);
		}

		/// <summary>
		/// Gets the type of the specified property in the specified type.
		/// <para>
		/// If the type is nullable, this function gets its generic definition.
		/// </para>
		/// </summary>
		/// <param name="type">The type that has the specified property.</param>
		/// <param name="propertyName">The name of the property.</param>
		public static Type GetPropertyType(Type type, string propertyName)
		{
			PropertyInfo property = GetPropertyInfo(type, propertyName);

			Type propertyType = property.PropertyType;

			// get the generic type of nullable, not THE nullable
			if(propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
				propertyType = GetGenericFirst(propertyType);

			return propertyType;
		}

		/// <summary>
		/// Gets the property information by name for the type of the provided object.
		/// </summary>
		/// <param name="obj">Object with a type that has the specified property.</param>
		/// <param name="propertyName">The name of the property.</param>
		public static PropertyInfo GetPropertyInfo(object obj, string propertyName)
		{
			return GetPropertyInfo(obj.GetType(), propertyName);
		}

		/// <summary>
		/// Gets the property information by name for the specified type.
		/// </summary>
		/// <param name="type">Type that has the specified property.</param>
		/// <param name="propertyName">The name of the property.</param>
		public static PropertyInfo GetPropertyInfo(Type type, string propertyName)
		{
			PropertyInfo property = type.GetProperty(propertyName);
			if(property == null)
				throw new Exception(string.Format("The provided property name ({0}) does not exist in type '{1}'.", propertyName, type.ToString()));

			return property;
		}

		/// <summary>
		/// Returns the first generic type of the specified type.
		/// </summary>
		/// <param name="type">The type from which to get the generic type.</param>
		public static Type GetGenericFirst(Type type)
		{
			return type.GetGenericArguments()[0];
		}

		/// <summary>
		/// Determines whether the specified type to check derives from the specified generic type.
		/// </summary>
		/// <param name="generic">The parent generic type.</param>
		/// <param name="toCheck">The type to check if it derives from the specified generic type.</param>
		public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
		{
			while(toCheck!=null && toCheck != typeof(object)) {
				Type current = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
				if(generic == current) {
					return true;
				}
				toCheck = toCheck.BaseType;
			}
			return false;
		}

		/// <summary>
		/// Determines whether the specified type is a generic list.
		/// </summary>
		/// <param name="type">The type to determine.</param>
		public static bool IsGenericList(Type type)
		{
			if(!typeof(IList).IsAssignableFrom(type))
				return false;
			if(!type.IsGenericType)
				return false;
			if(!type.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)))
				return false;
			return true;
		}
	}
}
