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

        public override int GetHashCode()
        {
            return _framework.GetHashCode();
        }
    }
}
