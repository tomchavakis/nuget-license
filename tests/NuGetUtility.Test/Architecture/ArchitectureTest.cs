using System.Reflection;
using NetArchTest.Rules;

namespace NuGetUtility.Test.Architecture
{
    public abstract class ArchitectureTest
    {
        protected Types Types { get; }

        protected ArchitectureTest()
        {
            this.Types = Types.InAssemblies(new[] { Assembly.Load(AssemblyNames.NuGetUtility) });
        }

        internal static class AssemblyNames
        {
            internal const string NuGetUtility = "NugetUtility";
        }
    }
}
