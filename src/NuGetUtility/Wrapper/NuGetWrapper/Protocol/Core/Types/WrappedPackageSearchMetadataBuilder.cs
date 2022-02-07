using NuGet.Protocol.Core.Types;

namespace NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types
{
    internal class WrappedPackageSearchMetadataBuilder : IPackageSearchMetadataBuilder
    {
        private readonly PackageSearchMetadataBuilder _builder;

        public WrappedPackageSearchMetadataBuilder(PackageSearchMetadataBuilder builder)
        {
            _builder = builder;
        }

        public IPackageSearchMetadata Build()
        {
            return _builder.Build();
        }
    }
}
