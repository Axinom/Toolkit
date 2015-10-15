namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Axinom.Toolkit;
	using Xunit;

	public sealed class QueryStringTests : TestClass
	{
		[Fact]
		public void DifferentUriTypesAreFine()
		{
			QueryString.FromUrl(new Uri("http://example.com?a=b", UriKind.Absolute));
			QueryString.FromUrl(new Uri("index.html?a=b", UriKind.Relative));
			QueryString.FromUrl(new Uri("index.html?a=b", UriKind.RelativeOrAbsolute));

			// We also check all of them without the QS part.
			QueryString.FromUrl(new Uri("http://example.com", UriKind.Absolute));
			QueryString.FromUrl(new Uri("index.html", UriKind.Relative));
			QueryString.FromUrl(new Uri("index.html", UriKind.RelativeOrAbsolute));
		}

		[Fact]
		public void StringUrlWithoutQueryStringIsFine()
		{
			QueryString.FromUrl("page.html");
		}

		[Fact]
		public void EmptyStringIsFineAsUrl()
		{
			QueryString.FromUrl("");
		}

		[Fact]
		public void EmptyStringIsFineAsQs()
		{
			QueryString.FromQueryString("");
		}

		[Fact]
		public void QuestionMarkIsFineAsQs()
		{
			QueryString.FromQueryString("?");
		}

		[Fact]
		public void EmptyInEmptyOut()
		{
			var qs = QueryString.FromUrl("http://example.com/index");

			Assert.Equal("?", qs.ToString());
		}

		[Fact]
		public void OneParameterIsRead()
		{
			var qs = QueryString.FromUrl("http://example.com?name=value");

			Assert.True(qs.Contains("name"));
			Assert.Equal("value", qs["name"]);
		}

		[Fact]
		public void ThreeParametersAreReadFromString()
		{
			var qs = QueryString.FromUrl("http://example.com?name1=value1&name2=value2&name3=value3");

			Assert.True(qs.Contains("name1"));
			Assert.Equal("value1", qs["name1"]);
			Assert.True(qs.Contains("name2"));
			Assert.Equal("value2", qs["name2"]);
			Assert.True(qs.Contains("name3"));
			Assert.Equal("value3", qs["name3"]);
		}

		[Fact]
		public void ThreeParametersAreReadFromUri()
		{
			// Same as ThreeParametersAreReadFromString but using Uri as input (to ensure parsing works for all sources)
			var qs = QueryString.FromUrl(new Uri("http://example.com?name1=value1&name2=value2&name3=value3"));

			Assert.True(qs.Contains("name1"));
			Assert.Equal("value1", qs["name1"]);
			Assert.True(qs.Contains("name2"));
			Assert.Equal("value2", qs["name2"]);
			Assert.True(qs.Contains("name3"));
			Assert.Equal("value3", qs["name3"]);
		}

		[Fact]
		public void ThreeParametersAreReadFromQs()
		{
			// Same as ThreeParametersAreReadFromString but using a QS string as input (to ensure parsing works for all sources)
			var qs = QueryString.FromQueryString("?name1=value1&name2=value2&name3=value3");

			Assert.True(qs.Contains("name1"));
			Assert.Equal("value1", qs["name1"]);
			Assert.True(qs.Contains("name2"));
			Assert.Equal("value2", qs["name2"]);
			Assert.True(qs.Contains("name3"));
			Assert.Equal("value3", qs["name3"]);
		}

		[Fact]
		public void EmptyValueIsReadAsEmpty()
		{
			var qs = QueryString.FromQueryString("name=");

			Assert.Equal("", qs["name"]);
		}

		[Fact]
		public void NoValueIsReadAsNull()
		{
			var qs = QueryString.FromQueryString("name");

			Assert.True(qs.Contains("name"));
			Assert.Equal(null, qs["name"]);
		}

		[Fact]
		public void EmptyValueIsWrittenAsEmpty()
		{
			var qs = new QueryString();

			qs["name"] = "";

			Assert.Equal("?name=", qs.ToString());
		}

		[Fact]
		public void NullIsWrittenAsNull()
		{
			var qs = new QueryString();

			qs["name"] = null;

			Assert.Equal("?name", qs.ToString());
		}

		[Fact]
		public void OneParameterIsWritten()
		{
			var qs = new QueryString();

			qs["name"] = "value";

			Assert.Equal("?name=value", qs.ToString());
		}

		[Fact]
		public void TwoParametersAreWritten()
		{
			var qs = new QueryString();

			qs["name1"] = "value1";
			qs["name2"] = "value2";

			var result = qs.ToString();
			var validAnswers = new[]
			{
				"?name1=value1&name2=value2",
				"?name2=value2&name1=value1"
			};

			if (!validAnswers.Contains(result))
				throw new Exception("Two parameter QS was not written correctly. Got result: " + result);
		}

		[Fact]
		public void BasicOperations()
		{
			var qs = new QueryString();

			qs.Set("key", "value");
			Assert.Equal("value", qs.Get("key"));
			Assert.Equal("value", qs["key"]);
			Assert.Equal("value", qs.TryGet("key", "default"));
			Assert.Equal("default", qs.TryGet("nonexistingkey", "default"));

			Assert.True(qs.Contains("key"));
			qs.Remove("key");
			Assert.False(qs.Contains("key"));

			qs["key"] = "value";
			Assert.True(qs.Contains("key"));
		}

		[Fact]
		public void NamesAreCaseInsensitive()
		{
			var qs = new QueryString();

			qs["name"] = "value1";
			qs["NAme"] = "value2";

			Assert.Equal("value2", qs["name"]);
		}

		[Fact]
		public void NamesCaseIsPreserved()
		{
			var qs = QueryString.FromQueryString("?NamE=value");

			qs["NamE"] = "value2";

			Assert.Equal("?NamE=value2", qs.ToString());
		}
	}
}