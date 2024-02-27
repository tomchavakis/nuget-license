// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NuGetUtility.LicenseValidator;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace NuGetUtility.Test.LicenseValidator
{
    [TestFixture]
    public class UrlToLicenseMappingTest
    {
        [Parallelizable(scope: ParallelScope.All)]
        [TestCaseSource(typeof(UrlToLicenseMapping), nameof(UrlToLicenseMapping.Default))]
        public async Task License_Should_Be_Available_And_Match_Expected_License(KeyValuePair<Uri, string> mappedValue)
        {
            var options = new ChromeOptions();
            options.AddArguments("--no-sandbox", "--disable-dev-shm-usage", "--headless");
            var driver = new ChromeDriver(options);
            try
            {
                driver.Navigate().GoToUrl(mappedValue.Key.ToString());

                IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

                await Verify(driver.FindElement(By.TagName("body")).Text).HashParameters().UseStringComparer(CompareLicense);
            }
            finally
            {
                driver.Quit();
            }
        }

        private Task<CompareResult> CompareLicense(string received, string verified, IReadOnlyDictionary<string, object> context)
        {
            return Task.FromResult(new CompareResult((!string.IsNullOrWhiteSpace(verified)) && received.Contains(verified)));
        }
    }
}
