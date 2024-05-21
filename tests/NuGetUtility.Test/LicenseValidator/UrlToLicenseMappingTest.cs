// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NuGetUtility.LicenseValidator;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace NuGetUtility.Test.LicenseValidator
{
    [TestFixture]
    public class UrlToLicenseMappingTest
    {
        [Parallelizable(scope: ParallelScope.All)]
        [TestCaseSource(typeof(UrlToLicenseMapping), nameof(UrlToLicenseMapping.Default))]
        public async Task License_Should_Be_Available_And_Match_Expected_License(KeyValuePair<Uri, string> mappedValue)
        {
            int retryCount = 0;
            while (true)
            {
                var options = new ChromeOptions();
                options.AddArguments("--no-sandbox", "--disable-dev-shm-usage", "--headless");
                var driver = new ChromeDriver(options);
                try
                {
                    driver.Navigate().GoToUrl(mappedValue.Key.ToString());

                    await Verify(driver.FindElement(By.TagName("body")).Text).HashParameters().UseStringComparer(CompareLicense);
                    return;
                }
                catch (WebDriverException e)
                {
                    if (retryCount >= 3)
                    {
                        throw;
                    }
                    retryCount++;
                    TestContext.Out.WriteLine($"Failed to check license for the {retryCount} time - retrying");
                    TestContext.Out.WriteLine(e);
                }
                finally
                {
                    driver.Quit();
                }
            }

        }

        private Task<CompareResult> CompareLicense(string received, string verified, IReadOnlyDictionary<string, object> context)
        {
            return Task.FromResult(new CompareResult((!string.IsNullOrWhiteSpace(verified)) && received.Contains(verified)));
        }
    }
}
