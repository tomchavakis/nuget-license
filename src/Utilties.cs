using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace NugetUtility
{
    public static class Utilties
    {
        public static bool IsLicenseFile(this License license) =>
            string.Compare(license?.Type, "file", StringComparison.OrdinalIgnoreCase) == 0;

        public static string EnsureCorrectPathCharacter(this string path) =>
            path?.Replace("\\", "/").Trim();

        public static ICollection<T> ReadListFromFile<T>(string jsonFileList)
        {
            if (string.IsNullOrWhiteSpace(jsonFileList))
            {
                return Array.Empty<T>();
            }

            return JsonConvert.DeserializeObject<List<T>>(EnsureFileExistsAndRead(jsonFileList));
        }

        public static Dictionary<T1, T2> ReadDictionaryFromFile<T1, T2>(string jsonFileList, Dictionary<T1, T2> defaultValue = default)
        {
            if (string.IsNullOrWhiteSpace(jsonFileList))
            {
                return defaultValue;
            }

            return JsonConvert.DeserializeObject<Dictionary<T1, T2>>(EnsureFileExistsAndRead(jsonFileList))
                .Concat(defaultValue)
                .GroupBy(kv => kv.Key)
                .ToDictionary(g => g.Key, g => g.First().Value)
                ?? defaultValue;
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