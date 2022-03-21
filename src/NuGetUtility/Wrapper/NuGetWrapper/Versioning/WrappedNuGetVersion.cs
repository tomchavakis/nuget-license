using NuGet.Versioning;

namespace NuGetUtility.Wrapper.NuGetWrapper.Versioning
{
    internal class WrappedNuGetVersion : INuGetVersion
    {
        private readonly NuGetVersion _version;

        public WrappedNuGetVersion(NuGetVersion version)
        {
            _version = version;
        }

        public WrappedNuGetVersion(string version)
        {
            _version = new NuGetVersion(version);
        }

        public override string ToString()
        {
            return _version.ToString();
        }

        public NuGetVersion Unwrap()
        {
            return _version;
        }
    }
}
