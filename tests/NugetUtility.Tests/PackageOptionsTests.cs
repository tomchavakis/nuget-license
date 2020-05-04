using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace NugetUtility.Tests
{
    [TestFixture]
    public class PackageOptionsTests
    {
        [Test]
        public void LicenseToUrlMappingsOption_When_Set_Should_Replace_Default_Mappings()
        {
            var testMappings = new Dictionary<string, string>
            {
                {"url1","license1" },
                {"url2","license1" },
            };
            var testFile = "test-mappings.json";
            File.WriteAllText(testFile, JsonConvert.SerializeObject(testMappings));

            var options = new PackageOptions { LicenseToUrlMappingsOption = testFile };

            Assert.IsTrue(options.LicenseToUrlMappingsDictionary.Count == 2);
            Assert.AreEqual(options.LicenseToUrlMappingsDictionary["url1"], "license1");
            Assert.AreEqual(options.LicenseToUrlMappingsDictionary["url2"], "license1");
            File.Delete(testFile);
        }

    }
}
