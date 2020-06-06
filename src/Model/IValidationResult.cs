using System.Collections.Generic;

namespace NugetUtility
{
    public interface IValidationResult<T>
    {
        bool IsValid { get; }
        IReadOnlyCollection<T> InvalidPackages { get; }
    }
}