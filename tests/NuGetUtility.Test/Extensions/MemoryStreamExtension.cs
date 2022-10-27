﻿namespace NuGetUtility.Test.Extensions
{
    public static class MemoryStreamExtension
    {
        public static string AsString(this MemoryStream stream)
        {
            stream.Position = 0;
            using var reader = new StreamReader(stream, leaveOpen: true, encoding: System.Text.Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}
