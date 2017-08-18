namespace Tests
{
    using Axinom.Toolkit;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class EnvironmentTests : TestClass
    {
        [TestMethod]
        public void IsMicrosoftOperatingSystem_IsOppositeOfIsNonMicrosoftOperatingSystem()
        {
            Assert.AreNotEqual(Helpers.Environment.IsNonMicrosoftOperatingSystem(), Helpers.Environment.IsMicrosoftOperatingSystem());
        }
    }
}