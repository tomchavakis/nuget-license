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
            var result = new ValidationResult<KeyValuePair<string,Package>>
            {
                IsValid = false,
                InvalidPackages = new List<KeyValuePair<string, Package>>
                {
                    new KeyValuePair<string, Package>(@"c:\some\project.csproj",new Package{ Metadata = new Metadata {  Id = "BadLicense", Version = "0.1.0"} }),
                    new KeyValuePair<string, Package>(@"c:\some\project.csproj",new Package{ Metadata = new Metadata {  Id = "BadLicense2", Version = "0.1.0"} }),
                    new KeyValuePair<string, Package>(@"c:\some\project.csproj",new Package{ Metadata = new Metadata {  Id = "BadLicense3", Version = "0.1.0"} }),
                }
            };
            var exception = new InvalidLicensesException<KeyValuePair<string,Package>>(result, !hasAllowed ? null : new List<string> { "MIT" });

            exception.Should().NotBeNull();
            exception.Message.Split(Environment.NewLine)
                .Should().HaveCount(result.InvalidPackages.Count + 1);
        }
    }
}