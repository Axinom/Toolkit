namespace Tests
{
    using Axinom.Toolkit;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Windows.Input;

    [TestClass]
    public sealed class DelegateCommandTests : BaseTestClass
    {
        [TestMethod]
        public void ExecuteIsCalled()
        {
            int callCount = 0;

            object expectedParameter = new object();
            object gotParameter = null;

            ICommand cmd = new DelegateCommand
            {
                Execute = delegate (object parameter)
                {
                    callCount++;
                    gotParameter = parameter;
                }
            };

            cmd.Execute(expectedParameter);

            Assert.AreEqual(1, callCount);
            Assert.AreEqual(expectedParameter, gotParameter);
        }

        [TestMethod]
        public void CanExecuteIsCalled()
        {
            int callCount = 0;

            object expectedParameter = new object();
            object gotParameter = null;

            ICommand cmd = new DelegateCommand
            {
                CanExecute = delegate (object parameter)
                {
                    callCount++;
                    gotParameter = parameter;
                    return false;
                }
            };

            var returnValue = cmd.CanExecute(expectedParameter);

            Assert.AreEqual(1, callCount);
            Assert.AreEqual(expectedParameter, gotParameter);
            Assert.IsFalse(returnValue);
        }

        [TestMethod]
        public void MissingExecuteDoesNothing()
        {
            ICommand cmd = new DelegateCommand
            {
                CanExecute = delegate { return true; } // Just in case, for isolation.
            };

            cmd.Execute(null);
        }

        [TestMethod]
        public void MissingCanExecuteAlwaysAllows()
        {
            ICommand cmd = new DelegateCommand();

            Assert.IsTrue(cmd.CanExecute(null));
        }

        [TestMethod]
        public void CannotExecuteWhenCanExecuteForbids()
        {
            ICommand cmd = new DelegateCommand
            {
                Execute = delegate { },
                CanExecute = delegate { return false; }
            };

            Assert.ThrowsException<InvalidOperationException>(() => cmd.Execute(null));
        }

        [TestMethod]
        public void CanExecuteChangedIsRaised()
        {
            bool wasCalled = false;

            ICommand cmd = new DelegateCommand();
            cmd.CanExecuteChanged += delegate { wasCalled = true; };

            ((DelegateCommand)cmd).RaiseCanExecuteChanged();

            Assert.IsTrue(wasCalled);
        }
    }
}