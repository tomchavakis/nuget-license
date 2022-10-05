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
using NuGetUtility.Test.Helper.NuGet.Version;
using NuGetUtility.Test.Helper.NUnitExtension;
using NuGetUtility.Test.Helper.ShuffelledEnumerable;
using NuGetUtility.Wrapper.HttpClientWrapper;

namespace NuGetUtility.Test.LicenseValidator
{
    [TestFixture]
    internal class LicenseValidatorTest
    {
        [SetUp]
        public void SetUp()
        {
            var fixture = new Fixture();
            _fileDownloader = new Mock<IFileDownloader>();
            _licenseMapping = fixture.Create<Dictionary<Uri, string>>();
            _allowedLicenses = fixture.CreateMany<string>();
            _context = fixture.Create<string>();

            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                _allowedLicenses,
                _fileDownloader.Object);
        }

        private NuGetUtility.LicenseValidator.LicenseValidator _uut = null!;
        private Dictionary<Uri, string> _licenseMapping = null!;
        private IEnumerable<string> _allowedLicenses = null!;
        private string _context = null!;
        private Mock<IFileDownloader> _fileDownloader = null!;

        [Test]
        public async Task ValidatingEmptyList_Should_ReturnEmptyErrorArray()
        {
            var emptyListToValidate = Enumerable.Empty<IPackageSearchMetadata>().AsAsyncEnumerable();
            await _uut.Validate(emptyListToValidate, _context);
            CollectionAssert.AreEqual(Enumerable.Empty<LicenseValidationError>(), _uut.GetErrors());
        }

        [Test]
        public async Task ValidatingEmptyList_Should_ReturnEmptyValidatedLicenses()
        {
            var emptyListToValidate = Enumerable.Empty<IPackageSearchMetadata>().AsAsyncEnumerable();
            await _uut.Validate(emptyListToValidate, _context);
            CollectionAssert.AreEqual(Enumerable.Empty<ValidatedLicense>(), _uut.GetValidatedLicenses());
        }

        private static Mock<IPackageSearchMetadata> SetupPackage(string packageId, NuGetVersion packageVersion)
        {
            var packageInfo = new Mock<IPackageSearchMetadata>();
            packageInfo.SetupGet(m => m.Identity).Returns(new PackageIdentity(packageId, packageVersion));
            return packageInfo;
        }

        private static Mock<IPackageSearchMetadata> SetupPackageWithLicenseInformationOfType(string packageId,
            NuGetVersion packageVersion,
            string license,
            LicenseType type)
        {
            var packageInfo = SetupPackage(packageId, packageVersion);
            packageInfo.SetupGet(m => m.LicenseMetadata)
                .Returns(new LicenseMetadata(type,
                    license,
                    NuGetLicenseExpression.Parse(license),
                    new string[] { },
                    LicenseMetadata.EmptyVersion));
            return packageInfo;
        }

        private static Mock<IPackageSearchMetadata> SetupPackageWithProperLicenseInformation(string packageId,
            NuGetVersion packageVersion,
            string license)
        {
            return SetupPackageWithLicenseInformationOfType(packageId, packageVersion, license, LicenseType.Expression);
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task
            ValidatingLicensesWithProperLicenseInformation_Should_NotContainErrorsIfAllowedLicensesIsEmpty(
                string packageId,
                NuGetVersion packageVersion,
                string license)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                new string[] { },
                _fileDownloader.Object);

