using NuGet.Packaging;
using NuGet.Protocol.Core.Types;

namespace NuGetUtility.LicenseValidator
{
    public class LicenseValidator
    {
        private readonly IEnumerable<LicenseId> _allowedLicenses;
        private readonly List<LicenseValidationError> _errors = new List<LicenseValidationError>();
        private readonly Dictionary<Uri, LicenseId> _licenseMapping;
        private readonly HashSet<ValidatedLicense> _validatedLicenses = new HashSet<ValidatedLicense>();

        public LicenseValidator(Dictionary<Uri, LicenseId> licenseMapping, IEnumerable<LicenseId> allowedLicenses)
        {
            _licenseMapping = licenseMapping;
            _allowedLicenses = allowedLicenses;
        }

        public async Task Validate(IAsyncEnumerable<IPackageSearchMetadata> downloadedInfo, string context)
        {
            await foreach (var info in downloadedInfo)
            {
                if (info.LicenseMetadata != null)
                {
                    ValidateLicenseByMetadata(info, context);
                }
                else if (info.LicenseUrl != null)
                {
                    ValidateLicenseByUrl(info, context);
                }
                else
                {
                    _errors.Add(new LicenseValidationError(context, info.Identity.Id, info.Identity.Version,
                        "No license information found"));
                }
            }
        }

        public IEnumerable<LicenseValidationError> GetErrors()
        {
            return _errors;
        }

        public IEnumerable<ValidatedLicense> GetValidatedLicenses()
        {
            return _validatedLicenses;
        }

        private void ValidateLicenseByMetadata(IPackageSearchMetadata info, string context)
        {
            switch (info.LicenseMetadata!.Type)
            {
                case LicenseType.Expression:
                    var licenseId = new LicenseId(info.LicenseMetadata!.License, info.LicenseMetadata!.Version);
                    if (IsLicenseValid(licenseId))
                    {
                        _validatedLicenses.Add(new ValidatedLicense(info.Identity.Id, info.Identity.Version,
                            new LicenseId(info.LicenseMetadata.License, info.LicenseMetadata.Version)));
                    }
                    else
                    {
                        _errors.Add(new LicenseValidationError(context, info.Identity.Id, info.Identity.Version,
                            GetLicenseNotAllowedMessage(info.LicenseMetadata.License, info.LicenseMetadata.Version)));
                    }

                    break;
                default:
                    _errors.Add(new LicenseValidationError(context, info.Identity.Id, info.Identity.Version,
                        $"Validation for licenses of type {info.LicenseMetadata!.Type} not yet supported"));
                    break;
            }
        }

        private void ValidateLicenseByUrl(IPackageSearchMetadata info, string context)
        {
            if (_licenseMapping.TryGetValue(info.LicenseUrl, out var licenseId))
            {
                if (IsLicenseValid(licenseId))
                {
                    _validatedLicenses.Add(new ValidatedLicense(info.Identity.Id, info.Identity.Version, licenseId));
                }
                else
                {
                    _errors.Add(new LicenseValidationError(context, info.Identity.Id, info.Identity.Version,
                        GetLicenseNotAllowedMessage(licenseId.Id, licenseId.Version)));
                }
            }
            else if (!_allowedLicenses.Any())
            {
                _validatedLicenses.Add(new ValidatedLicense(info.Identity.Id, info.Identity.Version,
                    new LicenseId(info.LicenseUrl.ToString())));
            }
            else
            {
                _errors.Add(new LicenseValidationError(context, info.Identity.Id, info.Identity.Version,
                    $"Cannot determine License type for url {info.LicenseUrl}"));
            }
        }

        private bool IsLicenseValid(LicenseId licenseId)
        {
            if (!_allowedLicenses.Any())
            {
                return true;
            }

            foreach (var allowedLicense in _allowedLicenses)
            {
                if (allowedLicense.Id.Equals(licenseId.Id) &&
                    (allowedLicense.Version?.Equals(licenseId.Version) ?? true))
                {
                    return true;
                }
            }

            return false;
        }

        private string GetLicenseNotAllowedMessage(string id, Version? version)
        {
            var versionString = version == null ? "" : $"({version})";
            return $"License type {id}{versionString} not found in list of supported licenses";
        }
    }
}
