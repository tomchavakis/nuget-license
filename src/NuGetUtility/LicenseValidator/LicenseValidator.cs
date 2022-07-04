using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using NuGetUtility.Wrapper.HttpClientWrapper;

namespace NuGetUtility.LicenseValidator
{
    public class LicenseValidator
    {
        private readonly IEnumerable<string> _allowedLicenses;
        private readonly List<LicenseValidationError> _errors = new List<LicenseValidationError>();
        private readonly IFileDownloader _fileDownloader;
        private readonly Dictionary<Uri, string> _licenseMapping;
        private readonly HashSet<ValidatedLicense> _validatedLicenses = new HashSet<ValidatedLicense>();

        public LicenseValidator(Dictionary<Uri, string> licenseMapping, IEnumerable<string> allowedLicenses,
            IFileDownloader fileDownloader)
        {
            _licenseMapping = licenseMapping;
            _allowedLicenses = allowedLicenses;
            _fileDownloader = fileDownloader;
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
                    await ValidateLicenseByUrl(info, context);
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
                    var licenseId = info.LicenseMetadata!.License;
                    if (IsLicenseValid(licenseId))
                    {
                        _validatedLicenses.Add(new ValidatedLicense(info.Identity.Id, info.Identity.Version,
                            info.LicenseMetadata.License));
                    }
                    else
                    {
                        _errors.Add(new LicenseValidationError(context, info.Identity.Id, info.Identity.Version,
                            GetLicenseNotAllowedMessage(info.LicenseMetadata.License)));
                    }

                    break;
                default:
                    _errors.Add(new LicenseValidationError(context, info.Identity.Id, info.Identity.Version,
                        $"Validation for licenses of type {info.LicenseMetadata!.Type} not yet supported"));
                    break;
            }
        }

        private async Task ValidateLicenseByUrl(IPackageSearchMetadata info, string context)
        {
            if (info.LicenseUrl.IsAbsoluteUri)
            {
                await _fileDownloader.DownloadFile(info.LicenseUrl, $"{info.Identity.Id}__{info.Identity.Version}.txt");
            }

            if (_licenseMapping.TryGetValue(info.LicenseUrl, out var licenseId))
            {
                if (IsLicenseValid(licenseId))
                {
                    _validatedLicenses.Add(new ValidatedLicense(info.Identity.Id, info.Identity.Version, licenseId));
                }
                else
                {
                    _errors.Add(new LicenseValidationError(context, info.Identity.Id, info.Identity.Version,
                        GetLicenseNotAllowedMessage(licenseId)));
                }
            }
            else if (!_allowedLicenses.Any())
            {
                _validatedLicenses.Add(new ValidatedLicense(info.Identity.Id, info.Identity.Version,
                    info.LicenseUrl.ToString()));
            }
            else
            {
                _errors.Add(new LicenseValidationError(context, info.Identity.Id, info.Identity.Version,
                    $"Cannot determine License type for url {info.LicenseUrl}"));
            }
        }

        private bool IsLicenseValid(string licenseId)
        {
            if (!_allowedLicenses.Any())
            {
                return true;
            }

            foreach (var allowedLicense in _allowedLicenses)
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
    }
}
