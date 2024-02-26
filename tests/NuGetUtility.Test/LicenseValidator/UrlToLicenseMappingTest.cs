// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using AngleSharp;
using AngleSharp.Dom;
using NuGetUtility.LicenseValidator;

namespace NuGetUtility.Test.LicenseValidator
{
    [TestFixture]
    public class UrlToLicenseMappingTest
    {
        [Ignore("this test is not yet stable enough")]
        [Parallelizable(scope: ParallelScope.All)]
        [TestCaseSource(typeof(UrlToLicenseMapping), nameof(UrlToLicenseMapping.Default))]
        public async Task License_Should_Be_Available_And_Match_Expected_License(KeyValuePair<Uri, string> mappedValue)
        {
            IConfiguration config = Configuration.Default.WithDefaultLoader();
            IBrowsingContext context = BrowsingContext.New(config);
            IDocument document = await context.OpenAsync(mappedValue.Key.ToString());

            await Verify(document.Body?.TextContent).HashParameters().UseStringComparer(CompareLicense);
        }

        private Task<CompareResult> CompareLicense(string received, string verified, IReadOnlyDictionary<string, object> context)
        {
            return Task.FromResult(new CompareResult((!string.IsNullOrWhiteSpace(verified)) && received.Contains(verified)));
        }
    }
}
