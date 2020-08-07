using FluentAssertions;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace NugetUtility.Tests
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class ProgramTests
    {
        [TestCase("-bad val")]
        [TestCase("-i good -bad val")]
        [Test]
        public async Task Main_Should_Error_With_Invalid_Args(string args)
        {
            var status = await Program.Main(args.Split(' '));

            status.Should().Be(1);
        }

        [Test]
        public async Task Main_Should_Error_Empty_Input()
        {
            var status = await Program.Main("-j".Split(' '));

            status.Should().Be(1);
        }

        [TestCase("-i " + TestSetup.ThisProjectSolutionPath + @" --allowed-license-types ../../../SampleAllowedLicenses.json")]
        [Test]
        public async Task Main_Should_ReturnNegative_When_InvalidLicenses(string args)
        {
            var status = await Program.Main(args.Split(' '));

            status.Should().Be(-1);
        }

        [Test]
        public async Task Main_Should_Run_For_This_Project()
        {
            var status = await Program.Main(@"-i ../../../".Split(' '));

            status.Should().Be(0);
        }

        [Test]
        public async Task Main_Should_Run_For_This_Solution_With_Custom_Outfile()
        {
            const string args = "-i " + TestSetup.ThisProjectSolutionPath + " -j --outfile custom.json";
            var status = await Program.Main(args.Split(' '));

            status.Should().Be(0);
            File.Exists("custom.json").Should().BeTrue();
        }

        [Test]
        public async Task Main_Should_Run_For_This_Solution_With_ManualInformation()
        {
            const string args = "-i " + TestSetup.ThisProjectSolutionPath + @" -j --outfile custom-manual.json --manual-package-information ../../../SampleManualInformation.json";
            var status = await Program.Main(args.Split(' '));

            status.Should().Be(0);
            var outputFile = new FileInfo("custom-manual.json");
            outputFile.Exists.Should().BeTrue();;
            Utilties.ReadListFromFile<LibraryInfo>(outputFile.FullName)
                .Should().NotContain(l => l.PackageName == "ASamplePackage");
        }

#if WINDOWS
        [Test]
        public async Task Main_Should_Run_For_This_Solution_With_Custom_Outfile_FullPath()
        {
            const string args = "-i " + TestSetup.ThisProjectSolutionPath + @" -j --outfile c:\temp\custom.json";
            var tempDirectory = new DirectoryInfo(@"C:\temp");
            if (!tempDirectory.Exists)
            {
                tempDirectory.Create();
            }
            var status = await Program.Main(args.Split(' '));

            status.Should().Be(0);
            File.Exists(@"c:\temp\custom.json").Should().BeTrue();
            File.Delete(@"c:\temp\custom.json");
        }
#endif
    }
}
