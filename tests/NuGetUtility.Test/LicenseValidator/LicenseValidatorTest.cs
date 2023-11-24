using AutoFixture;
using NSubstitute;
using NuGetUtility.LicenseValidator;
using NuGetUtility.PackageInformationReader;
using NuGetUtility.Test.Helper.AsyncEnumerableExtension;
using NuGetUtility.Test.Helper.AutoFixture.NuGet.Versioning;
using NuGetUtility.Test.Helper.NUnitExtension;
using NuGetUtility.Test.Helper.ShuffelledEnumerable;
using NuGetUtility.Wrapper.HttpClientWrapper;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;
using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

namespace NuGetUtility.Test.LicenseValidator
{
    [TestFixture]
    internal class LicenseValidatorTest
    {
        [SetUp]
        public void SetUp()
        {
            var fixture = new Fixture();
            _fileDownloader = Substitute.For<IFileDownloader>();
            _licenseMapping = fixture.Create<Dictionary<Uri, string>>();
            _allowedLicenses = fixture.CreateMany<string>();
            _context = fixture.Create<string>();
            _projectUrl = fixture.Create<Uri>();
            _ignoredLicenses = fixture.Create<string[]>();

            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                _allowedLicenses,
                _fileDownloader,
                _ignoredLicenses);
        }

        private NuGetUtility.LicenseValidator.LicenseValidator _uut = null!;
        private Dictionary<Uri, string> _licenseMapping = null!;
        private IEnumerable<string> _allowedLicenses = null!;
        private string _context = null!;
        private IFileDownloader _fileDownloader = null!;
        private Uri _projectUrl = null!;
        private string[] _ignoredLicenses = null!;

        [Test]
        public async Task ValidatingEmptyList_Should_ReturnEmptyValidatedLicenses()
        {
            IAsyncEnumerable<ReferencedPackageWithContext> emptyListToValidate = Enumerable.Empty<ReferencedPackageWithContext>().AsAsyncEnumerable();
            IEnumerable<LicenseValidationResult> results = await _uut.Validate(emptyListToValidate);
            CollectionAssert.AreEqual(Enumerable.Empty<LicenseValidationResult>(), results);
        }

        private IPackageMetadata SetupPackage(string packageId, INuGetVersion packageVersion)
        {
            IPackageMetadata packageInfo = Substitute.For<IPackageMetadata>();
            packageInfo.Identity.Returns(new PackageIdentity(packageId, packageVersion));
            packageInfo.ProjectUrl.Returns(_projectUrl.ToString());
            return packageInfo;
        }

        private IPackageMetadata SetupPackageWithLicenseInformationOfType(string packageId,
            INuGetVersion packageVersion,
            string license,
            LicenseType type)
        {
            IPackageMetadata packageInfo = SetupPackage(packageId, packageVersion);
            packageInfo.LicenseMetadata.Returns(new LicenseMetadata(type, license));
            return packageInfo;
        }

        private IPackageMetadata SetupPackageWithExpressionLicenseInformation(string packageId,
            INuGetVersion packageVersion,
            string license)
        {
            return SetupPackageWithLicenseInformationOfType(packageId, packageVersion, license, LicenseType.Expression);
        }

        private IPackageMetadata SetupPackageWithOverwriteLicenseInformation(string packageId,
            INuGetVersion packageVersion,
            string license)
        {
            return SetupPackageWithLicenseInformationOfType(packageId, packageVersion, license, LicenseType.Overwrite);
        }

        private static IAsyncEnumerable<ReferencedPackageWithContext> CreateInput(IPackageMetadata metadata,
            string context)
        {
            return new[] { new ReferencedPackageWithContext(context, metadata) }.AsAsyncEnumerable();
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicenses_Should_IgnorePackage_If_PackageNameMatchesExactly(
            string packageId,
            INuGetVersion packageVersion)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                Array.Empty<string>(),
                _fileDownloader,
                _ignoredLicenses.Append(packageId).ToArray());

            IPackageMetadata package = SetupPackage(packageId, packageVersion);

