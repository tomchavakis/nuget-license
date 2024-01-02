using System.Xml.Linq;
using NuGetUtility.Extensions;
using NuGetUtility.Wrapper.MsBuildWrapper;
using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

namespace NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core
{
    public class WindowsPackagesConfigReader : IPackagesConfigReader
    {
        public IEnumerable<PackageIdentity> GetPackages(IProject project)
        {
            var document = XDocument.Load(project.GetPackagesConfigPath());

            var reader = new NuGet.Packaging.PackagesConfigReader(document);

            return reader.GetPackages().Select(p => new PackageIdentity(p.PackageIdentity.Id, new WrappedNuGetVersion(p.PackageIdentity.Version)));
        }
    }
}
