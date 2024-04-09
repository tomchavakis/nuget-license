// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NuGetUtility.Output;
using NuGetUtility.Output.Table;

namespace NuGetUtility.Test.Output.Table
{
    [TestFixture(true, true, true, true, true)]
    [TestFixture(true, true, true, true, false)]
    [TestFixture(true, true, true, false, true)]
    [TestFixture(true, true, true, false, false)]
    [TestFixture(true, true, false, true, true)]
    [TestFixture(true, true, false, true, false)]
    [TestFixture(true, true, false, false, true)]
    [TestFixture(true, true, false, false, false)]
    [TestFixture(true, false, true, true, true)]
    [TestFixture(true, false, true, true, false)]
    [TestFixture(true, false, true, false, true)]
    [TestFixture(true, false, true, false, false)]
    [TestFixture(true, false, false, true, true)]
    [TestFixture(true, false, false, true, false)]
    [TestFixture(true, false, false, false, true)]
    [TestFixture(true, false, false, false, false)]
    [TestFixture(false, true, true, true, true)]
    [TestFixture(false, true, true, true, false)]
    [TestFixture(false, true, true, false, true)]
    [TestFixture(false, true, true, false, false)]
    [TestFixture(false, true, false, true, true)]
    [TestFixture(false, true, false, true, false)]
    [TestFixture(false, true, false, false, true)]
    [TestFixture(false, true, false, false, false)]
    [TestFixture(false, false, true, true, true)]
    [TestFixture(false, false, true, true, false)]
    [TestFixture(false, false, true, false, true)]
    [TestFixture(false, false, true, false, false)]
    [TestFixture(false, false, false, true, true)]
    [TestFixture(false, false, false, true, false)]
    [TestFixture(false, false, false, false, true)]
    [TestFixture(false, false, false, false, false)]
    public class TableOutputFormatterTest : TestBase
    {
        private readonly bool _omitValidLicensesOnError;
        private readonly bool _skipIgnoredPackages;

        public TableOutputFormatterTest(bool omitValidLicensesOnError, bool skipIgnoredPackages, bool includeCopyright, bool includeAuthors, bool includeLicenseUrl) : base(includeCopyright, includeAuthors, includeLicenseUrl)
        {
            _omitValidLicensesOnError = omitValidLicensesOnError;
            _skipIgnoredPackages = skipIgnoredPackages;
        }
        protected override IOutputFormatter CreateUut()
        {
            return new TableOutputFormatter(_omitValidLicensesOnError, _skipIgnoredPackages);
        }
    }
}
