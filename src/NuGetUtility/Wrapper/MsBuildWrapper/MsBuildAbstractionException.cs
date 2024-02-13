// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

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
