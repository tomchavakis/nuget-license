using System.Collections.Generic;

namespace NugetUtility
{
    public class ValidationResult
    {
        public bool IsValid { get; set; } = false;

        public IReadOnlyCollection<KeyValuePair<string, Package>> InvalidPackages { get; set; } = new List<KeyValuePair<string, Package>>();
    }
}