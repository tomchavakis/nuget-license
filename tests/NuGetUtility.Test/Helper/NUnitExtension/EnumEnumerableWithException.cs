// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using System.Collections;

namespace NuGetUtility.Test.Helper.NUnitExtension
{
    internal class EnumEnumerableWithException : IEnumerable
    {
        private readonly Array _allEnumOptions;
        private readonly object[] _exceptions;

        public EnumEnumerableWithException(System.Type t, object[] exceptions)
        {
            _exceptions = exceptions;
            _allEnumOptions = Enum.GetValues(t);
        }

        public IEnumerator GetEnumerator()
        {
            return new EnumEnumeratorWithExceptions(_allEnumOptions, _exceptions);
        }
    }
}
