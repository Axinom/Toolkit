namespace Tests.DotNet
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Input;
	using Axinom.Toolkit;
	using NUnit.Framework;

	[TestFixture]
	public class DelegateCommandTests
	{
		[Test]
		public void ExecuteIsCalled()
		{
			int callCount = 0;

			object expectedParameter = new object();
			object gotParameter = null;

			ICommand cmd = new DelegateCommand
			{
				Execute = delegate(object parameter)
				{
					callCount++;
					gotParameter = parameter;
				}
			};

			cmd.Execute(expectedParameter);

			Assert.AreEqual(1, callCount, "Execute() delegate was not called exactly once.");
			Assert.AreEqual(expectedParameter, gotParameter, "Execute() delegate got wrong parameter.");
		}

		[Test]
		public void CanExecuteIsCalled()
		{
			int callCount = 0;

			object expectedParameter = new object();
			object gotParameter = null;

			ICommand cmd = new DelegateCommand
			{
				CanExecute = delegate(object parameter)
				{
					callCount++;
					gotParameter = parameter;
					return false;
				}
			};

			var returnValue = cmd.CanExecute(expectedParameter);

			Assert.AreEqual(1, callCount, "CanExecute() delegate was not called exactly once.");
			Assert.AreEqual(expectedParameter, gotParameter, "CanExecute() delegate got wrong parameter.");
			Assert.IsFalse(returnValue, "CanExecute() returned wrong value.");
		}

		[Test]
		public void MissingExecuteDoesNothing()
		{
			ICommand cmd = new DelegateCommand
			{
				CanExecute = delegate { return true; } // Just in case, for isolation.
			};

			cmd.Execute(null);
		}

		[Test]
		public void MissingCanExecuteAlwaysAllows()
		{
			ICommand cmd = new DelegateCommand();

			Assert.IsTrue(cmd.CanExecute(null), "CanExecute() must be true when no CanExecuteDelegate given.");
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CannotExecuteWhenCanExecuteForbids()
		{
			bool wasExecuted = false;

			ICommand cmd = new DelegateCommand
			{
				Execute = delegate { wasExecuted = true; },
				CanExecute = delegate { return false; }
			};

			cmd.Execute(null);

			if (wasExecuted)
				Assert.Fail("Execute() was called even though CanExecute() was false and no exception occurred.");
			else
				Assert.Fail("Execute() was not called when CanExecute() was false but no exception happened either.");
		}

		[Test]
		public void CanExecuteChangedIsRaised()
		{
			bool wasCalled = false;

			ICommand cmd = new DelegateCommand();
			cmd.CanExecuteChanged += delegate { wasCalled = true; };

			((DelegateCommand)cmd).RaiseCanExecuteChanged();

			Assert.IsTrue(wasCalled, "CanExecuteChanged was not raised.");
		}
	}
}