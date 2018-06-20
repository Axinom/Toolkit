namespace Tests
{
    using Axinom.Toolkit;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
    public sealed class DebugTests : BaseTestClass
    {
        [TestMethod]
        public void GetAllExceptionMessages_DoesWhatItSays()
        {
            const string canary1 = "dvgrb4si";
            const string canary2 = "sw3345gg";
            const string canary3 = "dnjr68dr6";

            var ex1 = new Exception(canary1);
            var ex2 = new Exception(canary2, ex1);
            var ex3 = new Exception(canary3, ex2);

            var messages = Helpers.Debug.GetAllExceptionMessages(ex3);

            Assert.IsTrue(messages.Contains(canary1));
            Assert.IsTrue(messages.Contains(canary2));
            Assert.IsTrue(messages.Contains(canary3));
        }

        [TestMethod]
        public void GetAllExceptionMessages_WithOneMessage_HasNoNewlines()
        {
            var message = Helpers.Debug.GetAllExceptionMessages(new Exception("asdf"));

            Assert.IsFalse(message.Contains(Environment.NewLine));
        }

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
                StringField = StringValue;
                NullableIntField1 = IntValue;
                NullableIntField2 = null;
            }
        }

        private struct TestStructObject
        {
            public string StringProperty { get; set; }
            public List<string> StringListProperty { get; set; }
        }

        private class TestBaseClass
        {
            public string BaseString { get; set; }
        }

        private class TestDerivedClass : TestBaseClass
        {
            public string DerivedString { get; set; }
        }

        [TestMethod]
        public void ToDebugString_WithGuidField_OutputContainsValue()
        {
            var o = new TestClassObject();

            var output = Helpers.Debug.ToDebugString(o);
            _log.Debug(output);

            Assert.IsTrue(output.Contains(GuidValue.ToString()));
        }

        [TestMethod]
        public void ToDebugString_WithStringProperty_OutputContainsPropertyValue()
        {
            var o = new TestClassObject();

            var output = Helpers.Debug.ToDebugString(o);
            _log.Debug(output);

            Assert.IsTrue(output.Contains(StringValue));
        }

        [TestMethod]
        public void ToDebugString_WithRecursion_OutputsOnlyOneInstanceOfObject()
        {
            const string canary = "at'jhõ''õõõa'f'f''f'f'f'f'f";

            var o = new TestClassObject();
            o.AnotherClassObject = o;

            o.StringProperty = canary;

            var output = Helpers.Debug.ToDebugString(o);
            _log.Debug(output);

            var first = output.IndexOf(canary);
            var last = output.LastIndexOf(canary);

            Assert.AreNotEqual(-1, first);
            Assert.AreEqual(first, last);
        }

        [TestMethod]
        public void ToDebugString_WithStruct_OutputContainsStructData()
        {
            const string canary1 = "46464646546879887987654321";
            const string canary2 = "agjagjagjagja";

            var o = new TestClassObject();

            var s = new TestStructObject()
            {
                StringProperty = canary1,
                StringListProperty = new List<string>
                {
                    "qq",
                    canary2
                }
            };

            o.SomeStructObject = s;

            var output = Helpers.Debug.ToDebugString(o);
            _log.Debug(output);

            Assert.IsTrue(output.Contains(canary1));
            Assert.IsTrue(output.Contains(canary2));
        }

        [TestMethod]
        public void ToDebugString_WithDateTime_OutputContainsOnlyExpectedString()
        {
            var o = DateTime.Now;

            // Can't use u because it marks as UTC but the data might not actually be UTC.
            var expected = o.ToString("s") + Environment.NewLine;

            var output = Helpers.Debug.ToDebugString(o);
            _log.Debug(output);

            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void ToDebugString_WithDateTimeOffset_OutputContainsOnlyExpectedString()
        {
            var o = DateTimeOffset.Now;

            var expected = o.ToString("u") + Environment.NewLine;

            var output = Helpers.Debug.ToDebugString(o);
            _log.Debug(output);

            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void ToDebugString_WithTimeSpan_OutputContainsOnlyExpectedString()
        {
            var o = TimeSpan.FromSeconds(358357835753.2567247246572467);

            var expected = o + Environment.NewLine;

            var output = Helpers.Debug.ToDebugString(o);
            _log.Debug(output);

            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void ToDebugString_WithEnum_OutputContainsOnlyExpectedString()
        {
            var o = AttributeTargets.GenericParameter;

            var expected = o + Environment.NewLine;

            var output = Helpers.Debug.ToDebugString(o);
            _log.Debug(output);

            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void ToDebugString_WithGenericType_DoesNotPrintGenericArgumentSpam()
        {
            var o = new KeyValuePair<string, DebugTests>("asdfasdf", null);

            var output = Helpers.Debug.ToDebugString(o);
            _log.Debug(output);

            Assert.IsFalse(output.Contains(typeof(DebugTests).Name));
        }

        [TestMethod]
        public void ToDebugString_WithStaticStructInstance_DoesNotRecurse()
        {
            var o = Guid.Empty;

            var output = Helpers.Debug.ToDebugString(o);
            _log.Debug(output);

            // Guid.Empty leads to a new instance.
            // As long as we have "Empty" only once, we know a second instance was not printed.
            Assert.AreEqual(output.IndexOf("Empty"), output.LastIndexOf("Empty"));
        }

        [TestMethod]
        public void ToDebugString_WithLongArray_ReportsCorrectItemCount()
        {
            var o = new byte[1234];

            var output = Helpers.Debug.ToDebugString(o);
            _log.Debug(output);

            StringAssert.Contains(output, "1234");
        }

        [TestMethod]
        public void ToDebugString_WithDerivedClass_AlsoOutputsBaseClassMembers()
        {
            const string baseCanary = "esd4v689skrvtrgir rwor hg reakjh hljfd";
            const string derivedCanary = "pfnc9tj78fpm8 7l5rosdaaaaaaaa";

            var o = new TestDerivedClass
            {
                BaseString = baseCanary,
                DerivedString = derivedCanary
            };

            var output = Helpers.Debug.ToDebugString(o);
            _log.Debug(output);

            Assert.IsTrue(output.Contains(baseCanary));
            Assert.IsTrue(output.Contains(derivedCanary));
        }

        private static readonly LogSource _log = Log.Default.CreateChildSource(nameof(DebugTests));
    }
}