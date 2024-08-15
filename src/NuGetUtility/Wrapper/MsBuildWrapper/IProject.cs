// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using System.Diagnostics.CodeAnalysis;

namespace NuGetUtility.Wrapper.MsBuildWrapper
{
    public interface IProject
    {
        public string FullPath { get; }

        bool TryGetAssetsPath([NotNullWhen(true)] out string assetsFile);

        IEnumerable<string> GetEvaluatedIncludes();
    }
}
