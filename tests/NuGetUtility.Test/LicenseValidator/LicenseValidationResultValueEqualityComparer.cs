using NuGetUtility.LicenseValidator;

namespace NuGetUtility.Test.LicenseValidator
{
    public class LicenseValidationResultValueEqualityComparer : IEqualityComparer<LicenseValidationResult>
    {
        public bool Equals(LicenseValidationResult? x, LicenseValidationResult? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.ValidationErrors.SequenceEqual(y.ValidationErrors) && x.License == y.License && x.LicenseInformationOrigin == y.LicenseInformationOrigin && x.PackageId == y.PackageId && x.PackageVersion.Equals(y.PackageVersion) && x.PackageProjectUrl == y.PackageProjectUrl;
        }
        public int GetHashCode(LicenseValidationResult obj)
        {
            return HashCode.Combine(GetHashCode(obj.ValidationErrors), obj.License, (int)obj.LicenseInformationOrigin, obj.PackageId, obj.PackageVersion, obj.PackageProjectUrl);
        }
        private HashCode GetHashCode(List<ValidationError> validationErrors)
        {
            var code = new HashCode();
            foreach (var error in validationErrors)
            {
                code.Add(error);
            }
            return code;
        }
    }
}
