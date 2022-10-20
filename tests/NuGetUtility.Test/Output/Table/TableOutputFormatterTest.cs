using NuGetUtility.Output;
using NuGetUtility.Output.Table;

namespace NuGetUtility.Test.Output.Table
{
    [TestFixture(false)]
    [TestFixture(true)]
    public class TableOutputFormatterTest : TestBase
    {
        private readonly bool _omitValidLicensesOnError;
        public TableOutputFormatterTest(bool omitValidLicensesOnError)
        {
            _omitValidLicensesOnError = omitValidLicensesOnError;
        }
        protected override IOutputFormatter CreateUut()
        {
            return new TableOutputFormatter(_omitValidLicensesOnError);
        }
    }
}
