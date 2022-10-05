using NuGet.Versioning;
using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

namespace NuGetUtility.Test.Helper.NuGet.Version
{
    public class MockedNugetVersion : INuGetVersion, IEquatable<INuGetVersion>
    {
        private readonly string _semverString;
        public MockedNugetVersion(string semverString)
        {
            _semverString = semverString;
        }
        public MockedNugetVersion(NuGetVersion version)
        {
            _semverString = version.ToString();
        }

        public override string ToString()
        {
            return _semverString;
        }
        public bool Equals(INuGetVersion? other)
        {
            return _semverString == other?.ToString();
        }
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is INuGetVersion other)
            {
                return Equals(other);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return _semverString.GetHashCode();
        }
    }
}
