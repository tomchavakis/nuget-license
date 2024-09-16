// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NuGet.Frameworks;

namespace NuGetUtility.Wrapper.NuGetWrapper.Frameworks
{
    internal class WrappedNuGetFramework : INuGetFramework
    {
        private readonly NuGetFramework _framework;

        public WrappedNuGetFramework(NuGetFramework framework)
        {
            _framework = framework;
        }

        public override bool Equals(object? obj)
        {
            if (obj is WrappedNuGetFramework wrapped)
            {
                return _framework.Equals(wrapped._framework);
            }

            return false;
        }

        public bool Equals(string targetFramework)
        {
            var other = NuGetFramework.Parse(targetFramework);
            return _framework.Equals(other);
        }

        public override int GetHashCode()
        {
            return _framework.GetHashCode();
        }
    }
}
