using NuGetUtility.Output;
using NuGetUtility.Output.Json;

namespace NuGetUtility.Test.Output.Json
{

    [TestFixture(false, false)]
    [TestFixture(true, false)]
    [TestFixture(false, true)]
    [TestFixture(true, true)]
    public class JsonOutputFormatterTest : TestBase
    {
        private readonly bool _prettyPrint;
        private readonly bool _omitValidLicensesOnError;
        public JsonOutputFormatterTest(bool prettyPrint, bool omitValidLicensesOnError)
        {
            _prettyPrint = prettyPrint;
            _omitValidLicensesOnError = omitValidLicensesOnError;
        }
        protected override IOutputFormatter CreateUut()
        {
            return new JsonOutputFormatter(_prettyPrint, _omitValidLicensesOnError);
        }
    }
}
