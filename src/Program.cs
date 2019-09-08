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
                            System.Console.WriteLine("Project Reference(s) Analysis...");
                            bool licensesHasRetrieved = methods.PrintReferencesAsync(options.ProjectDirectory, options.UniqueOutput, options.JsonOutput, options.Output).Result;
                        }
                        return Task.FromResult(0);
                    },
                    errors => Task.FromResult(1));

            }).GetAwaiter().GetResult();
            
        }
    }
}
