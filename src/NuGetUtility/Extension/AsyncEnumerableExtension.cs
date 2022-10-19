namespace NuGetUtility.Extension
{
    public static class AsyncEnumerableExtension
    {
        public static async IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(this IAsyncEnumerable<TSource> input,
            Func<TSource, IAsyncEnumerable<TResult>> transform)
        {
            await foreach (var value in input)
            {
                await foreach (var transformedValue in transform(value))
                {
                    yield return transformedValue;
                }
            }
        }
        public static async IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> input,
            Func<TSource, IAsyncEnumerable<TResult>> transform)
        {
            foreach (var value in input)
            {
                await foreach (var transformedValue in transform(value))
                {
                    yield return transformedValue;
                }
            }
        }
    }
}
