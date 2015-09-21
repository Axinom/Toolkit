namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Axinom.Toolkit;
	using Xunit;

	public class XmlSerializationTests
	{
		public class MyClass
		{
			public string MyProperty { get; set; }
		}

		[Fact]
		public void BasicSeriAndDeseriSeemsToWork()
		{
			#region Doc example: basic seri/deseri
			MyClass input = new MyClass
			{
				MyProperty = "MyValue"
			};

			string xml = Helpers.XmlSerialization.XmlSerialize(input);

			MyClass result = Helpers.XmlSerialization.XmlDeserialize<MyClass>(xml);

			// If you do not know the type beforehand, you can provide a Type parameter.
			Type dataType = typeof(MyClass); // Pretend this comes from configuration or something.
			object alternativeResult = Helpers.XmlSerialization.XmlDeserialize(xml, dataType);
			#endregion

			Assert.Equal(input.MyProperty, result.MyProperty);
			Assert.IsType<MyClass>(alternativeResult);
			Assert.Equal(input.MyProperty, ((MyClass)alternativeResult).MyProperty);
		}

		[Fact]
		public void OutputIsUtf8()
		{
			string xml = Helpers.XmlSerialization.XmlSerialize(new MyClass());

			Assert.True(xml.Contains(@"encoding=""utf-8"""));
		}
	}
}