// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NuGetUtility.Output;
using NuGetUtility.Output.Table;

namespace NuGetUtility.Test.Output.Table
{
    [TestFixture(false, false)]
    [TestFixture(false, true)]
    [TestFixture(true, false)]
    [TestFixture(true, true)]
    public class TableOutputFormatterTest : TestBase
    {
        private readonly bool _omitValidLicensesOnError;
        private readonly bool _skipIgnoredPackages;

        public TableOutputFormatterTest(bool omitValidLicensesOnError, bool skipIgnoredPackages)
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
