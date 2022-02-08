using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.NUnit3;

namespace NuGetUtility.Test.Helper.AutoFixture.NuGet.Versioning
{
    public class ExtendedInlineAutoDataAttribute : InlineAutoDataAttribute
    {
        public ExtendedInlineAutoDataAttribute(Type customization, params object[] arguments)
            : base(() => CreateFixture(customization), arguments) { }

        private static IFixture CreateFixture(Type customization)
        {
            var fixture = new Fixture();

            if (Activator.CreateInstance(customization) is ISpecimenBuilder builder)
            {
                fixture.Customizations.Add(builder);
            }

            return fixture;
        }
    }
}
