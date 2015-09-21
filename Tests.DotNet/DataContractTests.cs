namespace Tests.DotNet
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Axinom.Toolkit;
	using NUnit.Framework;

	[TestFixture]
	public class DataContractTests
	{
		public class MyClass
		{
			public string MyProperty { get; set; }
		}

		[Test]
		public void BasicSeriAndDeseriSeemsToWork()
		{
			#region Doc example: basic seri/deseri
			MyClass input = new MyClass
			{
				MyProperty = "MyValue"
			};

			string xml = Helpers.DataContract.Serialize(input);

			MyClass result = Helpers.DataContract.Deserialize<MyClass>(xml);

			// If you do not know the type beforehand, you can provide a Type parameter.
			Type dataType = typeof(MyClass); // Pretend this comes from configuration or something.
			object alternativeResult = Helpers.DataContract.Deserialize(xml, dataType);
			#endregion

			Assert.AreEqual(input.MyProperty, result.MyProperty);
			Assert.IsInstanceOf<MyClass>(alternativeResult);
			Assert.AreEqual(input.MyProperty, ((MyClass)alternativeResult).MyProperty);
		}

		[Test]
		public void OutputIsUtf8()
		{
			string xml = Helpers.DataContract.Serialize(new MyClass());

			Assert.IsTrue(xml.Contains(@"encoding=""utf-8"""));
		}
	}
}