// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

namespace NuGetUtility.Wrapper.NuGetWrapper.Frameworks
{
    public interface INuGetFramework
    {
        string? ToString();
        bool Equals(string targetFramework);
    }
}
