using AutoFixture;
using Moq;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Licenses;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGetUtility.LicenseValidator;
using NuGetUtility.Test.Helper.AsyncEnumerableExtension;
using NuGetUtility.Test.Helper.AutoFixture.NuGet.Versioning;
using NuGetUtility.Test.Helper.NUnitExtension;
using NuGetUtility.Test.Helper.ShuffelledEnumerable;
using NUnit.Framework;

namespace NuGetUtility.Test.LicenseValidator
{
    [TestFixture]
    internal class LicenseValidatorTest
    {
        [SetUp]
        public void SetUp()
        {
            var fixture = new Fixture();
            _licenseMapping = fixture.Create<Dictionary<Uri, LicenseId>>();
            _allowedLicenses = fixture.CreateMany<LicenseId>();
            _context = fixture.Create<string>();

            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping, _allowedLicenses);
        }

        private NuGetUtility.LicenseValidator.LicenseValidator? _uut;
        private Dictionary<Uri, LicenseId>? _licenseMapping;
        private IEnumerable<LicenseId>? _allowedLicenses;
        private string? _context;

        [Test]
        public async Task ValidatingEmptyList_Should_ReturnEmptyErrorArray()
        {
            var emptyListToValidate = Enumerable.Empty<IPackageSearchMetadata>().AsAsyncEnumerable();
            await _uut!.Validate(emptyListToValidate, _context!);
            CollectionAssert.AreEqual(Enumerable.Empty<LicenseValidationError>(), _uut!.GetErrors());
        }

        [Test]
        public async Task ValidatingEmptyList_Should_ReturnEmptyValidatedLicenses()
        {
            var emptyListToValidate = Enumerable.Empty<IPackageSearchMetadata>().AsAsyncEnumerable();
            await _uut!.Validate(emptyListToValidate, _context!);
            CollectionAssert.AreEqual(Enumerable.Empty<ValidatedLicense>(), _uut!.GetValidatedLicenses());
        }

        private static Mock<IPackageSearchMetadata> SetupPackage(string packageId, NuGetVersion packageVersion)
        {
            var packageInfo = new Mock<IPackageSearchMetadata>();
            packageInfo.SetupGet(m => m.Identity).Returns(new PackageIdentity(packageId, packageVersion));
            return packageInfo;
        }

        private static Mock<IPackageSearchMetadata> SetupPackageWithLicenseInformationOfType(string packageId,
            NuGetVersion packageVersion, LicenseId license, LicenseType type)
        {
            var packageInfo = SetupPackage(packageId, packageVersion);
            packageInfo.SetupGet(m => m.LicenseMetadata).Returns(new LicenseMetadata(type, license.Id,
                NuGetLicenseExpression.Parse(license.Id), new string[] { }, license.Version));
            return packageInfo;
        }

        private static Mock<IPackageSearchMetadata> SetupPackageWithProperLicenseInformation(string packageId,
            NuGetVersion packageVersion, LicenseId license)
        {
            return SetupPackageWithLicenseInformationOfType(packageId, packageVersion, license, LicenseType.Expression);
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task
            ValidatingLicensesWithProperLicenseInformation_Should_NotContainErrorsIfAllowedLicensesIsEmpty(
                string packageId, NuGetVersion packageVersion, LicenseId license)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping!, new LicenseId[] { });

