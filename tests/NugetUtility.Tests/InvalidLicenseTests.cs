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
        [Test]
        public void Should_Format_Exceptions_On_NewLines()
        {
            var result = new ValidationResult
            {
                IsValid = false,
                InvalidPackages = new List<KeyValuePair<string, Package>>
                {
                    new KeyValuePair<string, Package>(@"c:\some\project.csproj",new Package{ Metadata = new Metadata {  Id = "BadLicense", Version = "0.1.0"} }),
                    new KeyValuePair<string, Package>(@"c:\some\project.csproj",new Package{ Metadata = new Metadata {  Id = "BadLicense2", Version = "0.1.0"} }),
                    new KeyValuePair<string, Package>(@"c:\some\project.csproj",new Package{ Metadata = new Metadata {  Id = "BadLicense3", Version = "0.1.0"} }),
                }
            };
            var exception = new InvalidLicensesException(result, null);

            Assert.IsTrue(exception.Message.Split(Environment.NewLine).Length == result.InvalidPackages.Count);
        }

        [Test]
        public void Should_Format_Exceptions_On_NewLines_With_Allowed_Header()
        {
            var result = new ValidationResult
            {
                IsValid = false,
                InvalidPackages = new List<KeyValuePair<string, Package>>
                {
                    new KeyValuePair<string, Package>(@"c:\some\project.csproj",new Package{ Metadata = new Metadata {  Id = "BadLicense", Version = "0.1.0"} }),
                    new KeyValuePair<string, Package>(@"c:\some\project.csproj",new Package{ Metadata = new Metadata {  Id = "BadLicense2", Version = "0.1.0"} }),
                    new KeyValuePair<string, Package>(@"c:\some\project.csproj",new Package{ Metadata = new Metadata {  Id = "BadLicense3", Version = "0.1.0"} }),
                }
            };
            var exception = new InvalidLicensesException(result, new List<string> { "MIT" });

            Assert.IsTrue(exception.Message.Split(Environment.NewLine).Length == result.InvalidPackages.Count + 1);
        }
    }
}