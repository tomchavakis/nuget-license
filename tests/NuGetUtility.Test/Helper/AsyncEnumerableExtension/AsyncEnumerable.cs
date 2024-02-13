// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

namespace NuGetUtility.Test.Helper.AsyncEnumerableExtension
{
    internal class AsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly IEnumerable<T> _synchronous;

        public AsyncEnumerable(IEnumerable<T> synchronous)
        {
            _synchronous = synchronous;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            return new AsyncEnumerator<T>(_synchronous.GetEnumerator());
        }
    }
}
