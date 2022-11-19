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
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            bool result = _sync.MoveNext();
            return ValueTask.FromResult(result);
        }

        public T Current => _sync.Current;
    }
}
