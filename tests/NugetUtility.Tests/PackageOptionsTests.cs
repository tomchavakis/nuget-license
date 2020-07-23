using FluentAssertions;
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

            options.LicenseToUrlMappingsDictionary.Should().HaveCount(2)
                .And.BeEquivalentTo(testMappings);
        }

    }
}
