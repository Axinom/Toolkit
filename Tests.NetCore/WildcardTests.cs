using Axinom.Toolkit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public sealed class WildcardTests : TestClass
    {
        [TestMethod]
        public void SingleMatch_MatchesWhenExpected()
        {
            var wildcard = new Wildcard("abc???_tere*");

            Assert.IsTrue(wildcard.IsMatch("abcabc_tere"));
            Assert.IsTrue(wildcard.IsMatch("abcabc_tere13451451345"));
            Assert.IsTrue(wildcard.IsMatch("abcabc_tere13451451345|||qqqq"));

            Assert.IsFalse(wildcard.IsMatch("abcabcd_tere13451451345|||qqqq"));
            Assert.IsFalse(wildcard.IsMatch("helloabcabc_tere!"));
        }

        public void MultipleMatch_MatchesWhenExpected()
        {
            var wildcard = new Wildcard("first?_*_xyz", "*second_?");

            Assert.IsTrue(wildcard.IsMatch("first1_tere_xyz"));
            Assert.IsTrue(wildcard.IsMatch("first1__xyz"));
            Assert.IsTrue(wildcard.IsMatch("anotherseccond_1"));

            Assert.IsFalse(wildcard.IsMatch("anotherseccond_1not"));
        }
    }
}
