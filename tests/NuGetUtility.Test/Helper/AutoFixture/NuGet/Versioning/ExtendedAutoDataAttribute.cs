using AutoFixture;
using AutoFixture.NUnit3;
using NuGetUtility.Test.Extensions;

namespace NuGetUtility.Test.Helper.AutoFixture.NuGet.Versioning
{
    public class ExtendedAutoDataAttribute : AutoDataAttribute
    {
        public ExtendedAutoDataAttribute(params System.Type[] customizations)
            : base(() => new Fixture().AddCustomizations(customizations)) { }
    }
}
