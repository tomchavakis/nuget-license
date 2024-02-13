// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using HtmlAgilityPack;
using NuGetUtility.LicenseValidator;

namespace NuGetUtility.Test.LicenseValidator
{
    [TestFixture]
    public class UrlToLicenseMappingTest
    {
        [Parallelizable(scope: ParallelScope.All)]
        [TestCaseSource(typeof(UrlToLicenseMapping), nameof(UrlToLicenseMapping.Default))]
        public async Task ValidatedLicenses_Should_PrintCorrectTable(KeyValuePair<Uri, string> mappedValue)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, mappedValue.Key);

            HttpResponseMessage response = await new HttpClient().SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            using Stream downloadStream = await response.Content.ReadAsStreamAsync();

            HtmlDocument doc = new HtmlDocument();
            doc.Load(downloadStream);

            await Verify(doc.DocumentNode.InnerText).HashParameters();
        }
    }
}
