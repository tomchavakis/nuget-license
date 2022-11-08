using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.NUnit3;

namespace NuGetUtility.Test.Helper.AutoFixture.NuGet.Versioning
{
    public class ExtendedAutoDataAttribute : AutoDataAttribute
    {
        public ExtendedAutoDataAttribute(params System.Type[] customizations)
            : base(() => CreateFixture(customizations)) { }

        private static IFixture CreateFixture(System.Type[] customizations)
        {
            var fixture = new Fixture();

            foreach (System.Type customization in customizations)
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
