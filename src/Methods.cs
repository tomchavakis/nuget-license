using System;
using System.Collections.Generic;
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


        public async Task<IEnumerable<Tuple<string, string, string>>> GetNugetInformationAsync(IEnumerable<string> references)
        {
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
                        //licenses.Add(license);
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

        public async Task<IEnumerable<Tuple<string, string, string>>> PrintReferencesAsync(string projectPath)
        {
            IEnumerable<Tuple<string, string, string>> licences = new List<Tuple<string, string, string>>();
            IEnumerable<string> references = this.GetProjectReferences(projectPath);
            licences = await this.GetNugetInformationAsync(references);
            return licences;
        }

    }
}