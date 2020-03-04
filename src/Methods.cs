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
            System.Console.WriteLine(Environment.NewLine + "project:" + project + Environment.NewLine);
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

        public async Task<bool> PrintReferencesAsync(string projectPath, bool uniqueList, bool jsonOutput, bool output)
        {
            bool result = false;
            List<Dictionary<string, Package>> licenses = new List<Dictionary<string, Package>>();

            var projects = Directory.GetFiles(projectPath, "*.*", SearchOption.AllDirectories).Where(i => i.EndsWith(GetProjectExtension()));
            foreach (var item in projects)
            {
                IEnumerable<string> references = this.GetProjectReferences(item);

                if (uniqueList)
                {
                    licenses.Add(await this.GetNugetInformationAsync(item, references));
                }
                else
                {
                    licenses.Add(await this.GetNugetInformationAsync(item, references));
                    PrintLicenses(licenses);
                }

                result = true;
            }

            if (jsonOutput)
                await PrintInJson(licenses);
            else if (uniqueList)
                await PrintUniqueLicenses(licenses, output);

            return result;
        }

        public void PrintLicenses(List<Dictionary<string, Package>> licenses)
        {
            if (licenses.Any())
            {
                Console.WriteLine(Environment.NewLine + "References:");

                foreach (var license in licenses)
                {
                    Console.WriteLine(license.ToStringTable(new[] {"Reference", "Licence", "Version", "LicenceType"},
                                                            a => a.Value.Metadata.Id ?? "---", a => a.Value.Metadata.LicenseUrl ?? "---",
                                                            a => a.Value.Metadata.Version ?? "---", a => (a.Value.Metadata.License != null ? a.Value.Metadata.License.Text : "---")));
                }
            }
        }

        public async Task PrintUniqueLicenses(List<Dictionary<string, Package>> licenses, bool output)
        {
            if (licenses.Any())
            {
                Console.WriteLine(Environment.NewLine + "References:");
                
                foreach (var license in licenses)
                {
                    Console.WriteLine(license.ToStringTable(new[] {"Reference", "Licence", "Version", "LicenceType"},
                                                            a => a.Value.Metadata.Id ?? "---", a => a.Value.Metadata.LicenseUrl ?? "---",
                                                            a => a.Value.Metadata.Version ?? "---", a => (a.Value.Metadata.License != null ? a.Value.Metadata.License.Text : "---")));
                }

                if (output)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var license in licenses)
                    {
                        foreach (var lic in license)
                        {
                            Package packageData = lic.Value;
                            if (packageData != null)
                            {
                                sb.Append(new string('#', 100));
                                sb.AppendLine();
                                sb.Append("Package:");
                                sb.Append(packageData.Metadata.Id);
                                sb.AppendLine();
                                sb.Append("Version:");
                                sb.Append(packageData.Metadata.Version);
                                sb.AppendLine();
                                sb.Append("project URL:");
                                sb.Append(packageData.Metadata.ProjectUrl ?? string.Empty);
                                sb.AppendLine();
                                sb.Append("Description:");
                                sb.Append(packageData.Metadata.Description ?? string.Empty);
                                sb.AppendLine();
                                sb.Append("licenseUrl:");
                                sb.Append(packageData.Metadata.LicenseUrl ?? string.Empty);
                                sb.AppendLine();
                                sb.Append("license Type:");
                                sb.Append(packageData.Metadata.License != null ? packageData.Metadata.License.Text : string.Empty);
                                sb.AppendLine();
                                sb.AppendLine();
                            }
                        }
                    }

                    File.WriteAllText("licences.txt", sb.ToString());
                }
            }
        }

        public async Task PrintInJson(List<Dictionary<string, Package>> licenses)
        {
            IList<LibraryInfo> libraryInfos = new List<LibraryInfo>();

            foreach (Dictionary<string,Package>  packageLicense in licenses)
            {
                foreach (KeyValuePair<string, Package> license in packageLicense)
                {
                    libraryInfos.Add(
                        new LibraryInfo
                        {
                            PackageName = license.Value.Metadata.Id ?? string.Empty,
                            PackageVersion = license.Value.Metadata.Version ?? string.Empty,
                            PackageUrl = license.Value.Metadata.ProjectUrl ?? string.Empty,
                            Description = license.Value.Metadata.Description ?? string.Empty,
                            LicenseType = license.Value.Metadata.License != null ? license.Value.Metadata.License.Text : string.Empty,
                            LicenseUrl = license.Value.Metadata.LicenseUrl ?? string.Empty
                        });
                }
            }
            
            var fileStream = new FileStream("licenses.json", FileMode.Create);
            using (var streamWriter = new StreamWriter(fileStream))
            {
                streamWriter.Write(JsonConvert.SerializeObject(libraryInfos));
                streamWriter.Flush();
            }
            
            fileStream.Close();
        }
    }
}