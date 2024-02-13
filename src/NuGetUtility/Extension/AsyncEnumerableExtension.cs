// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

namespace NuGetUtility.Extension
{
    public static class AsyncEnumerableExtension
    {
        public static async IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(this IAsyncEnumerable<TSource> input,
            Func<TSource, IAsyncEnumerable<TResult>> transform)
        {
            await foreach (TSource? value in input)
            {
                await foreach (TResult? transformedValue in transform(value))
                {
                    yield return transformedValue;
                }
            }
        }
        public static async IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> input,
            Func<TSource, IAsyncEnumerable<TResult>> transform)
        {
            foreach (TSource? value in input)
            {
                await foreach (TResult? transformedValue in transform(value))
                {
                    yield return transformedValue;
                }
            }
        }
    }
}
