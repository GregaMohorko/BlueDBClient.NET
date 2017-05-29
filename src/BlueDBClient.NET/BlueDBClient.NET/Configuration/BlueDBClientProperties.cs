using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BlueDBClient.NET.Entity;

namespace BlueDBClient.NET.Configuration
{
	public class BlueDBClientProperties
	{
		private static BlueDBClientProperties _instance;
		public static BlueDBClientProperties Instance
		{
			get
			{
				if(_instance == null) {
					throw new InvalidOperationException("You need to initialize BlueDBClient.NET properties. Call BlueDBClientProperties.Init() method.");
				}
				return _instance;
			}
		}

		/// <summary>
		/// Initializes BlueDBClient. The entity namespace and assembly is extracted from the specified example entity type.
		/// </summary>
		/// <typeparam name="T">One of the entity types from which to extract the entity namespace and assembly from.</typeparam>
		public static void Init<T>()
		{
			Init(typeof(T));
		}

		/// <summary>
		/// Initializes BlueDBClient. The entity namespace and assembly is extracted from the specified example entity type.
		/// </summary>
		/// <param name="entityExampleType">One of the entity types from which to extract the entity namespace and assembly from.</param>
		public static void Init(Type entityExampleType)
		{
			if(_instance != null) {
				BlueDBEntity.ClearAllFields();
			}
			_instance = new BlueDBClientProperties(entityExampleType);
		}

		/// <summary>
		/// The namespace of the entities.
		/// </summary>
		public readonly string EntityNamespace;
		/// <summary>
		/// The name of the assembly in which the entities are located.
		/// </summary>
		public readonly string EntityAssemblyName;

		private BlueDBClientProperties(Type entityExampleType)
		{
			EntityNamespace = entityExampleType.Namespace;
			Assembly assembly = Assembly.GetAssembly(entityExampleType);
			EntityAssemblyName = assembly.FullName;
		}
	}
}
