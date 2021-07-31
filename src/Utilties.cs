using System;
using System.Collections.Generic;
using System.IO;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace NugetUtility {
    public static class Utilties {
        public static bool IsLicenseFile (this License license) =>
            string.Compare (license?.Type, "file", StringComparison.OrdinalIgnoreCase) == 0;

        public static string EnsureCorrectPathCharacter (this string path) =>
            path?.Replace ("\\", "/").Trim ();

        public static ICollection<T> ReadListFromFile<T> (string jsonFileList) {
            if (string.IsNullOrWhiteSpace (jsonFileList)) {
                return Array.Empty<T> ();
            }

            return JsonConvert.DeserializeObject<List<T>> (EnsureFileExistsAndRead (jsonFileList));
        }

        public static Dictionary<T1, T2> ReadDictionaryFromFile<T1, T2> (string jsonFileList, Dictionary<T1, T2> defaultValue = default) {
            if (string.IsNullOrWhiteSpace (jsonFileList)) {
                return defaultValue;
            }

            return JsonConvert.DeserializeObject<Dictionary<T1, T2>> (EnsureFileExistsAndRead (jsonFileList)) ??
                defaultValue;
        }

        public static string EnsureFileExistsAndRead (string jsonFileList) {
            var file = new FileInfo (jsonFileList);

            if (!file.Exists) {
                throw new FileNotFoundException (file.FullName);
            }

            return File.ReadAllText (file.FullName);
        }

        /// <summary>
        /// ExtractTextFromHTML function converts the HTML licence to txt
        /// </summary>
        /// <param name="html">html</param>
        /// <returns>licence text from HTML</returns>
        public static string ExtractTextFromHTML (string html) {
            if (html == null) {
                throw new ArgumentNullException ();
            }

            HtmlDocument doc = new HtmlDocument ();
            doc.LoadHtml (html);

            var chunks = new List<string> ();
            foreach (var item in doc.DocumentNode.DescendantsAndSelf ()) {
                if (item.NodeType == HtmlNodeType.Text) {
                    if (item.InnerText.Trim () != "") {
                        chunks.Add (item.InnerText.Trim ());
                    }
                }
            }
            return String.Join (" ", chunks);
        }
    }
}