            IEnumerable<LicenseValidationResult> result = await _uut.Validate(LicenseValidatorTest.CreateInput(package, _context));

            Assert.That(result,
                Is.EquivalentTo(new[]
                    {
                        new LicenseValidationResult(packageId,
                            packageVersion,
                            _projectUrl.ToString(),
                            null,
                            LicenseInformationOrigin.Ignored)
                    })
                    .Using(new LicenseValidationResultValueEqualityComparer()));
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicenses_Should_NotIgnorePackage_If_PackageNameDoesNotMatchExactly(
            string packageId,
            INuGetVersion packageVersion,
            string license)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                Array.Empty<string>(),
                _fileDownloader,
                _ignoredLicenses.Append(packageId.Substring(1)).ToArray());

            IPackageMetadata package = SetupPackageWithExpressionLicenseInformation(packageId, packageVersion, license);

            IEnumerable<LicenseValidationResult> result = await _uut.Validate(LicenseValidatorTest.CreateInput(package, _context));

            Assert.That(result,
                Is.EquivalentTo(new[]
                    {
                        new LicenseValidationResult(packageId,
                            packageVersion,
                            _projectUrl.ToString(),
                            license,
                            LicenseInformationOrigin.Expression)
                    })
                    .Using(new LicenseValidationResultValueEqualityComparer()));
        }

        [Test]
        [ExtendedInlineAutoData(typeof(NuGetVersionBuilder), 1)]
        [ExtendedInlineAutoData(typeof(NuGetVersionBuilder), 5)]
        [ExtendedInlineAutoData(typeof(NuGetVersionBuilder), int.MaxValue)]
        public async Task ValidatingLicenses_Should_IgnorePackage_If_IgnoreWildcardMatches_If_WildcardMatchesStart(
            int matchedCharacters,
            string packageId,
            INuGetVersion packageVersion)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                Array.Empty<string>(),
                _fileDownloader,
                _ignoredLicenses.Append($"*{packageId.Substring(Math.Min(matchedCharacters, packageId.Length))}").ToArray());

            IPackageMetadata package = SetupPackage(packageId, packageVersion);

            IEnumerable<LicenseValidationResult> result = await _uut.Validate(LicenseValidatorTest.CreateInput(package, _context));

            Assert.That(result,
                Is.EquivalentTo(new[]
                    {
                        new LicenseValidationResult(packageId,
                            packageVersion,
                            _projectUrl.ToString(),
                            null,
                            LicenseInformationOrigin.Ignored)
                    })
                    .Using(new LicenseValidationResultValueEqualityComparer()));
        }

        [Test]
        [ExtendedInlineAutoData(typeof(NuGetVersionBuilder), 0)]
        [ExtendedInlineAutoData(typeof(NuGetVersionBuilder), 1)]
        [ExtendedInlineAutoData(typeof(NuGetVersionBuilder), 5)]
        public async Task ValidatingLicenses_Should_IgnorePackage_If_IgnoreWildcardMatches_If_WildcardMatchesEnd(
            int remainingCharacters,
            string packageId,
            INuGetVersion packageVersion)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                Array.Empty<string>(),
                _fileDownloader,
                _ignoredLicenses.Append($"{packageId.Substring(0, remainingCharacters)}*").ToArray());

            IPackageMetadata package = SetupPackage(packageId, packageVersion);

            IEnumerable<LicenseValidationResult> result = await _uut.Validate(LicenseValidatorTest.CreateInput(package, _context));

            Assert.That(result,
                Is.EquivalentTo(new[]
                    {
                        new LicenseValidationResult(packageId,
                            packageVersion,
                            _projectUrl.ToString(),
                            null,
                            LicenseInformationOrigin.Ignored)
                    })
                    .Using(new LicenseValidationResultValueEqualityComparer()));
        }

        [Test]
        [ExtendedInlineAutoData(typeof(NuGetVersionBuilder), 1, 2)]
        [ExtendedInlineAutoData(typeof(NuGetVersionBuilder), 1, 5)]
        [ExtendedInlineAutoData(typeof(NuGetVersionBuilder), 5, 10)]
        public async Task ValidatingLicenses_Should_IgnorePackage_If_IgnoreWildcardMatches_If_WildcardMatchesMiddle(
            int wildcardMatchStartIndex,
            int wildcardMatchEndIndex,
            string packageId,
            INuGetVersion packageVersion)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                Array.Empty<string>(),
                _fileDownloader,
                _ignoredLicenses.Append($"{packageId.Substring(0, wildcardMatchStartIndex)}*{packageId.Substring(wildcardMatchEndIndex)}").ToArray());

            IPackageMetadata package = SetupPackage(packageId, packageVersion);

            IEnumerable<LicenseValidationResult> result = await _uut.Validate(LicenseValidatorTest.CreateInput(package, _context));

            Assert.That(result,
                Is.EquivalentTo(new[]
                    {
                        new LicenseValidationResult(packageId,
                            packageVersion,
                            _projectUrl.ToString(),
                            null,
                            LicenseInformationOrigin.Ignored)
                    })
                    .Using(new LicenseValidationResultValueEqualityComparer()));
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicenses_Should_IgnorePackage_If_IgnoreWildcardMatches_If_MultipleWildcards(
            string packageId,
            INuGetVersion packageVersion)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                Array.Empty<string>(),
                _fileDownloader,
                _ignoredLicenses.Append($"*{packageId.Substring(2, 5)}*{packageId.Substring(10, 2)}*").ToArray());

            IPackageMetadata package = SetupPackage(packageId, packageVersion);

            IEnumerable<LicenseValidationResult> result = await _uut.Validate(LicenseValidatorTest.CreateInput(package, _context));

            Assert.That(result,
                Is.EquivalentTo(new[]
                    {
                        new LicenseValidationResult(packageId,
                            packageVersion,
                            _projectUrl.ToString(),
                            null,
                            LicenseInformationOrigin.Ignored)
                    })
                    .Using(new LicenseValidationResultValueEqualityComparer()));
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithExpressionLicenseInformation_Should_GiveCorrectValidatedLicenseList(
            string packageId,
            INuGetVersion packageVersion,
            string license)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                Array.Empty<string>(),
                _fileDownloader,
                _ignoredLicenses);

            IPackageMetadata package = SetupPackageWithExpressionLicenseInformation(packageId, packageVersion, license);

            IEnumerable<LicenseValidationResult> result = await _uut.Validate(LicenseValidatorTest.CreateInput(package, _context));

            Assert.That(result,
                Is.EquivalentTo(new[]
                    {
                        new LicenseValidationResult(packageId,
                            packageVersion,
                            _projectUrl.ToString(),
                            license,
                            LicenseInformationOrigin.Expression)
                    })
                    .Using(new LicenseValidationResultValueEqualityComparer()));
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithOverwriteLicenseInformation_Should_GiveCorrectValidatedLicenseList(
            string packageId,
            INuGetVersion packageVersion,
            string license)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                Array.Empty<string>(),
                _fileDownloader,
                _ignoredLicenses);

            IPackageMetadata package = SetupPackageWithOverwriteLicenseInformation(packageId, packageVersion, license);

            IEnumerable<LicenseValidationResult> result = await _uut.Validate(LicenseValidatorTest.CreateInput(package, _context));

            Assert.That(result,
                Is.EquivalentTo(new[]
                    {
                        new LicenseValidationResult(packageId,
                            packageVersion,
                            _projectUrl.ToString(),
                            license,
                            LicenseInformationOrigin.Overwrite)
                    })
                    .Using(new LicenseValidationResultValueEqualityComparer()));
        }

        private IPackageMetadata SetupPackageWithLicenseUrl(string packageId,
            INuGetVersion packageVersion,
            Uri url)
        {
            IPackageMetadata packageInfo = SetupPackage(packageId, packageVersion);
            packageInfo.LicenseUrl.Returns(url);
            return packageInfo;
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithMatchingLicenseUrl_Should_GiveCorrectValidatedLicenseList(
            string packageId,
            INuGetVersion packageVersion)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                Array.Empty<string>(),
                _fileDownloader,
                _ignoredLicenses);

            KeyValuePair<Uri, string> mappingLicense = _licenseMapping.Shuffle(34561).First();
            IPackageMetadata package = SetupPackageWithLicenseUrl(packageId, packageVersion, mappingLicense.Key);

            IEnumerable<LicenseValidationResult> result = await _uut.Validate(LicenseValidatorTest.CreateInput(package, _context));

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
            INuGetVersion packageVersion,
            Uri licenseUrl)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                Array.Empty<string>(),
                _fileDownloader,
                _ignoredLicenses);

            IPackageMetadata package = SetupPackageWithLicenseUrl(packageId, packageVersion, licenseUrl);

            IEnumerable<LicenseValidationResult> result = await _uut.Validate(LicenseValidatorTest.CreateInput(package, _context));

            Assert.That(result,
                Is.EquivalentTo(new[]
                    {
                        new LicenseValidationResult(packageId,
                            packageVersion,
                            _projectUrl.ToString(),
                            licenseUrl.ToString(),
                            LicenseInformationOrigin.Url)
                    })
                    .Using(new LicenseValidationResultValueEqualityComparer()));
        }

        [Test]
        public async Task ValidatingLicensesWithNotSupportedLicenseMetadata_Should_GiveCorrectResult(
            [EnumValuesExcept(LicenseType.Expression, LicenseType.Overwrite)] LicenseType licenseType)
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new NuGetVersionBuilder());
            string packageId = fixture.Create<string>();
            INuGetVersion packageVersion = fixture.Create<INuGetVersion>();
            string license = fixture.Create<string>();

            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                Array.Empty<string>(),
                _fileDownloader,
                _ignoredLicenses);

            IPackageMetadata package = SetupPackageWithLicenseInformationOfType(packageId, packageVersion, license, licenseType);

            IEnumerable<LicenseValidationResult> result = await _uut.Validate(LicenseValidatorTest.CreateInput(package, _context));

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
            INuGetVersion packageVersion)
        {
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                Array.Empty<string>(),
                _fileDownloader,
                _ignoredLicenses);

            IPackageMetadata package = SetupPackage(packageId, packageVersion);

            IEnumerable<LicenseValidationResult> result = await _uut.Validate(LicenseValidatorTest.CreateInput(package, _context));

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
        public async Task ValidatingLicensesWithExpressionLicenseInformation_Should_GiveCorrectResult_If_NotAllowed(
            string packageId,
            INuGetVersion packageVersion,
            string license)
        {
            IPackageMetadata package = SetupPackageWithExpressionLicenseInformation(packageId, packageVersion, license);

            IEnumerable<LicenseValidationResult> result = await _uut.Validate(LicenseValidatorTest.CreateInput(package, _context));

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
        public async Task ValidatingLicensesWithOverwriteLicenseInformation_Should_GiveCorrectResult_If_NotAllowed(
            string packageId,
            INuGetVersion packageVersion,
            string license)
        {
            IPackageMetadata package = SetupPackageWithOverwriteLicenseInformation(packageId, packageVersion, license);

            IEnumerable<LicenseValidationResult> result = await _uut.Validate(LicenseValidatorTest.CreateInput(package, _context));

            Assert.That(result,
                Is.EquivalentTo(new[]
                    {
                        new LicenseValidationResult(packageId,
                            packageVersion,
                            _projectUrl.ToString(),
                            license,
                            LicenseInformationOrigin.Overwrite,
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
        public async Task ValidatingLicensesWithExpressionLicenseInformation_Should_GiveCorrectResult_If_Allowed(
            string packageId,
            INuGetVersion packageVersion)
        {
            string validLicense = _allowedLicenses.Shuffle(135643).First();
            IPackageMetadata package = SetupPackageWithExpressionLicenseInformation(packageId, packageVersion, validLicense);

            IEnumerable<LicenseValidationResult> result = await _uut.Validate(LicenseValidatorTest.CreateInput(package, _context));

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
        public async Task ValidatingLicensesWithOverwriteLicenseInformation_Should_GiveCorrectResult_If_Allowed(
            string packageId,
            INuGetVersion packageVersion)
        {
            string validLicense = _allowedLicenses.Shuffle(135643).First();
            IPackageMetadata package = SetupPackageWithOverwriteLicenseInformation(packageId, packageVersion, validLicense);

            IEnumerable<LicenseValidationResult> result = await _uut.Validate(LicenseValidatorTest.CreateInput(package, _context));

            Assert.That(result,
                Is.EquivalentTo(new[]
                    {
                        new LicenseValidationResult(packageId,
                            packageVersion,
                            _projectUrl.ToString(),
                            validLicense,
                            LicenseInformationOrigin.Overwrite)
                    })
                    .Using(new LicenseValidationResultValueEqualityComparer()));
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithMatchingUrlInformation_Should_GiveCorrectResult_If_NotAllowed(
            string packageId,
            INuGetVersion packageVersion)
        {
            KeyValuePair<Uri, string> urlMatch = _licenseMapping.Shuffle(765).First();
            IPackageMetadata package = SetupPackageWithLicenseUrl(packageId, packageVersion, urlMatch.Key);

            IEnumerable<LicenseValidationResult> result = await _uut.Validate(LicenseValidatorTest.CreateInput(package, _context));

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
            INuGetVersion packageVersion)
        {
            KeyValuePair<Uri, string> urlMatch = _licenseMapping.Shuffle(4567).First();
            IPackageMetadata package = SetupPackageWithLicenseUrl(packageId, packageVersion, urlMatch.Key);

            _ = await _uut.Validate(LicenseValidatorTest.CreateInput(package, _context));

            await _fileDownloader.Received(1).DownloadFile(package.LicenseUrl!,
                    $"{package.Identity.Id}__{package.Identity.Version}.html");
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public void ValidatingLicensesWithUrlInformation_Should_ThrowLicenseDownloadInformation_If_DownloadThrows(
            string packageId,
            INuGetVersion packageVersion)
        {
            KeyValuePair<Uri, string> urlMatch = _licenseMapping.Shuffle(12345).First();
            IPackageMetadata package = SetupPackageWithLicenseUrl(packageId, packageVersion, urlMatch.Key);
            _fileDownloader.When(m => m.DownloadFile(package.LicenseUrl!, Arg.Any<string>()))
                .Do(_ => throw new Exception());

            LicenseDownloadException? exception =
                Assert.ThrowsAsync<LicenseDownloadException>(() => _uut.Validate(LicenseValidatorTest.CreateInput(package, _context)));
            Assert.IsInstanceOf<Exception>(exception!.InnerException);
            Assert.AreEqual(
                $"Failed to download license for package {packageId} ({packageVersion}).\nContext: {_context}",
                exception.Message);
        }

        [Test]
        [ExtendedAutoData(typeof(NuGetVersionBuilder))]
        public async Task ValidatingLicensesWithMatchingUrlInformation_Should_GiveCorrectResult_If_Allowed(
            string packageId,
            INuGetVersion packageVersion)
        {
            KeyValuePair<Uri, string> urlMatch = _licenseMapping.Shuffle(43562).First();
            _uut = new NuGetUtility.LicenseValidator.LicenseValidator(_licenseMapping,
                _allowedLicenses.Append(urlMatch.Value),
                _fileDownloader,
                _ignoredLicenses);
            IPackageMetadata package = SetupPackageWithLicenseUrl(packageId, packageVersion, urlMatch.Key);

            IEnumerable<LicenseValidationResult> result = await _uut.Validate(LicenseValidatorTest.CreateInput(package, _context));

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
            INuGetVersion packageVersion,
            Uri licenseUrl)
        {
            IPackageMetadata package = SetupPackageWithLicenseUrl(packageId, packageVersion, licenseUrl);

            IEnumerable<LicenseValidationResult> result = await _uut.Validate(LicenseValidatorTest.CreateInput(package, _context));

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
