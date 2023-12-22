using System.Collections;
using NUnit.Framework.Interfaces;

namespace NuGetUtility.Test.Helper.NUnitExtension
{
    [AttributeUsage(AttributeTargets.Parameter)]
    internal class EnumValuesExceptAttribute : NUnitAttribute, IParameterDataSource
    {
        private readonly object[] _exceptions;

        public EnumValuesExceptAttribute(params object[] exceptions)
        {
            _exceptions = exceptions;
        }

        public IEnumerable GetData(IParameterInfo parameter)
        {
            return new EnumEnumerableWithException(parameter.ParameterType, _exceptions);
        }
    }
}
