using NuGet.Versioning;

namespace NuGetUtility.Wrapper.NuGetWrapper.Versioning
{
    internal class WrappedNuGetVersion : INuGetVersion, IComparable
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

        public int CompareTo(object? other)
        {
            if (other is not WrappedNuGetVersion wrappedNuGetVersion)
            {
                throw new ArgumentException($"{nameof(other)} must be of type {nameof(WrappedNuGetVersion)} to be comparable.");
            }
            return _version.CompareTo(wrappedNuGetVersion._version);
        }

        public NuGetVersion Unwrap()
        {
            return _version;
        }

        internal static bool TryParse(string stringVersion, out WrappedNuGetVersion version)
        {
            if (NuGetVersion.TryParse(stringVersion, out NuGetVersion? internalVersion))
            {
                version = new WrappedNuGetVersion(internalVersion);
                return true;
            }
            version = default!;
            return false;
        }
    }
}
