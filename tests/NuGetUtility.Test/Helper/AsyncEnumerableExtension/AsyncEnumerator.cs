// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

namespace NuGetUtility.Test.Helper.AsyncEnumerableExtension
{
    internal class AsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _sync;

        public AsyncEnumerator(IEnumerator<T> sync)
        {
            _sync = sync;
        }

        public ValueTask DisposeAsync()
        {
            _sync.Dispose();
#if NETFRAMEWORK
            return new ValueTask(Task.CompletedTask);
#else
            return ValueTask.CompletedTask;
#endif
        }

        public ValueTask<bool> MoveNextAsync()
        {
            bool result = _sync.MoveNext();
#if NETFRAMEWORK
            return new ValueTask<bool>(Task.FromResult(result));
#else
            return ValueTask.FromResult(result);
#endif
        }

        public T Current => _sync.Current;
    }
}