            var package = SetupPackageWithProperLicenseInformation(packageId, packageVersion, license);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context);

            CollectionAssert.AreEqual(Enumerable.Empty<LicenseValidationError>(), _uut.GetErrors());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithProperLicenseInformation_Should_GiveCorrectValidatedLicenseList(
            string packageId,
            NuGetVersion packageVersion,
            string license)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                new string[] { },
                _fileDownloader.Object);

            var package = SetupPackageWithProperLicenseInformation(packageId, packageVersion, license);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context);

            CollectionAssert.AreEquivalent(new[]
                    { new ValidatedLicense(packageId, new MockedNugetVersion(packageVersion), license, LicenseInformationOrigin.Expression) },
                _uut.GetValidatedLicenses());
        }

        private static Mock<IPackageSearchMetadata> SetupPackageWithLicenseUrl(string packageId,
            NuGetVersion packageVersion,
            Uri url)
        {
            var packageInfo = SetupPackage(packageId, packageVersion);
            packageInfo.SetupGet(m => m.LicenseUrl).Returns(url);
            return packageInfo;
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithMatchingLicenseUrl_Should_NotContainErrorsIfAllowedLicensesIsEmpty(
            string packageId,
            NuGetVersion packageVersion)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                new string[] { },
                _fileDownloader.Object);

            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, _licenseMapping.Shuffle().First().Key);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context);

            CollectionAssert.AreEqual(Enumerable.Empty<LicenseValidationError>(), _uut.GetErrors());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithMatchingLicenseUrl_Should_GiveCorrectValidatedLicenseList(
            string packageId,
            NuGetVersion packageVersion)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                new string[] { },
                _fileDownloader.Object);

            var mappingLicense = _licenseMapping.Shuffle().First();
            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, mappingLicense.Key);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context);

            CollectionAssert.AreEquivalent(
                new[]
                {
                    new ValidatedLicense(packageId, new MockedNugetVersion(packageVersion), mappingLicense.Value, LicenseInformationOrigin.Url)
                },
                _uut.GetValidatedLicenses());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithNonMatchingLicenseUrl_Should_NotContainErrorsIfAllowedLicensesIsEmpty(
            string packageId,
            NuGetVersion packageVersion,
            Uri licenseUrl)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                new string[] { },
                _fileDownloader.Object);

            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, licenseUrl);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context);

            CollectionAssert.AreEqual(Enumerable.Empty<LicenseValidationError>(), _uut.GetErrors());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithMatchingLicenseUrl_Should_GiveCorrectValidatedLicenseList(
            string packageId,
            NuGetVersion packageVersion,
            Uri licenseUrl)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                new string[] { },
                _fileDownloader.Object);

            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, licenseUrl);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context);

            CollectionAssert.AreEquivalent(
                new[]
                {
                    new ValidatedLicense(packageId,
                        new MockedNugetVersion(packageVersion),
                        new string(licenseUrl.ToString()),
                        LicenseInformationOrigin.Url)
                },
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
            var license = fixture.Create<string>();

            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                new string[] { },
                _fileDownloader.Object);

            var package = SetupPackageWithLicenseInformationOfType(packageId, packageVersion, license, licenseType);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context);

            CollectionAssert.AreEqual(Enumerable.Empty<ValidatedLicense>(), _uut.GetValidatedLicenses());
            CollectionAssert.AreEquivalent(
                new[]
                {
                    new LicenseValidationError(_context,
                        packageId,
                        new MockedNugetVersion(packageVersion),
                        $"Validation for licenses of type {licenseType} not yet supported")
                },
                _uut.GetErrors());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithoutLicenseInformation_Should_GiveCorrectErrorsAndValidationList(
            string packageId,
            NuGetVersion packageVersion)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                new string[] { },
                _fileDownloader.Object);

            var package = SetupPackage(packageId, packageVersion);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context);

            CollectionAssert.AreEqual(Enumerable.Empty<ValidatedLicense>(), _uut.GetValidatedLicenses());
            CollectionAssert.AreEquivalent(
                new[]
                {
                    new LicenseValidationError(_context, packageId, new MockedNugetVersion(packageVersion), "No license information found")
                },
                _uut.GetErrors());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithProperLicenseInformation_Should_LeadToCorrectErrorsIfNotAllowed(
            string packageId,
            NuGetVersion packageVersion,
            string license)
        {
            var package = SetupPackageWithProperLicenseInformation(packageId, packageVersion, license);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context);

            CollectionAssert.AreEqual(
                new[]
                {
                    new LicenseValidationError(_context,
                        packageId,
                        new MockedNugetVersion(packageVersion),
                        $"License {license} not found in list of supported licenses")
                },
                _uut.GetErrors());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithProperLicenseInformation_Should_LeadToEmptyValidArrayIfNotAllowed(
            string packageId,
            NuGetVersion packageVersion,
            string license)
        {
            var package = SetupPackageWithProperLicenseInformation(packageId, packageVersion, license);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context);

            CollectionAssert.AreEqual(Enumerable.Empty<ValidatedLicense>(), _uut.GetValidatedLicenses());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithProperLicenseInformation_Should_LeadToEmptyErrorArrayIfAllowed(
            string packageId,
            NuGetVersion packageVersion)
        {
            var validLicense = _allowedLicenses.Shuffle().First();
            var package = SetupPackageWithProperLicenseInformation(packageId, packageVersion, validLicense);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context);

            CollectionAssert.AreEqual(Enumerable.Empty<LicenseValidationError>(), _uut.GetErrors());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithProperLicenseInformation_Should_LeadToCorrectValidationArrayIfAllowed(
            string packageId,
            NuGetVersion packageVersion)
        {
            var validLicense = _allowedLicenses.Shuffle().First();
            var package = SetupPackageWithProperLicenseInformation(packageId, packageVersion, validLicense);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context);

            CollectionAssert.AreEqual(new[]
                {
                    new ValidatedLicense(packageId, new MockedNugetVersion(packageVersion), validLicense, LicenseInformationOrigin.Expression)
                },
                _uut.GetValidatedLicenses());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithMatchingUrlInformation_Should_LeadToCorrectErrorsIfNotAllowed(
            string packageId,
            NuGetVersion packageVersion)
        {
            var urlMatch = _licenseMapping.Shuffle().First();
            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, urlMatch.Key);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context);

            CollectionAssert.AreEqual(
                new[]
                {
                    new LicenseValidationError(_context,
                        packageId,
                        new MockedNugetVersion(packageVersion),
                        $"License {urlMatch.Value} not found in list of supported licenses")
                },
                _uut.GetErrors());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithUrlInformation_Should_StartDownloadingSaidLicense(
            string packageId,
            NuGetVersion packageVersion)
        {
            var urlMatch = _licenseMapping.Shuffle().First();
            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, urlMatch.Key);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context);

            _fileDownloader.Verify(m => m.DownloadFile(package.Object.LicenseUrl,
                    $"{package.Object.Identity.Id}__{package.Object.Identity.Version}.html"),
                Times.Once);
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public void ValidatingLicensesWithUrlInformation_Should_ThrowLicenseDownloadInformation_If_DownloadThrows(
            string packageId,
            NuGetVersion packageVersion)
        {
            var urlMatch = _licenseMapping.Shuffle().First();
            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, urlMatch.Key);
            _fileDownloader.Setup(m => m.DownloadFile(package.Object.LicenseUrl, It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            var exception = Assert.ThrowsAsync<LicenseDownloadException>(async () => await _uut.Validate(
                new[] { package.Object }.AsAsyncEnumerable(),
                _context));
            Assert.IsInstanceOf<Exception>(exception!.InnerException);
            Assert.AreEqual(
                $"Failed to download license for package {packageId} ({packageVersion}).\nContext: {_context}",
                exception.Message);
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithMatchingUrlInformation_Should_LeadToEmptyValidArrayIfNotAllowed(
            string packageId,
            NuGetVersion packageVersion)
        {
            var urlMatch = _licenseMapping.Shuffle().First();
            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, urlMatch.Key);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context);

            CollectionAssert.AreEqual(Enumerable.Empty<ValidatedLicense>(), _uut.GetValidatedLicenses());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithMatchingUrlInformation_Should_LeadToEmptyErrorArrayIfAllowed(
            string packageId,
            NuGetVersion packageVersion)
        {
            var urlMatch = _licenseMapping.Shuffle().First();
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                _allowedLicenses.Append(urlMatch.Value),
                _fileDownloader.Object);
            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, urlMatch.Key);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context);

            CollectionAssert.AreEqual(Enumerable.Empty<LicenseValidationError>(), _uut.GetErrors());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithMatchingUrlInformation_Should_LeadToCorrectValidationArrayIfAllowed(
            string packageId,
            NuGetVersion packageVersion)
        {
            var urlMatch = _licenseMapping.Shuffle().First();
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                _allowedLicenses.Append(urlMatch.Value),
                _fileDownloader.Object);
            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, urlMatch.Key);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context);

            CollectionAssert.AreEqual(new[]
                    { new ValidatedLicense(packageId, new MockedNugetVersion(packageVersion), urlMatch.Value, LicenseInformationOrigin.Url) },
                _uut.GetValidatedLicenses());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithNotMatchingUrlInformation_Should_LeadToCorrectErrorsIfNotAllowed(
            string packageId,
            NuGetVersion packageVersion,
            Uri licenseUrl)
        {
            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, licenseUrl);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context);

            CollectionAssert.AreEqual(
                new[]
                {
                    new LicenseValidationError(_context,
                        packageId,
                        new MockedNugetVersion(packageVersion),
                        $"Cannot determine License type for url {licenseUrl}")
                },
                _uut.GetErrors());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithNotMatchingUrlInformation_Should_LeadToEmptyValidArrayIfNotAllowed(
            string packageId,
            NuGetVersion packageVersion,
            Uri licenseUrl)
        {
            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, licenseUrl);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context);

            CollectionAssert.AreEqual(Enumerable.Empty<ValidatedLicense>(), _uut.GetValidatedLicenses());
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task AppendIgnoredLicenses_Should_AppendIgnoredLicensesToValidatedLicenses(
            string validatedPackageId,
            NuGetVersion validatedPackageVersion,
            string ignoredPackageId,
            string ignoredPackageVersion)
        {
            var validatedLicense = _allowedLicenses.Shuffle().First();
            var package = SetupPackageWithProperLicenseInformation(validatedPackageId,
                validatedPackageVersion,
                validatedLicense);

            await _uut.Validate(new[] { package.Object }.AsAsyncEnumerable(), _context);

            var ignoredPackages = new[]
            {
                new Wrapper.NuGetWrapper.Packaging.Core.PackageIdentity(ignoredPackageId,
                    new MockedNugetVersion(ignoredPackageVersion))
            };
            _uut.AppendIgnoredPackages(ignoredPackages);

            CollectionAssert.AreEquivalent(new[]
                {
                    new ValidatedLicense(validatedPackageId,
                        new MockedNugetVersion(validatedPackageVersion),
                        validatedLicense,
                        LicenseInformationOrigin.Expression),
                    new ValidatedLicense(ignoredPackageId, new MockedNugetVersion(ignoredPackageVersion), null, LicenseInformationOrigin.Ignored)
                },
                _uut.GetValidatedLicenses());
        }
    }
}
