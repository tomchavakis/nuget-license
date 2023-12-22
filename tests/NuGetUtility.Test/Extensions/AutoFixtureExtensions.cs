// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using AutoFixture;
using AutoFixture.Kernel;

namespace NuGetUtility.Test.Extensions
{
    internal static class AutoFixtureExtensions
    {
        public static IFixture AddCustomizations(this Fixture fixture, params Type[] customizations)
        {
            foreach (Type customization in customizations)
            {
                if (Activator.CreateInstance(customization) is ISpecimenBuilder builder)
                {
                    fixture.Customizations.Add(builder);
                }
            }

            return fixture;
        }
    }
}
