// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using System.Text;

namespace NuGetUtility.Test.Extensions
{
    public static class MemoryStreamExtension
    {
        public static string AsString(this MemoryStream stream)
        {
            stream.Position = 0;
            using var reader = new StreamReader(stream, encoding: new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true), bufferSize: 1024, leaveOpen: true, detectEncodingFromByteOrderMarks: false);
            return reader.ReadToEnd();
        }
    }
}
