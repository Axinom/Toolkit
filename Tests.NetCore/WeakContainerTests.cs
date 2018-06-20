namespace Tests
{
    using Axinom.Toolkit;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class WeakContainerTests : BaseTestClass
    {
        [TestMethod]
        public void BasicOperations()
        {
            var container = new WeakContainer<object>();

            object item = new object();

            Assert.IsFalse(container.Contains(item), "WeakContainer contained item before it was added.");

            container.Add(item);

            Assert.IsTrue(container.Contains(item), "WeakContainer did not contain item after adding it.");

            int count = 0;

            foreach (var o in container)
            {
                count++;
            }

            Assert.AreEqual(1, count);

            container.Remove(item);

            Assert.IsFalse(container.Contains(item), "WeakContainer contained item after it was removed.");
        }

        [TestMethod]
        public void DeadItemsAreForgotten()
        {
#if RELEASE || !DEBUG
			var container = new WeakContainer<Bongo>();

			// Item to forget.
			Bongo item1 = new Bongo();
			// Item to keep.
			Bongo item2 = new Bongo();

			container.Add(item1);
			container.Add(item2);

			int count = 0;

			foreach (var o in container)
			{
				count++;
			}

			Assert.AreEqual(2, count);

			item1 = null; // Die!
			System.GC.Collect();

			count = 0;

			foreach (var o in container)
			{
				count++;
			}

			Assert.AreEqual(1, count);

			Assert.IsTrue(container.Contains(item2));
#else
            Assert.Inconclusive("This test is not reliable in debug build, as objects may live longer than they should.");
#endif
        }
    }

    internal class Bongo
    {
    }
}