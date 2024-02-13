// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

namespace NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core
{
    public class PackagesConfigReaderException : Exception
    {
        public PackagesConfigReaderException(string? message) : base(message) { }
    }
}
