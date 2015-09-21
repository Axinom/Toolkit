namespace Tests.DotNet
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using Axinom.Toolkit;
	using NUnit.Framework;

	[TestFixture]
	public sealed class WeakContainerTests
	{
		[Test]
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

			Assert.AreEqual(1, count, "WeakContainer did not contain exactly 1 item after it was added.");

			container.Remove(item);

			Assert.IsFalse(container.Contains(item), "WeakContainer contained item after it was removed.");
		}

		[Test]
		public void DeadItemsAreForgotten()
		{
			InconclusiveInDebugBuild(); // GC might not clean the dead item up in debug build.

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

			Assert.AreEqual(2, count, "WeakContainer did not contain exactly 2 items after they were added.");

			item1 = null; // Die!
			GC.Collect();

			count = 0;

			foreach (var o in container)
			{
				count++;
			}

			Assert.AreEqual(1, count, "WeakContainer did not contain exactly 1 item after one dead item was GCed.");

			Assert.IsTrue(container.Contains(item2), "WeakContainer did not contain alive item after another item was GCed.");
		}

		[Conditional("DEBUG")]
		private void InconclusiveInDebugBuild()
		{
			Assert.Inconclusive("This test requires the tests to be built with a release configuration.");
		}
	}

	internal class Bongo
	{
	}
}