using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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

            Assert.AreEqual(1, status);
        }

        [Test]
        public async Task Main_Should_Error_Empty_Input()
        {
            var status = await Program.Main("-j".Split(' '));

            Assert.AreEqual(1, status);
        }

        [TestCase("-i " + TestSetup.ThisProjectSolutionPath + @" --allowed-license-types ../../../SampleAllowedLicenses.json")]
        [Test]
        public void Main_Should_Throw_When_InvalidLicenses(string args)
        {
            Assert.ThrowsAsync<InvalidLicensesException>(async () => await Program.Main(args.Split(' ')));
        }

        [Test]
        public async Task Main_Should_Run_For_This_Project()
        {
            var status = await Program.Main(@"-i ../../../".Split(' '));

            Assert.AreEqual(0, status);
        }

        [Test]
        public async Task Main_Should_Run_For_This_Solution_With_Custom_Outfile()
        {
            const string args = "-i " + TestSetup.ThisProjectSolutionPath + " -j --outfile custom.json";
            var status = await Program.Main(args.Split(' '));

            Assert.AreEqual(0, status);
            Assert.IsTrue(File.Exists("custom.json"));
        }

        [Test]
        public async Task Main_Should_Run_For_This_Solution_With_ManualInformation()
        {
            const string args = "-i " + TestSetup.ThisProjectSolutionPath + @" -j --outfile custom-manual.json --manual-package-information ../../../SampleManualInformation.json";
            var status = await Program.Main(args.Split(' '));

            Assert.AreEqual(0, status);
            var outputFile = new FileInfo("custom-manual.json");
            Assert.IsTrue(outputFile.Exists, "File doesn't exist");
            var outputList = Utilties.ReadListFromFile<LibraryInfo>(outputFile.FullName);
            Assert.IsTrue(outputList.Any(l => l.PackageName == "ASamplePackage"));
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

            Assert.AreEqual(0, status);
            Assert.IsTrue(File.Exists(@"c:\temp\custom.json"));
            File.Delete(@"c:\temp\custom.json");
        }
#endif
    }
}
