namespace NuGetUtility.Wrapper.MsBuildWrapper
{
    public class MsBuildAbstractionException : Exception
    {
        public MsBuildAbstractionException(string message)
            : base(message) { }

        public MsBuildAbstractionException(string message, Exception inner)
            : base(message, inner) { }
    }
}
