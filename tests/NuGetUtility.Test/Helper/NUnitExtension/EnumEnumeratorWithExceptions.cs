// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using System.Collections;

namespace NuGetUtility.Test.Helper.NUnitExtension
{
    internal class EnumEnumeratorWithExceptions : IEnumerator
    {
        private readonly object[] _exceptions;
        private readonly IEnumerator _internalEnumerator;

        public EnumEnumeratorWithExceptions(Array allEnumOptions, object[] exceptions)
        {
            _internalEnumerator = allEnumOptions.GetEnumerator();
            _exceptions = exceptions;
        }

        public bool MoveNext()
        {
            while (_internalEnumerator.MoveNext())
            {
                if (!IsException(Current))
                {
                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            _internalEnumerator.Reset();
        }

        public object Current => _internalEnumerator.Current!;

        private bool IsException(object current)
        {
            foreach (object exception in _exceptions)
            {
                if (exception.Equals(current))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
