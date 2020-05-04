using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace NugetUtility
{
    public static class Utilties
    {
        public static bool IsLicenseFile(this License license) =>
            string.Compare(license?.Type, "file", StringComparison.OrdinalIgnoreCase) == 0;

        public static ICollection<T> ReadListFromFile<T>(string jsonFileList)
        {
            if (string.IsNullOrWhiteSpace(jsonFileList))
            {
                return Array.Empty<T>();
            }

            return JsonConvert.DeserializeObject<List<T>>(EnsureFileExistsAndRead(jsonFileList));
        }

        public static Dictionary<string, string> ReadDictionaryFromFile(string jsonFileList)
        {
            if (string.IsNullOrWhiteSpace(jsonFileList))
            {
                return LicenseToUrlMappings.Default;
            }

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(EnsureFileExistsAndRead(jsonFileList))
                ?? LicenseToUrlMappings.Default;
        }

        public static string EnsureFileExistsAndRead(string jsonFileList)
        {
            var file = new FileInfo(jsonFileList);

            if (!file.Exists)
            {
                throw new FileNotFoundException(file.FullName);
            }

            return File.ReadAllText(file.FullName);
        }
    }
}