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
Created: 2018-1-11
Author: GregaMohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDB.Configuration;
using BlueDB.Entity;

namespace BlueDB
{
	/// <summary>
	/// A static class used to initialize the BlueDB library.
	/// </summary>
	public static class BlueDBLib
	{
		/// <summary>
		/// One of the entity types from which the BlueDB extracted the entity namespace and assembly from.
		/// </summary>
		public static Type SampleEntityType { get; private set; }

		/// <summary>
		/// Initializes BlueDB. The entity namespace and assembly is extracted from the specified sample entity type.
		/// </summary>
		/// <typeparam name="TSampleEntityType">One of the entity types from which to extract the entity namespace and assembly from.</typeparam>
		public static void Init<TSampleEntityType>()
		{
			Init(typeof(TSampleEntityType));
		}

		/// <summary>
		/// Initializes BlueDB. The entity namespace and assembly is extracted from the specified sample entity type.
		/// </summary>
		/// <param name="entitySampleType">One of the entity types from which to extract the entity namespace and assembly from.</param>
		public static void Init(Type entitySampleType)
		{
			SampleEntityType = entitySampleType;

			BlueDBProperties.Initialize();
		}
	}
}
