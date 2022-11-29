namespace NugetUtility
{
    public class InvalidLicensesException<T> : Exception
    {
        public InvalidLicensesException(IValidationResult<T> validationResult, ICollection<string> allowedLicenses)
            : base(GetMessage(validationResult, allowedLicenses))
        {
        }

        private static string GetMessage(IValidationResult<T> validationResult, ICollection<string> allowedLicenses)
        {
            allowedLicenses ??= Array.Empty<string>();
            var message = $"Only the following licenses are allowed: {string.Join(", ", allowedLicenses.ToArray())}{Environment.NewLine}";

            if (validationResult is IValidationResult<KeyValuePair<string, Package>> packageValidation)
            {
                return message + string.Join(Environment.NewLine, packageValidation.InvalidPackages.Select(x =>
                {
                    return $"Project ({x.Key}) Package({x.Value.Metadata.Id}-{x.Value.Metadata.Version}) LicenseUrl({x.Value.Metadata.LicenseUrl}) License Type ({x.Value.Metadata.License?.Text})";
                }));
            }
            else if (validationResult is IValidationResult<LibraryInfo> libraryInfos)
            {
                return message + string.Join(Environment.NewLine, libraryInfos.InvalidPackages.Select(x =>
                {
                    return $"Project ({x.Projects}) Package({x.PackageName}-{x.PackageVersion}) LicenseUrl({x.LicenseUrl}) License Type ({x.LicenseType})";
                }));
            }

            return message;
        }
    }
}