using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDB.Entity.Fields;

namespace BlueDB.Test.Test3.Entity
{
	class Student:User
	{
		public static readonly Field RegistrationNumberField = Field.Register(nameof(RegistrationNumber), typeof(Student));
		public string RegistrationNumber { get; set; }
	}
}
