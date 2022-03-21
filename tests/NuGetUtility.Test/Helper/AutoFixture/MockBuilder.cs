using AutoFixture.Kernel;
using Moq;
using NuGetUtility.Test.Helper.Type;

namespace NuGetUtility.Test.Helper.AutoFixture
{
    internal class MockBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is System.Type t)
            {
                if (t.IsOfGenericType(typeof(Mock<>)))
                {
                    return Activator.CreateInstance(t)!;
                }
            }

            return new NoSpecimen();
        }
    }
}
