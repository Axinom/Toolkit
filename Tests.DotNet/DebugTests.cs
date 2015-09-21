namespace Tests
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Diagnostics;
	using System.Linq;
	using Axinom.Toolkit;
	using Xunit;

	public sealed class DebugTests
	{
		private const string StringValue = "et6ujhaeõ'54a";
		private const int IntValue = 2367;
		private static readonly Guid GuidValue = new Guid("8EE7DB16-E768-4E06-B8A6-05E4A14BC814");

		private class TestClassObject
		{
			public string StringProperty { get; set; }
			public int IntProperty { get; set; }
			public int? NullableIntProperty { get; set; }
			public int[] IntArrayProperty { get; set; }
			public byte[] ByteArrayProperty { get; set; }
			public string[] StringArrayProperty { get; set; }

			public TestClassObject AnotherClassObject { get; set; }
			public TestStructObject SomeStructObject { get; set; }

			public Hashtable HashtableProperty { get; set; }
			public NameValueCollection NameValueCollectionProperty { get; set; }

			public string StringField;
			public int? NullableIntField1;
			public int? NullableIntField2;

			public Guid GuidField = GuidValue;

			public TestClassObject()
			{
				StringProperty = StringValue;
				IntProperty = IntValue;
				NullableIntProperty = IntValue;

				IntArrayProperty = Enumerable.Range(1, 10).ToArray();
				ByteArrayProperty = Enumerable.Range(1, 10).Select(i => (byte)i).ToArray();
				StringArrayProperty = Enumerable.Range(1, 10).Select(i => i.ToString()).ToArray();

				HashtableProperty = new Hashtable();
				HashtableProperty["a"] = "b";
				HashtableProperty[IntValue] = StringValue;

				NameValueCollectionProperty = new NameValueCollection();
				NameValueCollectionProperty["a"] = "b";
				NameValueCollectionProperty[StringValue] = IntValue.ToString();

				StringField = StringValue;
				NullableIntField1 = IntValue;
				NullableIntField2 = null;
			}
		}

		private struct TestStructObject
		{
			public string StringProperty { get; set; }
			public Hashtable HashtableField;
		}

		[Fact]
		public void ToDebugString_WithGuidField_OutputContainsValue()
		{
			var o = new TestClassObject();

			var output = Helpers.Debug.ToDebugString(o);
			Debug.WriteLine(output);

			Assert.Contains(GuidValue.ToString(), output);
		}

		[Fact]
		public void ToDebugString_WithStringProperty_OutputContainsPropertyValue()
		{
			var o = new TestClassObject();

			var output = Helpers.Debug.ToDebugString(o);
			Debug.WriteLine(output);

			Assert.Contains(StringValue, output);
		}

		[Fact]
		public void ToDebugString_WithRecursion_OutputsOnlyOneInstanceOfObject()
		{
			const string canary = "at'jhõ''õõõa'f'f''f'f'f'f'f";

			var o = new TestClassObject();
			o.AnotherClassObject = o;

			o.StringProperty = canary;

			var output = Helpers.Debug.ToDebugString(o);
			Debug.WriteLine(output);

			var first = output.IndexOf(canary);
			var last = output.LastIndexOf(canary);

			Assert.NotEqual(-1, first);
			Assert.Equal(first, last);
		}

		[Fact]
		public void ToDebugString_WithStruct_OutputContainsStructData()
		{
			const string canary1 = "46464646546879887987654321";
			const string canary2 = "agjagjagjagja";

			var o = new TestClassObject();

			var s = new TestStructObject()
			{
				HashtableField = new Hashtable(),
				StringProperty = canary1
			};

			s.HashtableField["a"] = canary2;
			s.HashtableField[2] = 3;

			o.SomeStructObject = s;

			var output = Helpers.Debug.ToDebugString(o);
			Debug.WriteLine(output);

			Assert.Contains(canary1, output);
			Assert.Contains(canary2, output);
		}

		[Fact]
		public void ToDebugString_WithDateTime_OutputContainsOnlyExpectedString()
		{
			var o = DateTime.Now;

			// Can't use u because it marks as UTC but the data might not actually be UTC.
			var expected = o.ToString("s") + Environment.NewLine;

			var output = Helpers.Debug.ToDebugString(o);
			Debug.WriteLine(output);

			Assert.Equal(expected, output);
		}

		[Fact]
		public void ToDebugString_WithDateTimeOffset_OutputContainsOnlyExpectedString()
		{
			var o = DateTimeOffset.Now;

			var expected = o.ToString("u") + Environment.NewLine;

			var output = Helpers.Debug.ToDebugString(o);
			Debug.WriteLine(output);

			Assert.Equal(expected, output);
		}

		[Fact]
		public void ToDebugString_WithTimeSpan_OutputContainsOnlyExpectedString()
		{
			var o = TimeSpan.FromSeconds(358357835753.2567247246572467);

			var expected = o + Environment.NewLine;

			var output = Helpers.Debug.ToDebugString(o);
			Debug.WriteLine(output);

			Assert.Equal(expected, output);
		}

		[Fact]
		public void ToDebugString_WithEnum_OutputContainsOnlyExpectedString()
		{
			var o = AttributeTargets.GenericParameter;

			var expected = o + Environment.NewLine;

			var output = Helpers.Debug.ToDebugString(o);
			Debug.WriteLine(output);

			Assert.Equal(expected, output);
		}

		[Fact]
		public void ToDebugString_WithGenericType_DoesNotPrintGenericArgumentSpam()
		{
			var o = new KeyValuePair<string, DebugTests>("asdfasdf", null);

			var output = Helpers.Debug.ToDebugString(o);
			Debug.WriteLine(output);

			Assert.DoesNotContain(typeof(DebugTests).Name, output);
		}
	}
}