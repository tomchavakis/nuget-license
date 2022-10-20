using AutoFixture;
using Moq;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Licenses;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGetUtility.LicenseValidator;
using NuGetUtility.PackageInformationReader;
using NuGetUtility.Test.Helper.AsyncEnumerableExtension;
using NuGetUtility.Test.Helper.AutoFixture.NuGet.Versioning;
using NuGetUtility.Test.Helper.NUnitExtension;
using NuGetUtility.Test.Helper.ShuffelledEnumerable;
using NuGetUtility.Wrapper.HttpClientWrapper;
using System.Collections;

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
            _projectUrl = fixture.Create<Uri>();

            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                _allowedLicenses,
                _fileDownloader.Object);
        }

        private NuGetUtility.LicenseValidator.LicenseValidator _uut = null!;
        private Dictionary<Uri, string> _licenseMapping = null!;
        private IEnumerable<string> _allowedLicenses = null!;
        private string _context = null!;
        private Mock<IFileDownloader> _fileDownloader = null!;
        private Uri _projectUrl = null!;

        [Test]
        public async Task ValidatingEmptyList_Should_ReturnEmptyValidatedLicenses()
        {
            var emptyListToValidate = Enumerable.Empty<ReferencedPackageWithContext>().AsAsyncEnumerable();
            var results = await _uut.Validate(emptyListToValidate);
            CollectionAssert.AreEqual(Enumerable.Empty<LicenseValidationResult>(), results);
        }

        private Mock<IPackageSearchMetadata> SetupPackage(string packageId, NuGetVersion packageVersion)
        {
            var packageInfo = new Mock<IPackageSearchMetadata>();
            packageInfo.SetupGet(m => m.Identity).Returns(new PackageIdentity(packageId, packageVersion));
            packageInfo.SetupGet(m => m.ProjectUrl).Returns(_projectUrl);
            return packageInfo;
        }

        private Mock<IPackageSearchMetadata> SetupPackageWithLicenseInformationOfType(string packageId,
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

        private Mock<IPackageSearchMetadata> SetupPackageWithProperLicenseInformation(string packageId,
            NuGetVersion packageVersion,
            string license)
        {
            return SetupPackageWithLicenseInformationOfType(packageId, packageVersion, license, LicenseType.Expression);
        }

        private IAsyncEnumerable<ReferencedPackageWithContext> CreateInput(Mock<IPackageSearchMetadata> metadata,
            string context)
        {
            return new[] { new ReferencedPackageWithContext(context, metadata.Object) }.AsAsyncEnumerable();
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

            var result = await _uut.Validate(CreateInput(package, _context));

            Assert.That(result, Is.EquivalentTo(new[]
            {
                new LicenseValidationResult(packageId,
                    packageVersion,
                    _projectUrl.ToString(),
                    license,
                    LicenseInformationOrigin.Expression)
            }).Using(new LicenseValidationResultValueEqualityComparer()));
        }

        private Mock<IPackageSearchMetadata> SetupPackageWithLicenseUrl(string packageId,
            NuGetVersion packageVersion,
            Uri url)
        {
            var packageInfo = SetupPackage(packageId, packageVersion);
            packageInfo.SetupGet(m => m.LicenseUrl).Returns(url);
            return packageInfo;
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

            var mappingLicense = _licenseMapping.Shuffle(34561).First();
            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, mappingLicense.Key);

            var result = await _uut.Validate(CreateInput(package, _context));

            Assert.That(result,
                Is.EquivalentTo(new[]
                    {
                        new LicenseValidationResult(packageId,
                            packageVersion,
                            _projectUrl.ToString(),
                            mappingLicense.Value,
                            LicenseInformationOrigin.Url)
                    })
                    .Using(new LicenseValidationResultValueEqualityComparer()));
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

            var result = await _uut.Validate(CreateInput(package, _context));

            Assert.That(result,
                Is.EquivalentTo(new[]
                    {                    new LicenseValidationResult(packageId,
                                             packageVersion,
                                             _projectUrl.ToString(),
                                             licenseUrl.ToString(),
                                             LicenseInformationOrigin.Url)
                    })
                    .Using(new LicenseValidationResultValueEqualityComparer()));
        }

        [Test]
        public async Task ValidatingLicensesWithNotSupportedLicenseMetadata_Should_GiveCorrectResult(
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

            var result = await _uut.Validate(CreateInput(package, _context));

            Assert.That(result,
                Is.EquivalentTo(new[]
                    {                    new LicenseValidationResult(packageId,
                                             packageVersion,
                                             _projectUrl.ToString(),
                                             null,
                                             LicenseInformationOrigin.Unknown,
                                             new List<ValidationError>
                                             {
                                                 new ValidationError($"Validation for licenses of type {licenseType} not yet supported",
                                                     _context)
                                             })
                    })
                    .Using(new LicenseValidationResultValueEqualityComparer()));
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithoutLicenseInformation_Should_GiveCorrectResult(
            string packageId,
            NuGetVersion packageVersion)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                new string[] { },
                _fileDownloader.Object);

            var package = SetupPackage(packageId, packageVersion);

            var result = await _uut.Validate(CreateInput(package, _context));

            Assert.That(result,
                Is.EquivalentTo(new[]
                    {
                    new LicenseValidationResult(packageId,
                        packageVersion,
                        _projectUrl.ToString(),
                        null,
                        LicenseInformationOrigin.Unknown,
                        new List<ValidationError>
                        {
                            new ValidationError("No license information found",
                                _context)
                        })
                    })
                    .Using(new LicenseValidationResultValueEqualityComparer()));
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithProperLicenseInformation_Should_GiveCorrectResult_If_NotAllowed(
            string packageId,
            NuGetVersion packageVersion,
            string license)
        {
            var package = SetupPackageWithProperLicenseInformation(packageId, packageVersion, license);

            var result = await _uut.Validate(CreateInput(package, _context));

            Assert.That(result,
                Is.EquivalentTo(new[]
                    {
                    new LicenseValidationResult(packageId,
                        packageVersion,
                        _projectUrl.ToString(),
                        license,
                        LicenseInformationOrigin.Expression,
                        new List<ValidationError>
                        {
                            new ValidationError($"License {license} not found in list of supported licenses",
                                _context)
                        })
                    })
                    .Using(new LicenseValidationResultValueEqualityComparer()));
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithProperLicenseInformation_Should_GiveCorrectResult_If_Allowed(
            string packageId,
            NuGetVersion packageVersion)
        {
            var validLicense = _allowedLicenses.Shuffle(135643).First();
            var package = SetupPackageWithProperLicenseInformation(packageId, packageVersion, validLicense);

            var result = await _uut.Validate(CreateInput(package, _context));

            Assert.That(result,
                Is.EquivalentTo(new[]
                    {
                    new LicenseValidationResult(packageId,
                        packageVersion,
                        _projectUrl.ToString(),
                        validLicense,
                        LicenseInformationOrigin.Expression)
                    })
                    .Using(new LicenseValidationResultValueEqualityComparer()));
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithMatchingUrlInformation_Should_GiveCorrectResult_If_NotAllowed(
            string packageId,
            NuGetVersion packageVersion)
        {
            var urlMatch = _licenseMapping.Shuffle(765).First();
            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, urlMatch.Key);

            var result = await _uut.Validate(CreateInput(package, _context));

            Assert.That(result,
                Is.EquivalentTo(new[]
                    {
                    new LicenseValidationResult(packageId,
                        packageVersion,
                        _projectUrl.ToString(),
                        urlMatch.Value,
                        LicenseInformationOrigin.Url,
                        new List<ValidationError>
                        {
                            new ValidationError($"License {urlMatch.Value} not found in list of supported licenses",
                                _context)
                        })
                    })
                    .Using(new LicenseValidationResultValueEqualityComparer()));
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithUrlInformation_Should_StartDownloadingSaidLicense(
            string packageId,
            NuGetVersion packageVersion)
        {
            var urlMatch = _licenseMapping.Shuffle(4567).First();
            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, urlMatch.Key);

            _ = await _uut.Validate(CreateInput(package, _context));

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
            var urlMatch = _licenseMapping.Shuffle(12345).First();
            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, urlMatch.Key);
            _fileDownloader.Setup(m => m.DownloadFile(package.Object.LicenseUrl, It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            var exception = Assert.ThrowsAsync<LicenseDownloadException>(() => _uut.Validate(CreateInput(package, _context)));
            Assert.IsInstanceOf<Exception>(exception!.InnerException);
            Assert.AreEqual(
                $"Failed to download license for package {packageId} ({packageVersion}).\nContext: {_context}",
                exception.Message);
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithMatchingUrlInformation_Should_GiveCorrectResult_If_Allowed(
            string packageId,
            NuGetVersion packageVersion)
        {
            var urlMatch = _licenseMapping.Shuffle(43562).First();
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                _allowedLicenses.Append(urlMatch.Value),
                _fileDownloader.Object);
            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, urlMatch.Key);

            var result = await _uut.Validate(CreateInput(package, _context));

            Assert.That(result,
                Is.EquivalentTo(new[]
                    {
                    new LicenseValidationResult(packageId,
                        packageVersion,
                        _projectUrl.ToString(),
                        urlMatch.Value,
                        LicenseInformationOrigin.Url)
                    })
                    .Using(new LicenseValidationResultValueEqualityComparer()));
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithNotMatchingUrlInformation_Should_GiveCorrectResult_If_NotAllowed(
            string packageId,
            NuGetVersion packageVersion,
            Uri licenseUrl)
        {
            var package = SetupPackageWithLicenseUrl(packageId, packageVersion, licenseUrl);

            var result = await _uut.Validate(CreateInput(package, _context));

            Assert.That(result,
                Is.EquivalentTo(new[]
                    {
                    new LicenseValidationResult(packageId,
                        packageVersion,
                        _projectUrl.ToString(),
                        licenseUrl.ToString(),
                        LicenseInformationOrigin.Url,
                        new List<ValidationError>
                        {
                            new ValidationError($"Cannot determine License type for url {licenseUrl}",
                                _context)
                        })
                    })
                    .Using(new LicenseValidationResultValueEqualityComparer()));
        }
    }

}
