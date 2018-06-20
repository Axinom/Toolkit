namespace Tests
{
    using Axinom.Toolkit;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    [TestClass]
    public sealed class DataContractTests : BaseTestClass
    {
        public class MyClass
        {
            public string MyProperty { get; set; }
        }

        [TestMethod]
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
            Assert.IsInstanceOfType(alternativeResult, typeof(MyClass));
            Assert.AreEqual(input.MyProperty, ((MyClass)alternativeResult).MyProperty);
        }

        [TestMethod]
        public void OutputIsUtf8()
        {
            string xml = Helpers.DataContract.Serialize(new MyClass());

            Assert.IsTrue(xml.Contains(@"encoding=""utf-8"""));
        }
    }
}