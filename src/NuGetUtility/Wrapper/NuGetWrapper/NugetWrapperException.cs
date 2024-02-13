// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

namespace NuGetUtility.Wrapper.NuGetWrapper
{
    internal class NugetWrapperException : Exception
    {
        public NugetWrapperException(string message)
            : base(message) { }
    }
}
