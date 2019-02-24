using CommandLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NugetUtility
{
    class Program
    {
        static void Main(string[] args)
        {
            IEnumerable<Tuple<string, string, string>> licences = new List<Tuple<string, string, string>>();

            Methods methods = new Methods();
            Task.Run(async () =>
            {
                var result = CommandLine.Parser.Default.ParseArguments<PackageOptions>(args);

                await result.MapResult(
                    (PackageOptions options) =>
                    {
                        if (string.IsNullOrEmpty(options.ProjectDirectory))
                        {
                            Console.WriteLine("ERROR(S):");
                            Console.WriteLine("-i\tInput the Directory Path (csproj file)");
                        }
                        else
                        {
                            System.Console.WriteLine("Nuget Reference(s) Analysis...");
                            licences = methods.PrintReferencesAsync(options.ProjectDirectory).Result;
                        }
                        return Task.FromResult(0);
                    },
                    errors => Task.FromResult(1));

            }).GetAwaiter().GetResult();

            if (licences.Count() > 0)
            {
                Console.WriteLine(licences.ToStringTable(
                new[] { "Reference", "Version", "Licence" },
                a => a.Item1 != null ? a.Item1 : "---", a => a.Item2 != null ? a.Item2 : "", a => a.Item3 != null ? a.Item3 : "---"));
            }

        }
    }
}
