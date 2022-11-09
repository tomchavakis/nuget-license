using NuGetUtility.Extensions;
using NuGetUtility.PackageInformationReader;
using NuGetUtility.Wrapper.HttpClientWrapper;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;
using NuGetUtility.Wrapper.NuGetWrapper.Versioning;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace NuGetUtility.LicenseValidator
{
    public class LicenseValidator
    {
        private readonly IEnumerable<string> _allowedLicenses;
        private readonly IFileDownloader _fileDownloader;
        private readonly string[] _ignoredPackages;
        private readonly Dictionary<Uri, string> _licenseMapping;

        public LicenseValidator(Dictionary<Uri, string> licenseMapping,
            IEnumerable<string> allowedLicenses,
            IFileDownloader fileDownloader,
            string[] ignoredPackages)
        {
            _licenseMapping = licenseMapping;
            _allowedLicenses = allowedLicenses;
            _fileDownloader = fileDownloader;
            _ignoredPackages = ignoredPackages;
        }

        public async Task<IEnumerable<LicenseValidationResult>> Validate(
            IAsyncEnumerable<ReferencedPackageWithContext> packages)
        {
            var result = new ConcurrentDictionary<LicenseNameAndVersion, LicenseValidationResult>();
            await foreach (ReferencedPackageWithContext info in packages)
            {
                if (IsIgnoredPackage(info.PackageInfo.Identity))
                {
                    AddOrUpdateLicense(result,
                        info.PackageInfo,
                        LicenseInformationOrigin.Ignored);
                }
                else if (info.PackageInfo.LicenseMetadata != null)
                {
                    ValidateLicenseByMetadata(info.PackageInfo, info.Context, result);
                }
                else if (info.PackageInfo.LicenseUrl != null)
                {
                    await ValidateLicenseByUrl(info.PackageInfo, info.Context, result);
                }
                else
                {
                    AddOrUpdateLicense(result,
                        info.PackageInfo,
                        LicenseInformationOrigin.Unknown,
                        new ValidationError("No license information found", info.Context));
                }
            }
            return result.Values;
        }

        private bool IsIgnoredPackage(PackageIdentity identity)
        {
            return _ignoredPackages.Any(ignored => identity.Id.Like(ignored));
        }

        private void AddOrUpdateLicense(
            ConcurrentDictionary<LicenseNameAndVersion, LicenseValidationResult> result,
            IPackageMetadata info,
            LicenseInformationOrigin origin,
            ValidationError error,
            string? license = null)
        {
            var newValue = new LicenseValidationResult(
                info.Identity.Id,
                info.Identity.Version,
                info.ProjectUrl?.ToString(),
                license,
                origin,
                new List<ValidationError> { error });
            result.AddOrUpdate(new LicenseNameAndVersion(info.Identity.Id, info.Identity.Version),
                key => CreateResult(key, newValue),
                (key, oldValue) => UpdateResult(key, oldValue, newValue));
        }

        private void AddOrUpdateLicense(
            ConcurrentDictionary<LicenseNameAndVersion, LicenseValidationResult> result,
            IPackageMetadata info,
            LicenseInformationOrigin origin,
            string? license = null)
        {
            var newValue = new LicenseValidationResult(
                info.Identity.Id,
                info.Identity.Version,
                info.ProjectUrl?.ToString(),
                license,
                origin);
            result.AddOrUpdate(new LicenseNameAndVersion(info.Identity.Id, info.Identity.Version),
                key => CreateResult(key, newValue),
                (key, oldValue) => UpdateResult(key, oldValue, newValue));
        }

        private LicenseValidationResult UpdateResult(LicenseNameAndVersion _,
            LicenseValidationResult oldValue,
            LicenseValidationResult newValue)
        {
            oldValue.ValidationErrors.AddRange(newValue.ValidationErrors);
            if (oldValue.License is null && newValue.License is not null)
            {
                oldValue.License = newValue.License;
                oldValue.LicenseInformationOrigin = newValue.LicenseInformationOrigin;
            }
            return oldValue;
        }

        private LicenseValidationResult CreateResult(LicenseNameAndVersion _, LicenseValidationResult newValue)
        {
            return newValue;
        }

        private void ValidateLicenseByMetadata(IPackageMetadata info,
            string context,
            ConcurrentDictionary<LicenseNameAndVersion, LicenseValidationResult> result)
        {
            switch (info.LicenseMetadata!.Type)
            {
                case LicenseType.Expression:
                    string licenseId = info.LicenseMetadata!.License;
                    if (IsLicenseValid(licenseId))
                    {
                        AddOrUpdateLicense(result,
                            info,
                            LicenseInformationOrigin.Expression,
                            info.LicenseMetadata.License);
                    }
                    else
                    {
                        AddOrUpdateLicense(result,
                            info,
                            LicenseInformationOrigin.Expression,
                            new ValidationError(GetLicenseNotAllowedMessage(info.LicenseMetadata.License), context),
                            info.LicenseMetadata.License);
                    }

                    break;
                default:
                    AddOrUpdateLicense(result,
                        info,
                        LicenseInformationOrigin.Unknown,
                        new ValidationError(
                            $"Validation for licenses of type {info.LicenseMetadata!.Type} not yet supported",
                            context));
                    break;
            }
        }

        private async Task ValidateLicenseByUrl(IPackageMetadata info,
            string context,
            ConcurrentDictionary<LicenseNameAndVersion, LicenseValidationResult> result)
        {
            if (info.LicenseUrl!.IsAbsoluteUri)
            {
                try
                {
                    await _fileDownloader.DownloadFile(info.LicenseUrl,
                        $"{info.Identity.Id}__{info.Identity.Version}.html");
                }
                catch (Exception e)
                {
                    throw new LicenseDownloadException(e, context, info.Identity);
                }
            }

            if (_licenseMapping.TryGetValue(info.LicenseUrl, out string? licenseId))
            {
                if (IsLicenseValid(licenseId))
                {
                    AddOrUpdateLicense(result,
                        info,
                        LicenseInformationOrigin.Url,
                        licenseId);
                }
                else
                {
                    AddOrUpdateLicense(result,
                        info,
                        LicenseInformationOrigin.Url,
                        new ValidationError(GetLicenseNotAllowedMessage(licenseId), context),
                        licenseId);
                }
            }
            else if (!_allowedLicenses.Any())
            {
                AddOrUpdateLicense(result,
                    info,
                    LicenseInformationOrigin.Url,
                    info.LicenseUrl.ToString());
            }
            else
            {
                AddOrUpdateLicense(result,
                    info,
                    LicenseInformationOrigin.Url,
                    new ValidationError($"Cannot determine License type for url {info.LicenseUrl}", context),
                    info.LicenseUrl.ToString());
            }
        }

        private bool IsLicenseValid(string licenseId)
        {
            if (!_allowedLicenses.Any())
            {
                return true;
            }

            foreach (string allowedLicense in _allowedLicenses)
            {
                if (allowedLicense.Equals(licenseId))
                {
                    return true;
                }
            }

            return false;
        }

        private string GetLicenseNotAllowedMessage(string license)
        {
            return $"License {license} not found in list of supported licenses";
        }

        private record LicenseNameAndVersion(string LicenseName, INuGetVersion Version);
    }
}
