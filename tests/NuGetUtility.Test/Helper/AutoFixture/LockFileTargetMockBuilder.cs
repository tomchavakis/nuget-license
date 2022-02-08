using AutoFixture.Kernel;
using Moq;

namespace NuGetUtility.Test.Helper.AutoFixture
{
    internal class MockBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is Type t)
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
