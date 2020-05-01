using System;
using System.Collections.Generic;
using System.Linq;

namespace NugetUtility
{
    public class InvalidLicensesException : Exception
    {
        public InvalidLicensesException(ValidationResult validationResult, ICollection<string> allowedLicenses) : base(GetMessage(validationResult, allowedLicenses))
        {
        }

        private static string GetMessage(ValidationResult validationResult, ICollection<string> allowedLicenses)
        {
            allowedLicenses = allowedLicenses ?? Array.Empty<string>();

            return $"Only the following packages are allowed: {string.Join(", ", allowedLicenses.ToArray())}{Environment.NewLine}" 
                + string.Join(Environment.NewLine, validationResult.InvalidPackages.Select(x =>
            {
                return $"Project ({x.Key}) Package({x.Value.Metadata.Id}-{x.Value.Metadata.Version}) LicenseUrl({x.Value.Metadata.LicenseUrl}) License Type ({x.Value.Metadata.License?.Text})";
            }));
        }
    }
}