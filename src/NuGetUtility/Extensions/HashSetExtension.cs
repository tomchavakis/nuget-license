// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

namespace NuGetUtility.Extensions
{
    public static class HashSetExtension
    {
        /// <summary>
        ///     Taken from https://stackoverflow.com/a/15267217/1199089
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static bool AddRange<T>(this HashSet<T> source, IEnumerable<T> items)
        {
            bool allAdded = true;
            foreach (T? item in items)
            {
                source.Add(item);
            }

            return allAdded;
        }
    }
}
