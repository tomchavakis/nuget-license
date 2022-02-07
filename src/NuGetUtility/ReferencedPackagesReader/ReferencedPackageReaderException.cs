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
