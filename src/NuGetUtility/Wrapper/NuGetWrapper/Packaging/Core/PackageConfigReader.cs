using System.Xml.Linq;
using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

namespace NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core
{
    public static class PackagesConfigReader
    {
        public static IEnumerable<PackageIdentity> GetPackages(string path)
        {
            var document = XDocument.Load(path);

            var reader = new NuGet.Packaging.PackagesConfigReader(document);

            return reader.GetPackages().Select(p => new PackageIdentity(p.PackageIdentity.Id, new WrappedNuGetVersion(p.PackageIdentity.Version)));
        }
    }
}
