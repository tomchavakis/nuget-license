namespace NuGetUtility.Test.Helper.AsyncEnumerableExtension
{
    internal static class AsyncEnumerableExtension
    {
        public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> synchronous)
        {
            return new AsyncEnumerable<T>(synchronous);
        }

        public static async Task<IEnumerable<T>> Synchronize<T>(this IAsyncEnumerable<T> async)
        {
            var list = new List<T>();
            await foreach (T? item in async)
            {
                lock (list)
                {
                    list.Add(item);
                }
            }

            return list;
        }
    }
}
