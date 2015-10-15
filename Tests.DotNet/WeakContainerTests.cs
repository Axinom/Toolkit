namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Axinom.Toolkit;
	using Xunit;

	public sealed class WeakContainerTests : TestClass
	{
		[Fact]
		public void BasicOperations()
		{
			var container = new WeakContainer<object>();

			object item = new object();

			Assert.False(container.Contains(item), "WeakContainer contained item before it was added.");

			container.Add(item);

			Assert.True(container.Contains(item), "WeakContainer did not contain item after adding it.");

			int count = 0;

			foreach (var o in container)
			{
				count++;
			}

			Assert.Equal(1, count);

			container.Remove(item);

			Assert.False(container.Contains(item), "WeakContainer contained item after it was removed.");
		}

		[Fact]
		public void DeadItemsAreForgotten()
		{
			// This test is not reliable in debug build, as objects may live longer than they should.

#if !DEBUG
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

			Assert.Equal(2, count);

			item1 = null; // Die!
			GC.Collect();

			count = 0;

			foreach (var o in container)
			{
				count++;
			}

			Assert.Equal(1, count);

			Assert.True(container.Contains(item2));
#endif
		}
	}

	internal class Bongo
	{
	}
}