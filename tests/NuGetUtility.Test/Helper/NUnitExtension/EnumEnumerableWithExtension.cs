using System.Collections;

namespace NuGetUtility.Test.Helper.NUnitExtension
{
    internal class EnumEnumerableWithExtension : IEnumerable
    {
        private readonly Array _allEnumOptions;
        private readonly object[] _exceptions;

        public EnumEnumerableWithExtension(Type t, object[] exceptions)
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
