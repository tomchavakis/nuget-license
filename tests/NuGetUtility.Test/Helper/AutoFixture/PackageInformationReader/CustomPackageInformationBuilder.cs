using AutoFixture.Kernel;
using NuGet.Versioning;
using NuGetUtility.PackageInformationReader;

namespace NuGetUtility.Test.Helper.AutoFixture.PackageInformationReader
{
    internal class CustomPackageInformationBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is System.Type t)
            {
                if (t == typeof(CustomPackageInformation))
                {
                    return new CustomPackageInformation(Resolve<string>(context), Resolve<NuGetVersion>(context),
                        Resolve<string>(context), Resolve<Version>(context).ToString());
                }
            }

            return new NoSpecimen();
        }

        private T Resolve<T>(ISpecimenContext c) where T : class
        {
            return (c.Resolve(typeof(T)) as T)!;
        }
    }
}
