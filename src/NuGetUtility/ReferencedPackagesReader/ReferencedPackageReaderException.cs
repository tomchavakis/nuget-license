// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

namespace NuGetUtility.ReferencedPackagesReader
{
    public class ReferencedPackageReaderException : Exception
    {
        public ReferencedPackageReaderException(Exception inner)
            : base("", inner) { }

        public ReferencedPackageReaderException(string message, Exception? inner = null)
            : base(message, inner) { }
    }
}
