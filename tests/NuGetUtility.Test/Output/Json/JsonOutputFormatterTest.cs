// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NuGetUtility.Output;
using NuGetUtility.Output.Json;

namespace NuGetUtility.Test.Output.Json
{
    [TestFixture(false, false, false)]
    [TestFixture(true, false, false)]
    [TestFixture(false, true, false)]
    [TestFixture(true, true, false)]
    [TestFixture(false, false, true)]
    [TestFixture(true, false, true)]
    [TestFixture(false, true, true)]
    [TestFixture(true, true, true)]
    public class JsonOutputFormatterTest : TestBase
    {
        private readonly bool _prettyPrint;
        private readonly bool _omitValidLicensesOnError;
        private readonly bool _skipIgnoredPackages;

        public JsonOutputFormatterTest(bool prettyPrint, bool omitValidLicensesOnError, bool skipIgnoredPackages)
        {
            _prettyPrint = prettyPrint;
            _omitValidLicensesOnError = omitValidLicensesOnError;
            _skipIgnoredPackages = skipIgnoredPackages;
        }
        protected override IOutputFormatter CreateUut()
        {
            return new JsonOutputFormatter(_prettyPrint, _omitValidLicensesOnError, _skipIgnoredPackages);
        }
    }
}
