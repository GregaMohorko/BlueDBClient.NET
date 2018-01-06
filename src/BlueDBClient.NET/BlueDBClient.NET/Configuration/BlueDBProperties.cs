﻿/*
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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BlueDB.Entity;
using GM.Utility;

namespace BlueDB.Configuration
{
	/// <summary>
	/// A configuration class.
	/// </summary>
	public class BlueDBProperties
	{
		private static BlueDBProperties _instance;
		/// <summary>
		/// Gets the instance of BlueDB properties.
		/// </summary>
		public static BlueDBProperties Instance
		{
			get
			{
				if(_instance == null) {
					throw new InvalidOperationException("You need to initialize BlueDB. Call BlueDB.Configuration.BlueDBProperties.Init() method.");
				}
				return _instance;
			}
		}

		/// <summary>
		/// Initializes BlueDB. The entity namespace and assembly is extracted from the specified example entity type.
		/// </summary>
		/// <typeparam name="T">One of the entity types from which to extract the entity namespace and assembly from.</typeparam>
		public static void Init<T>()
		{
			Init(typeof(T));
		}

		/// <summary>
		/// Initializes BlueDB. The entity namespace and assembly is extracted from the specified example entity type.
		/// </summary>
		/// <param name="entityExampleType">One of the entity types from which to extract the entity namespace and assembly from.</param>
		public static void Init(Type entityExampleType)
		{
			if(_instance != null) {
				BlueDBEntity.ClearAllFields();
			}
			_instance = new BlueDBProperties(entityExampleType);
		}

		/// <summary>
		/// The namespace of the entities.
		/// </summary>
		public readonly string EntityNamespace;
		/// <summary>
		/// The display name of the assembly in which the entities are located.
		/// </summary>
		public readonly string EntityAssemblyName;

		private BlueDBProperties(Type entityExampleType)
		{
			EntityNamespace = entityExampleType.Namespace;
			Assembly assembly = Assembly.GetAssembly(entityExampleType);
			EntityAssemblyName = assembly.FullName;
		}
	}
}