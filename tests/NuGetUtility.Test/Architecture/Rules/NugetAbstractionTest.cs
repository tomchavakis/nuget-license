

namespace NuGetUtility.Test.Architecture.Rules
{
    public class NugetAbstractionTest : ArchitectureTest
    {
        public NugetAbstractionTest() : base() {}

        [Test]
        public void TestOnlyNugetWrapperHasDependencyToNuget()
        {
            Types.That()
                .DoNotResideInNamespace($"{AssemblyNames.NuGetUtility}.{nameof(Wrapper)}.{nameof(Wrapper.NuGetWrapper)}")
                .And().DoNotResideInNamespace($"{AssemblyNames.NuGetUtility}.{nameof(Program)}")
                .ShouldNot().HaveDependencyOn(nameof(NuGet))
                .Assert($"Only the {nameof(Wrapper.NuGetWrapper)} should have dependencies to {nameof(NuGet)}.");
        }
    }
}
