namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Input;
	using Axinom.Toolkit;
	using Xunit;

	public sealed class DelegateCommandTests : TestClass
	{
		[Fact]
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

			Assert.Equal(1, callCount);
			Assert.Equal(expectedParameter, gotParameter);
		}

		[Fact]
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

			Assert.Equal(1, callCount);
			Assert.Equal(expectedParameter, gotParameter);
			Assert.False(returnValue);
		}

		[Fact]
		public void MissingExecuteDoesNothing()
		{
			ICommand cmd = new DelegateCommand
			{
				CanExecute = delegate { return true; } // Just in case, for isolation.
			};

			cmd.Execute(null);
		}

		[Fact]
		public void MissingCanExecuteAlwaysAllows()
		{
			ICommand cmd = new DelegateCommand();

			Assert.True(cmd.CanExecute(null));
		}

		[Fact]
		public void CannotExecuteWhenCanExecuteForbids()
		{
			ICommand cmd = new DelegateCommand
			{
				Execute = delegate { },
				CanExecute = delegate { return false; }
			};

			Assert.Throws<InvalidOperationException>(() => cmd.Execute(null));
		}

		[Fact]
		public void CanExecuteChangedIsRaised()
		{
			bool wasCalled = false;

			ICommand cmd = new DelegateCommand();
			cmd.CanExecuteChanged += delegate { wasCalled = true; };

			((DelegateCommand)cmd).RaiseCanExecuteChanged();

			Assert.True(wasCalled);
		}
	}
}