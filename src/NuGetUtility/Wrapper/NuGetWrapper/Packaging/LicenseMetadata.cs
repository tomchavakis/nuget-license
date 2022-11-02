using System.ComponentModel;
using OriginalLicenseType = NuGet.Packaging.LicenseType;
using WrappedLicenseType = NuGetUtility.Wrapper.NuGetWrapper.Packaging.LicenseType;

namespace NuGetUtility.Wrapper.NuGetWrapper.Packaging
{
    public record LicenseMetadata(WrappedLicenseType Type, string License)
    {
        public static implicit operator LicenseMetadata?(NuGet.Packaging.LicenseMetadata? metadata) => metadata == null ? null : new LicenseMetadata(Convert(metadata.Type), metadata.License);

        private static WrappedLicenseType Convert(OriginalLicenseType type)
        {
            return type switch
            {
                OriginalLicenseType.Expression => WrappedLicenseType.Expression,
                OriginalLicenseType.File => WrappedLicenseType.File,
                _ => throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(OriginalLicenseType)),
            };
        }
    }
}
