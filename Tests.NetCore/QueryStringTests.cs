namespace Tests
{
    using Axinom.Toolkit;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;

    [TestClass]
    public sealed class QueryStringTests : TestClass
    {
        [TestMethod]
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

        [TestMethod]
        public void StringUrlWithoutQueryStringIsFine()
        {
            QueryString.FromUrl("page.html");
        }

        [TestMethod]
        public void EmptyStringIsFineAsUrl()
        {
            QueryString.FromUrl("");
        }

        [TestMethod]
        public void EmptyStringIsFineAsQs()
        {
            QueryString.FromQueryString("");
        }

        [TestMethod]
        public void QuestionMarkIsFineAsQs()
        {
            QueryString.FromQueryString("?");
        }

        [TestMethod]
        public void EmptyInEmptyOut()
        {
            var qs = QueryString.FromUrl("http://example.com/index");

            Assert.AreEqual("?", qs.ToString());
        }

        [TestMethod]
        public void OneParameterIsRead()
        {
            var qs = QueryString.FromUrl("http://example.com?name=value");

            Assert.IsTrue(qs.Contains("name"));
            Assert.AreEqual("value", qs["name"]);
        }

        [TestMethod]
        public void ThreeParametersAreReadFromString()
        {
            var qs = QueryString.FromUrl("http://example.com?name1=value1&name2=value2&name3=value3");

            Assert.IsTrue(qs.Contains("name1"));
            Assert.AreEqual("value1", qs["name1"]);
            Assert.IsTrue(qs.Contains("name2"));
            Assert.AreEqual("value2", qs["name2"]);
            Assert.IsTrue(qs.Contains("name3"));
            Assert.AreEqual("value3", qs["name3"]);
        }

        [TestMethod]
        public void ThreeParametersAreReadFromUri()
        {
            // Same as ThreeParametersAreReadFromString but using Uri as input (to ensure parsing works for all sources)
            var qs = QueryString.FromUrl(new Uri("http://example.com?name1=value1&name2=value2&name3=value3"));

            Assert.IsTrue(qs.Contains("name1"));
            Assert.AreEqual("value1", qs["name1"]);
            Assert.IsTrue(qs.Contains("name2"));
            Assert.AreEqual("value2", qs["name2"]);
            Assert.IsTrue(qs.Contains("name3"));
            Assert.AreEqual("value3", qs["name3"]);
        }

        [TestMethod]
        public void ThreeParametersAreReadFromQs()
        {
            // Same as ThreeParametersAreReadFromString but using a QS string as input (to ensure parsing works for all sources)
            var qs = QueryString.FromQueryString("?name1=value1&name2=value2&name3=value3");

            Assert.IsTrue(qs.Contains("name1"));
            Assert.AreEqual("value1", qs["name1"]);
            Assert.IsTrue(qs.Contains("name2"));
            Assert.AreEqual("value2", qs["name2"]);
            Assert.IsTrue(qs.Contains("name3"));
            Assert.AreEqual("value3", qs["name3"]);
        }

        [TestMethod]
        public void EmptyValueIsReadAsEmpty()
        {
            var qs = QueryString.FromQueryString("name=");

            Assert.AreEqual("", qs["name"]);
        }

        [TestMethod]
        public void NoValueIsReadAsNull()
        {
            var qs = QueryString.FromQueryString("name");

            Assert.IsTrue(qs.Contains("name"));
            Assert.AreEqual(null, qs["name"]);
        }

        [TestMethod]
        public void EmptyValueIsWrittenAsEmpty()
        {
            var qs = new QueryString();

            qs["name"] = "";

            Assert.AreEqual("?name=", qs.ToString());
        }

        [TestMethod]
        public void NullIsWrittenAsNull()
        {
            var qs = new QueryString();

            qs["name"] = null;

            Assert.AreEqual("?name", qs.ToString());
        }

        [TestMethod]
        public void OneParameterIsWritten()
        {
            var qs = new QueryString();

            qs["name"] = "value";

            Assert.AreEqual("?name=value", qs.ToString());
        }

        [TestMethod]
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

        [TestMethod]
        public void BasicOperations()
        {
            var qs = new QueryString();

            qs.Set("key", "value");
            Assert.AreEqual("value", qs.Get("key"));
            Assert.AreEqual("value", qs["key"]);
            Assert.AreEqual("value", qs.TryGet("key", "default"));
            Assert.AreEqual("default", qs.TryGet("nonexistingkey", "default"));

            Assert.IsTrue(qs.Contains("key"));
            qs.Remove("key");
            Assert.IsFalse(qs.Contains("key"));

            qs["key"] = "value";
            Assert.IsTrue(qs.Contains("key"));
        }

        [TestMethod]
        public void NamesAreCaseInsensitive()
        {
            var qs = new QueryString();

            qs["name"] = "value1";
            qs["NAme"] = "value2";

            Assert.AreEqual("value2", qs["name"]);
        }

        [TestMethod]
        public void NamesCaseIsPreserved()
        {
            var qs = QueryString.FromQueryString("?NamE=value");

            qs["NamE"] = "value2";

            Assert.AreEqual("?NamE=value2", qs.ToString());
        }
    }
}