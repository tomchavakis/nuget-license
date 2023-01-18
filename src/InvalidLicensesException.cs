using System;
using System.Collections.Generic;
using System.Linq;
namespace NugetUtility
{
    public class InvalidLicensesException<T> : Exception
    {
        public InvalidLicensesException(ValidationResult<T> validationResult, PackageOptions options)
            : base(GetMessage(validationResult, options))
        {
        }

        private static string GetMessage(
            IValidationResult<T> validationResult,
            PackageOptions options)
        {
            var allowedLicenses = options?.AllowedLicenseType ?? Array.Empty<string>();
            var forbiddenLicenses = options?.ForbiddenLicenseType ?? Array.Empty<string>();
            
            var message = allowedLicenses.Any()
                ? $"Only the following licenses are allowed: {string.Join(", ", allowedLicenses.ToArray())}{Environment.NewLine}"
                : $"The following licenses are forbidden: {string.Join(", ", forbiddenLicenses.ToArray())}{Environment.NewLine}";

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