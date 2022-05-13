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

        public override bool Equals(object? other)
        {
            if (other == null)
            {
                return false;
            }

            if (other.GetType() != GetType())
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals((other as WrappedNuGetVersion)!);
        }

        private bool Equals(WrappedNuGetVersion other)
        {
            return _version.Equals(other._version);
        }

        public override int GetHashCode()
        {
            return _version.GetHashCode();
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
