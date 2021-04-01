using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
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


        [Test]
        public void UniqueMappingsOption_When_Set_Should_Replace_Default_Mappings()
        {
            var options = new PackageOptions { UniqueOnly = true };
            
            options.UniqueOnly.Should().BeTrue();
        }

        [Test]
        public void PackagesFilterOption_IncorrectRegexPackagesFilter_Should_Throw_ArgumentException()
        {
            var options = new PackageOptions
            {
                PackagesFilterOption = "/(?/",
            };

            Assert.Throws(typeof(ArgumentException), () => { var regex = options.PackageRegex; });
        }

        [Test]
        public void PackagesFilterOption_IncorrectPackagesFilterPath_Should_Throw_FileNotFoundException () {
            
            var options = new PackageOptions
            {
                PackagesFilterOption = @"../../../DoesNotExist.json",
            };

            Assert.Throws(typeof(FileNotFoundException), () => { var regex = options.PackageFilter; });
        }
    }
}
