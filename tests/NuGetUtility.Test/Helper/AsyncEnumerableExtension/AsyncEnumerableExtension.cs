namespace NuGetUtility.Test.Helper.AsyncEnumerableExtension
{
    internal static class AsyncEnumerableExtension
    {
        public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> synchronous)
        {
            return new AsyncEnumerable<T>(synchronous);
        }
    }
}
