using System;
using System.Collections;
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
            var allowedMessage = (allowedLicenses?.Any() ?? false)
                ? ("The following licenses are allowed:" + string.Join(",", allowedLicenses.ToArray()) + Environment.NewLine)
                : string.Empty;

            return allowedMessage + string.Join(Environment.NewLine, validationResult.InvalidPackages.Select(x =>
            {
                return $"Project ({x.Key}) Package({x.Value.Metadata.Id}-{x.Value.Metadata.Version}) LicenseUrl({x.Value.Metadata.LicenseUrl}) License Type ({x.Value.Metadata.License?.Text})";
            }));
        }
    }
}