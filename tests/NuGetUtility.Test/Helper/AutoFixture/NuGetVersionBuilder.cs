using AutoFixture.Kernel;
using NuGet.Versioning;

namespace NuGetUtility.Test.Helper.AutoFixture
{
    internal class NuGetVersionBuilder : ISpecimenBuilder
    {
        private readonly Random _rnd = new Random();

        public object Create(object request, ISpecimenContext context)
        {
            if (request is Type t)
            {
                if (t == typeof(NuGetVersion))
                {
                    return new NuGetVersion($"{_rnd.Next(100, 999)}.{_rnd.Next(100, 999)}{GetPatch()}{GetBeta()}");
                }
            }

            return new NoSpecimen();
        }

        private string GetBeta()
        {
            return RandomBool() ? $"-beta.{_rnd.Next(100, 999)}" : "";
        }

        private string GetPatch()
        {
            return RandomBool() ? $".{_rnd.Next(100, 999)}" : "";
        }

        private bool RandomBool()
        {
            return (_rnd.Next() % 2) == 0;
        }
    }
}
