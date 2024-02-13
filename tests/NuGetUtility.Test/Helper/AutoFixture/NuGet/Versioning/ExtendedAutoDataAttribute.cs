// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

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
