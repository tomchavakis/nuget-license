using CommandLine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NugetUtility
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<PackageOptions>(args);
            return await result.MapResult(
                options => Execute(options),
                errors => Task.FromResult(1));
        }

        private static async Task<int> Execute(PackageOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ProjectDirectory))
            {
                Console.WriteLine("ERROR(S):");
                Console.WriteLine("-i\tInput the Directory Path (csproj file)");

                return 1;
            }

            Methods methods = new Methods(options);
            var projectsWithPackages = await methods.GetPackages();
            var mappedLibraryInfo = methods.MapPackagesToLibraryInfo(projectsWithPackages);
            HandleInvalidLicenses(methods, mappedLibraryInfo, options.AllowedLicenseType);

            if (options.ExportLicenseTexts)
            {
                await methods.ExportLicenseTexts(mappedLibraryInfo);
            }

            if (options.Print == true)
            {
                Console.WriteLine();
                Console.WriteLine("Project Reference(s) Analysis...");
                methods.PrintLicenses(mappedLibraryInfo);
            }

            if (options.JsonOutput)
            {
                methods.SaveAsJson(mappedLibraryInfo);
            }
            else
            {
                methods.SaveAsTextFile(mappedLibraryInfo);
            }

            return 0;
        }

        private static void HandleInvalidLicenses(Methods methods, List<LibraryInfo> libraries, ICollection<string> allowedLicenseType)
        {
            var invalidPackages = methods.ValidateLicenses(libraries);

            if (!invalidPackages.IsValid)
            {
                throw new InvalidLicensesException<LibraryInfo>(invalidPackages, allowedLicenseType);
            }
        }
    }
}
