using System;

namespace Axinom.Toolkit
{
    public sealed class DelegatingDisposable : IDisposable
    {
        public DelegatingDisposable(Action onDispose)
        {
            _onDispose = onDispose;
        }

        private readonly Action _onDispose;

        public void Dispose()
        {
            _onDispose?.Invoke();
        }
    }
}
