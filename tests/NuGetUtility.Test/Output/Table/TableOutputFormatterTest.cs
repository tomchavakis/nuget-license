using Bogus;
using NuGet.Versioning;
using NuGetUtility.LicenseValidator;
using NuGetUtility.Output;
using NuGetUtility.Output.Table;
using NuGetUtility.Test.Extensions;

namespace NuGetUtility.Test.Output.Table
{
    [TestFixture]
    public class TableOutputFormatterTest
    {
        [SetUp]
        public void SetUp()
        {
            _validatedLicenseFaker = new Faker<ValidatedLicense>().CustomInstantiator(f =>
                    new ValidatedLicense(f.Name.JobTitle(),
                        new NuGetVersion(f.System.Semver()),
                        f.Hacker.Phrase(),
                        f.Random.Enum<LicenseInformationOrigin>(),
                        new Uri(f.Internet.Url())))
                .UseSeed(8675309);
            _licenseValidationErrorFaker = new Faker<LicenseValidationError>().CustomInstantiator(f =>
                    new LicenseValidationError(f.System.FilePath(),
                        f.Name.FullName(),
                        new NuGetVersion(f.System.Semver()),
                        f.Lorem.Sentence()))
                .UseSeed(9078345);
            _uut = new TableOutputFormatter(false);
        }
        private IOutputFormatter _uut = null!;
        private Faker<ValidatedLicense> _validatedLicenseFaker = null!;
        private Faker<LicenseValidationError> _licenseValidationErrorFaker = null!;

        [Test]
        public async Task Errors_Should_PrintCorrectTable([Values(0, 1, 5, 20)] int errorCount)
        {
            using var stream = new MemoryStream();
            var errors = _licenseValidationErrorFaker.GenerateForever().Take(errorCount);
            await _uut.Write(stream, errors);

            await Verify(stream.AsString());
        }

        [Test]
        public async Task ValidatedLicenses_Should_PrintCorrectTable(
            [Values(0, 1, 5, 20, 100)] int validatedLicenseCount)
        {
            using var stream = new MemoryStream();
            var validated = _validatedLicenseFaker.GenerateForever().Take(validatedLicenseCount);
            await _uut.Write(stream, validated);

            await Verify(stream.AsString());
        }

        [Test]
        public async Task ValidatedLicenses_Should_PrintCorrectTableWithUrls(
            [Values(0, 1, 5, 20, 100)] int validatedLicenseCount)
        {
            _uut = new TableOutputFormatter(true);

            using var stream = new MemoryStream();
            var validated = _validatedLicenseFaker.GenerateForever().Take(validatedLicenseCount);
            await _uut.Write(stream, validated);

            await Verify(stream.AsString());
        }
    }
}