            var package = SetupPackageWithProperLicenseInformation(packageId, packageVersion, license);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context!);

            CollectionAssert.AreEqual(Enumerable.Empty<LicenseValidationError>(), _uut!.GetErrors());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithProperLicenseInformation_Should_GiveCorrectValidatedLicenseList(
            string packageId, NuGetVersion packageVersion, LicenseId license)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping!, new LicenseId[] { });

            var package = SetupPackageWithProperLicenseInformation(packageId, packageVersion, license);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context!);

            CollectionAssert.AreEquivalent(new[] { new ValidatedLicense(packageId, packageVersion, license) },
                _uut.GetValidatedLicenses());
        }

        private static Mock<IPackageSearchMetadata> SetupPackageWithLicenseUrl(string packageId,
            NuGetVersion packageVersion, Uri url)
        {
            var packageInfo = SetupPackage(packageId, packageVersion);
            packageInfo.SetupGet(m => m.LicenseUrl).Returns(url);
            return packageInfo;
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithMatchingLicenseUrl_Should_NotContainErrorsIfAllowedLicensesIsEmpty(
            string packageId, NuGetVersion packageVersion)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping!, new LicenseId[] { });

            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, _licenseMapping!.Shuffle().First().Key);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context!);

            CollectionAssert.AreEqual(Enumerable.Empty<LicenseValidationError>(), _uut!.GetErrors());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithMatchingLicenseUrl_Should_GiveCorrectValidatedLicenseList(
            string packageId, NuGetVersion packageVersion)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping!, new LicenseId[] { });

            var mappingLicense = _licenseMapping!.Shuffle().First();
            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, mappingLicense.Key);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context!);

            CollectionAssert.AreEquivalent(
                new[] { new ValidatedLicense(packageId, packageVersion, mappingLicense.Value) },
                _uut.GetValidatedLicenses());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithNonMatchingLicenseUrl_Should_NotContainErrorsIfAllowedLicensesIsEmpty(
            string packageId, NuGetVersion packageVersion, Uri licenseUrl)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping!, new LicenseId[] { });

            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, licenseUrl);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context!);

            CollectionAssert.AreEqual(Enumerable.Empty<LicenseValidationError>(), _uut!.GetErrors());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithMatchingLicenseUrl_Should_GiveCorrectValidatedLicenseList(
            string packageId, NuGetVersion packageVersion, Uri licenseUrl)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping!, new LicenseId[] { });

            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, licenseUrl);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context!);

            CollectionAssert.AreEquivalent(
                new[] { new ValidatedLicense(packageId, packageVersion, new LicenseId(licenseUrl.ToString())) },
                _uut.GetValidatedLicenses());
        }

        [Test]
        public async Task ValidatingLicensesWithNotSupportedLicenseMetadata_Should_GiveCorrectErrorsAndValidationList(
            [EnumValuesExcept(LicenseType.Expression)] LicenseType licenseType)
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new NuGetVersionBuilder());
            var packageId = fixture.Create<string>();
            var packageVersion = fixture.Create<NuGetVersion>();
            var license = fixture.Create<LicenseId>();

            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping!, new LicenseId[] { });

            var package = SetupPackageWithLicenseInformationOfType(packageId, packageVersion, license, licenseType);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context!);

            CollectionAssert.AreEqual(Enumerable.Empty<ValidatedLicense>(), _uut.GetValidatedLicenses());
            CollectionAssert.AreEquivalent(
                new[]
                {
                    new LicenseValidationError(_context!, packageId, packageVersion,
                        $"Validation for licenses of type {licenseType} not yet supported")
                }, _uut.GetErrors());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithoutLicenseInformation_Should_GiveCorrectErrorsAndValidationList(
            string packageId, NuGetVersion packageVersion)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping!, new LicenseId[] { });

            var package = SetupPackage(packageId, packageVersion);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context!);

            CollectionAssert.AreEqual(Enumerable.Empty<ValidatedLicense>(), _uut.GetValidatedLicenses());
            CollectionAssert.AreEquivalent(
                new[]
                {
                    new LicenseValidationError(_context!, packageId, packageVersion, "No license information found")
                }, _uut.GetErrors());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithProperLicenseInformation_Should_LeadToCorrectErrorsIfNotAllowed(
            string packageId, NuGetVersion packageVersion, LicenseId license)
        {
            var package = SetupPackageWithProperLicenseInformation(packageId, packageVersion, license);

            await _uut!.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context!);

            CollectionAssert.AreEqual(
                new[]
                {
                    new LicenseValidationError(_context!, packageId, packageVersion,
                        $"License type {license.Id}({license.Version}) not found in list of supported licenses")
                }, _uut!.GetErrors());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithProperLicenseInformation_Should_LeadToEmptyValidArrayIfNotAllowed(
            string packageId, NuGetVersion packageVersion, LicenseId license)
        {
            var package = SetupPackageWithProperLicenseInformation(packageId, packageVersion, license);

            await _uut!.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context!);

            CollectionAssert.AreEqual(Enumerable.Empty<ValidatedLicense>(), _uut!.GetValidatedLicenses());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithProperLicenseInformation_Should_LeadToEmptyErrorArrayIfAllowed(
            string packageId, NuGetVersion packageVersion)
        {
            var validLicense = _allowedLicenses!.Shuffle().First();
            var package = SetupPackageWithProperLicenseInformation(packageId, packageVersion, validLicense);

            await _uut!.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context!);

            CollectionAssert.AreEqual(Enumerable.Empty<LicenseValidationError>(), _uut!.GetErrors());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithProperLicenseInformation_Should_LeadToCorrectValidationArrayIfAllowed(
            string packageId, NuGetVersion packageVersion)
        {
            var validLicense = _allowedLicenses!.Shuffle().First();
            var package = SetupPackageWithProperLicenseInformation(packageId, packageVersion, validLicense);

            await _uut!.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context!);

            CollectionAssert.AreEqual(new[] { new ValidatedLicense(packageId, packageVersion, validLicense) },
                _uut!.GetValidatedLicenses());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithMatchingUrlInformation_Should_LeadToCorrectErrorsIfNotAllowed(
            string packageId, NuGetVersion packageVersion)
        {
            var urlMatch = _licenseMapping!.Shuffle().First();
            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, urlMatch.Key);

            await _uut!.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context!);

            CollectionAssert.AreEqual(
                new[]
                {
                    new LicenseValidationError(_context!, packageId, packageVersion,
                        $"License type {urlMatch.Value.Id}({urlMatch.Value.Version}) not found in list of supported licenses")
                }, _uut!.GetErrors());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithMatchingUrlInformation_Should_LeadToEmptyValidArrayIfNotAllowed(
            string packageId, NuGetVersion packageVersion)
        {
            var urlMatch = _licenseMapping!.Shuffle().First();
            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, urlMatch.Key);

            await _uut!.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context!);

            CollectionAssert.AreEqual(Enumerable.Empty<ValidatedLicense>(), _uut!.GetValidatedLicenses());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithMatchingUrlInformation_Should_LeadToEmptyErrorArrayIfAllowed(
            string packageId, NuGetVersion packageVersion)
        {
            var validLicense = _allowedLicenses!.Shuffle().First();
            var urlMatch = _licenseMapping!.Shuffle().First();
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping!,
                _allowedLicenses!.Append(urlMatch.Value));
            var package = SetupPackageWithProperLicenseInformation(packageId, packageVersion, validLicense);

            await _uut!.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context!);

            CollectionAssert.AreEqual(Enumerable.Empty<LicenseValidationError>(), _uut!.GetErrors());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithMatchingUrlInformation_Should_LeadToCorrectValidationArrayIfAllowed(
            string packageId, NuGetVersion packageVersion)
        {
            var validLicense = _allowedLicenses!.Shuffle().First();
            var urlMatch = _licenseMapping!.Shuffle().First();
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping!,
                _allowedLicenses!.Append(urlMatch.Value));
            var package = SetupPackageWithProperLicenseInformation(packageId, packageVersion, validLicense);

            await _uut!.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context!);

            CollectionAssert.AreEqual(new[] { new ValidatedLicense(packageId, packageVersion, validLicense) },
                _uut!.GetValidatedLicenses());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithNotMatchingUrlInformation_Should_LeadToCorrectErrorsIfNotAllowed(
            string packageId, NuGetVersion packageVersion, Uri licenseUrl)
        {
            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, licenseUrl);

            await _uut!.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context!);

            CollectionAssert.AreEqual(
                new[]
                {
                    new LicenseValidationError(_context!, packageId, packageVersion,
                        $"Cannot determine License type for url {licenseUrl}")
                }, _uut!.GetErrors());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithNotMatchingUrlInformation_Should_LeadToEmptyValidArrayIfNotAllowed(
            string packageId, NuGetVersion packageVersion, Uri licenseUrl)
        {
            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, licenseUrl);

            await _uut!.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context!);

            CollectionAssert.AreEqual(Enumerable.Empty<ValidatedLicense>(), _uut!.GetValidatedLicenses());
        }
    }
}
