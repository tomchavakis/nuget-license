// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using Bogus;
using NuGetUtility.LicenseValidator;
using NuGetUtility.Output;
using NuGetUtility.Test.Extensions;
using NuGetUtility.Test.Helper.ShuffelledEnumerable;
using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

namespace NuGetUtility.Test.Output
{
    public abstract class TestBase
    {
        private IOutputFormatter _uut = null!;
        protected Faker<LicenseValidationResult> LicenseValidationErrorFaker = null!;

        protected Faker<LicenseValidationResult> ValidatedLicenseFaker = null!;
        [SetUp]
        public void SetUp()
        {
            ValidatedLicenseFaker = new Faker<LicenseValidationResult>().CustomInstantiator(f =>
                    new LicenseValidationResult(f.Name.JobTitle(),
                        new NuGetVersion(f.System.Semver()),
                        f.Internet.Url(),
                        f.Hacker.Phrase(),
                        f.Internet.Url(),
                        f.Random.Enum<LicenseInformationOrigin>()))
                .UseSeed(8675309);
            LicenseValidationErrorFaker = new Faker<LicenseValidationResult>().CustomInstantiator(f =>
                    new LicenseValidationResult(f.Name.JobTitle(),
                        new NuGetVersion(f.System.Semver()),
                        GetNullableUrl(f),
                        GetNullableLicense(f),
                        GetNullableUrl(f),
                        f.Random.Enum<LicenseInformationOrigin>(),
                        GetErrorList(f).ToList()))
                .UseSeed(9078345);
            _uut = CreateUut();
        }
        protected abstract IOutputFormatter CreateUut();

        private string? GetNullableUrl(Faker faker)
        {
            if (faker.Random.Bool())
            {
                return null;
            }
            return faker.Internet.Url();
        }

        private string? GetNullableLicense(Faker faker)
        {
            if (faker.Random.Bool())
            {
                return null;
            }
            return faker.Hacker.Phrase();
        }

        private IEnumerable<ValidationError> GetErrorList(Faker faker)
        {
            int itemCount = faker.Random.Int(3, 10);
            for (int i = 0; i < itemCount; i++)
            {
                yield return new ValidationError(faker.Name.FirstName(), faker.Internet.Url());
            }
        }

        [Test]
        public async Task ValidatedLicensesWithErrors_Should_PrintCorrectTable(
            [Values(0, 1, 5, 20, 100)] int validCount,
            [Values(1, 3, 5, 20)] int errorCount)
        {
            using var stream = new MemoryStream();
            var result = LicenseValidationErrorFaker.GenerateForever()
                .Take(errorCount)
                .Concat(ValidatedLicenseFaker.GenerateForever().Take(validCount))
                .Shuffle(971234)
                .ToList();
            await _uut.Write(stream, result);

            await Verify(stream.AsString());
        }

        [Test]
        public async Task ValidatedLicenses_Should_PrintCorrectTable(
            [Values(0, 1, 5, 20, 100)] int validatedLicenseCount)
        {
            using var stream = new MemoryStream();
            var validated = ValidatedLicenseFaker.GenerateForever().Take(validatedLicenseCount).ToList();
            await _uut.Write(stream, validated);

            await Verify(stream.AsString());
        }

        private class NuGetVersion : INuGetVersion
        {
            private readonly string _version;

            public NuGetVersion(string version)
            {
                _version = version;
            }

            public int CompareTo(INuGetVersion? other) => throw new NotImplementedException();

            public override string ToString()
            {
                return _version;
            }
        }
    }
}
