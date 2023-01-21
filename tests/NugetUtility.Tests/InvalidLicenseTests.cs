using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NugetUtility.Tests
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class InvalidLicenseTests
    {
        [TestCase(true)]
        [TestCase(false)]
        [Test]
        public void Should_Format_Exceptions_On_NewLines_With_Allowed_Header(bool hasAllowed)
        {
            var result = new ValidationResult<KeyValuePair<string, Package>>
            {
                IsValid = false,
                InvalidPackages = new List<KeyValuePair<string, Package>>
                {
                    new(@"c:\some\project.csproj", new Package {Metadata = new Metadata {Id = "BadLicense", Version = "0.1.0"}}),
                    new(@"c:\some\project.csproj", new Package {Metadata = new Metadata {Id = "BadLicense2", Version = "0.1.0"}}),
                    new(@"c:\some\project.csproj", new Package {Metadata = new Metadata {Id = "BadLicense3", Version = "0.1.0"}}),
                }
            };

            var options = new PackageOptions
            {
                AllowedLicenseTypesOption = @"../../../SampleAllowedLicenses.json"
            };
            
            var exception = new InvalidLicensesException<KeyValuePair<string, Package>>(result, !hasAllowed ? null : options);

            exception.Should().NotBeNull();
            exception.Message.Split(Environment.NewLine)
                .Should().HaveCount(result.InvalidPackages.Count + 1);
        }
    }
}