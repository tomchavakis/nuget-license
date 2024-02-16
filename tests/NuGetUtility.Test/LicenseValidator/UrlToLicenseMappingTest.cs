// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using System.Runtime.CompilerServices;
using AngleSharp.Dom;
using NuGetUtility.LicenseValidator;
using VerifyTests.AngleSharp;

namespace NuGetUtility.Test.LicenseValidator
{
    [TestFixture]
    public class UrlToLicenseMappingTest
    {
        [ModuleInitializer]
        public static void InitializeAngleSharpDiffing() =>
            VerifyAngleSharpDiffing.Initialize();

        [Parallelizable(scope: ParallelScope.All)]
        [TestCaseSource(typeof(UrlToLicenseMapping), nameof(UrlToLicenseMapping.Default))]
        public async Task ValidatedLicenses_Should_PrintCorrectTable(KeyValuePair<Uri, string> mappedValue)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, mappedValue.Key);

            HttpResponseMessage response = await new HttpClient().SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            string html = await response.Content.ReadAsStringAsync();

            await Verify(html, "html").PrettyPrintHtml(nodes =>
            {
                foreach (IElement node in nodes.QuerySelectorAll("head"))
                {
                    node.Remove();
                }
                foreach (IElement node in nodes.QuerySelectorAll("script"))
                {
                    node.Remove();
                }
                foreach (IElement node in nodes.QuerySelectorAll("meta"))
                {
                    node.Remove();
                }
                foreach (IElement node in nodes.QuerySelectorAll("qbsearch-input"))
                {
                    node.Remove();
                }
                foreach (IElement node in nodes.QuerySelectorAll("input").Where(e => e.Attributes.Any(a => a.Name == "type" && a.Value == "hidden")))
                {
                    node.Remove();
                }
                nodes.ScrubAttributes("aria-describedby");
                nodes.ScrubAttributes("data-delete-custom-scopes-csrf");
                nodes.ScrubAttributes("id");
                nodes.ScrubAttributes("for");
                nodes.ScrubAttributes("anchor");
                nodes.ScrubAttributes("aria-labelledby");
                nodes.ScrubAttributes("popovertarget");
                nodes.ScrubAttributes("aria-controls");
                nodes.ScrubAttributes("data-cookie-consent-required");
                nodes.ScrubEmptyDivs();
            }).HashParameters();
        }
    }
}
