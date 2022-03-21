using NuGet.Versioning;
using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

namespace NuGetUtility.Test.Helper.NuGet.Versioning
{
    internal class WrappedNuGetVersion : INuGetVersion
    {
        private readonly NuGetVersion _internalVersion;

        public WrappedNuGetVersion(NuGetVersion internalVersion)
        {
            _internalVersion = internalVersion;
        }

        public override string ToString()
        {
            return _internalVersion.ToString()!;
        }
    }
}
