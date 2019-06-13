using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace NugetUtility
{
    public class Methods
    {
        public Methods()
        {
        }
        //private string nugetUrl = "https://api-v2v3search-0.nuget.org:443/query?q=";

        private string nugetUrl = "https://api.nuget.org/v3-flatcontainer/";

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
                             .Select(refElem => (refElem.Attribute("Include") == null ? "" : refElem.Attribute("Include").Value) + "," +
                                                (refElem.Attribute("Version") == null ? "" : refElem.Attribute("Version").Value));
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
        public async Task<Dictionary<string, Package>> GetNugetInformationAsync(string project, IEnumerable<string> references)
        {
            System.Console.WriteLine(project);
            Dictionary<string, Package> licenses = new Dictionary<string, Package>();
            foreach (var reference in references)
            {
                string referenceName = reference.Split(',')[0];
                string versionNumber = reference.Split(',')[1];
                using (var httpClient = new HttpClient {Timeout = TimeSpan.FromSeconds(10)})
                {
                    string requestUrl = nugetUrl + referenceName + "/" + versionNumber + "/" + referenceName + ".nuspec";
                    Console.WriteLine(requestUrl);
                    try
                    {
                        HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                        HttpResponseMessage response = null;
                        response = await httpClient.SendAsync(req);
                        string responseText = await response.Content.ReadAsStringAsync();
                        XmlSerializer serializer = new XmlSerializer(typeof(Package));
                        using (TextReader writer = new StringReader(responseText))
                        {
                            try
                            {
                                Package result = (Package) serializer.Deserialize(new NamespaceIgnorantXmlTextReader(writer));
                                licenses.Add(reference, result);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                throw;
                            }
                        }

                        //Package result = JsonConvert.DeserializeObject<Package>(responseText);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            foreach (var item in licenses)
            {
                Console.WriteLine(JsonConvert.SerializeObject(item.Key));
            }

            return licenses;
        }

        public string GetProjectExtension()
        {
            return ".csproj";
        }

        public async Task<bool> PrintReferencesAsync(string projectPath, bool uniqueList, bool output)
        {
            Console.WriteLine("output" + output);
            bool result = false;
            Dictionary<string, Package> licenses = new Dictionary<string, Package>();

            var projects = Directory.GetFiles(projectPath, "*.*", SearchOption.AllDirectories).Where(i => i.EndsWith(GetProjectExtension()));
            foreach (var item in projects)
            {
                IEnumerable<string> references = this.GetProjectReferences(item);

                if (uniqueList)
                {
                    licenses = await this.GetNugetInformationAsync(item, references);
                }
                else
                {
                    licenses = await this.GetNugetInformationAsync(item, references);
                    PrintLicenses(licenses);
                }

                result = true;
            }

            if (uniqueList)
                await PrintUniqueLicenses(licenses, output);

            return result;
        }

        public void PrintLicenses(Dictionary<string, Package> licenses)
        {
            if (licenses.Count() > 0)
            {
                Console.WriteLine(licenses.ToStringTable(new[] {"Reference", "Version", "Licence"},
                                                         a => a.Key != null ? a.Key : "---", a => a.Value.Metadata.License != null ? a.Value.Metadata.ProjectUrl : "",
                                                         a => a.Value.Metadata != null ? a.Value.Metadata.ProjectUrl : "---"));
            }
        }

        public async Task PrintUniqueLicenses(Dictionary<string, Package> licenses, bool output)
        {
            if (licenses.Count() > 0)
            {
                Console.WriteLine("#####PrintUniqueLicenses######");

                Console.WriteLine(licenses.ToStringTable(new[] {"Reference", "Licence", "Version", "LicenceType"},
                                                         a => a.Value.Metadata.Id ?? "---", a => a.Value.Metadata.LicenseUrl ?? "---",
                                                         a => a.Value.Metadata.Version ?? "---", a => (a.Value.Metadata.License != null ? a.Value.Metadata.License.Text : "---")));

                if (output)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var license in licenses)
                    {
                        Console.WriteLine("version:" + JsonConvert.SerializeObject(license.Value));
                        Package packageData = license.Value;

                        if (packageData != null)
                        {
                            //Console.WriteLine(JsonConvert.SerializeObject(packageData));
                            Console.WriteLine("#################################");

                            sb.Append(new string('#', 150));
                            sb.AppendLine();
                            sb.Append(license.Key);

                            sb.AppendLine();
                            sb.AppendLine();
                            sb.Append("project URL:");
                            sb.Append(packageData.Metadata.ProjectUrl ?? string.Empty);

                            sb.AppendLine();
                            sb.AppendLine();
                            sb.Append(packageData.Metadata.Description ?? string.Empty);

                            sb.AppendLine();
                            sb.AppendLine();
                            sb.Append("licenseUrl:");
                            sb.Append(packageData.Metadata.LicenseUrl ?? string.Empty);

                            sb.AppendLine();
                            sb.AppendLine();
                            sb.Append("license Type:");
                            sb.Append(packageData.Metadata.License != null ? packageData.Metadata.License.Text : string.Empty);

                            sb.AppendLine();
                            sb.AppendLine();
                            sb.Append(new string('#', 150));
                        }
                    }

                    File.WriteAllText("licences.txt", sb.ToString());
                }
            }
        }
    }
}