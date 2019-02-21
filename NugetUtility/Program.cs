using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;

namespace NugetUtility
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> licences = new List<string>();

            Console.WriteLine("Application Started...");

            XDocument projDefinition = XDocument.Load("Project-Path");
            IEnumerable<string> references = projDefinition
                .Element("Project")
                .Elements("ItemGroup")
                .Elements("PackageReference")
                .Attributes("Include")
                .Select(i => i.Value);

            foreach (var reference in references)
            {
                using (var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10000) })
                {
                    string requestUrl = "https://api-v2v3search-0.nuget.org:443/query?q=" + reference;
                    try
                    {
                        HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                        HttpResponseMessage response = null;
                        response = httpClient.SendAsync(req).Result;
                        string responseText = response.Content.ReadAsStringAsync().Result;
                        Package result = JsonConvert.DeserializeObject<Package>(responseText);
                        licences.Add(reference + "," + result.data.FirstOrDefault().projectUrl + "," + result.data.FirstOrDefault().version + "," + result.data.FirstOrDefault().licenseUrl);
                    }
                    catch (Exception e)
                    {
                        Console.Write("FAIL                 .");
                        Console.WriteLine(e);
                    }
                }
            }

            foreach (var item in licences)
            {
                Console.WriteLine(item);
            }

            Console.ReadKey();
            Console.WriteLine("Application Finished...");
        }
    }
}
