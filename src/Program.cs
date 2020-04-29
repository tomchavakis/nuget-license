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
            HandleInvalidLicenses(methods, projectsWithPackages, options.AllowedLicenseType);
            var mappedLibraryInfo = methods.MapPackagesToLibraryInfo(projectsWithPackages);

            if (options.Print == true)
            {
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

        private static void HandleInvalidLicenses(Methods methods, Dictionary<string, PackageList> projectsWithPackages, ICollection<string> allowedLicenseType)
        {
            var invalidPackages = methods.ValidateLicenses(projectsWithPackages);

            if (!invalidPackages.IsValid)
            {
                throw new InvalidLicensesException(invalidPackages, allowedLicenseType);
            }
        }
    }
}
