using System.Collections.Generic;

namespace NugetUtility
{
    public class ValidationResult<T> : IValidationResult<T>
    {
        public bool IsValid { get; set; } = false;

        public IReadOnlyCollection<T> InvalidPackages { get; set; } = new List<T>();
    }
}