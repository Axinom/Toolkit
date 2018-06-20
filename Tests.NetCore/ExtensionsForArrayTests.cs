namespace Tests
{
    using Axinom.Toolkit;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class ExtensionsForArrayTests : BaseTestClass
    {
        [TestMethod]
        public void ArrayContainsArray_WithNeedleLargerThanHaystack_ReturnsFalse()
        {
            var needle = new[] { 1, 2, 3, 4, 5 };
            var haystack = new[] { 1, 2, 3, 4 };

            Assert.IsFalse(haystack.ContainsArray(needle));
        }


        [TestMethod]
        public void ArrayContainsArray_WithEmptyNeedle_ReturnsFalse()
        {
            var needle = new int[0];
            var haystack = new[] { 1, 2, 3, 4 };

            Assert.IsFalse(haystack.ContainsArray(needle));
        }

        [TestMethod]
        public void ArrayContainsArray_WithNeedleInBeginning_ReturnsTrue()
        {
            var needle = new[] { 1, 2, 3 };
            var haystack = new[] { 1, 2, 3, 4 };

            Assert.IsTrue(haystack.ContainsArray(needle));
        }

        [TestMethod]
        public void ArrayContainsArray_WithNeedleInMiddle_ReturnsTrue()
        {
            var needle = new[] { 2, 3 };
            var haystack = new[] { 1, 2, 3, 4 };

            Assert.IsTrue(haystack.ContainsArray(needle));
        }

        [TestMethod]
        public void ArrayContainsArray_WithNeedleInEnd_ReturnsTrue()
        {
            var needle = new[] { 3, 4 };
            var haystack = new[] { 1, 2, 3, 4 };

            Assert.IsTrue(haystack.ContainsArray(needle));
        }

        [TestMethod]
        public void ArrayContainsArray_WithNeedleEqualsHaystack_ReturnsTrue()
        {
            var needle = new[] { 1, 2, 3, 4, 5 };
            var haystack = new[] { 1, 2, 3, 4, 5 };

            Assert.IsTrue(haystack.ContainsArray(needle));
        }
    }
}