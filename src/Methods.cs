using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace NugetUtility
{
    public class Methods
    {
        public Methods()
        {

        }

        private string nugetUrl = "https://api-v2v3search-0.nuget.org:443/query?q=";

        /// <summary>
        /// Retreive the project references from csproj file
        /// </summary>
        /// <param name="projectPath">The Project Path</param>
        /// <returns></returns>
        public IEnumerable<string> GetProjectReferences(string projectPath)
        {
            IEnumerable<string> references = new List<string>();
            XDocument projDefinition = XDocument.Load(projectPath);
            try
            {
                references = projDefinition
                    .Element("Project")
                    .Elements("ItemGroup")
                    .Elements("PackageReference")
                    .Attributes("Include")
                    .Select(i => i.Value);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

            return references;
        }


        /// <summary>
        /// Get Nuget References per project
        /// </summary>
        /// <param name="project">project name</param>
        /// <param name="references">List of projects</param>
        /// <returns></returns>
        public async Task<IEnumerable<Tuple<string, string, string>>> GetNugetInformationAsync(string project, IEnumerable<string> references)
        {
            System.Console.WriteLine(project);
            List<Tuple<string, string, string>> licenses = new List<Tuple<string, string, string>>();
            foreach (var reference in references)
            {
                using (var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) })
                {
                    string requestUrl = nugetUrl + reference;
                    try
                    {
                        HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                        HttpResponseMessage response = null;
                        response = await httpClient.SendAsync(req);
                        string responseText = await response.Content.ReadAsStringAsync();
                        Package result = JsonConvert.DeserializeObject<Package>(responseText);
                        licenses.Add(Tuple.Create(reference, result.data.FirstOrDefault().version, result.data.FirstOrDefault().licenseUrl));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            return licenses;
        }

        public string GetProjectExtension()
        {
            return ".csproj";
        }

        public async Task<bool> PrintReferencesAsync(string projectPath, bool uniqueList)
        {
            bool result = false;
            List<Tuple<string, string, string>> licenses_new = new List<Tuple<string, string, string>>();
            IEnumerable<Tuple<string, string, string>> licenses = new List<Tuple<string, string, string>>();

            var projects = Directory.GetFiles(projectPath, "*.*", SearchOption.AllDirectories).Where(i => i.EndsWith(GetProjectExtension()));
            foreach (var item in projects)
            {
                IEnumerable<string> references = this.GetProjectReferences(item);

                if (uniqueList)
                {
                    licenses = await this.GetNugetInformationAsync(item, references);
                    licenses_new.AddRange(licenses.ToList());
                }
                else
                {
                    licenses = await this.GetNugetInformationAsync(item, references);
                    PrintLicenses(licenses);
                }

                result = true;

            }

            if (uniqueList)
                PrintUniqueLicenses(licenses_new);

            return result;

        }

        public void PrintLicenses(IEnumerable<Tuple<string, string, string>> licenses)
        {
            if (licenses.Count() > 0)
            {
                Console.WriteLine(licenses.ToStringTable(
                new[] { "Reference", "Version", "Licence" },
                a => a.Item1 != null ? a.Item1 : "---", a => a.Item2 != null ? a.Item2 : "", a => a.Item3 != null ? a.Item3 : "---"));
            }
        }

        public void PrintUniqueLicenses(IEnumerable<Tuple<string, string, string>> licenses)
        {

            if (licenses.Count() > 0)
            {
                licenses = licenses.Distinct().ToList();
                Console.WriteLine(licenses.ToStringTable(
                new[] { "Reference", "Version", "Licence" },
                a => a.Item1 != null ? a.Item1 : "---", a => a.Item2 != null ? a.Item2 : "", a => a.Item3 != null ? a.Item3 : "---"));
            }
        }


    }
}