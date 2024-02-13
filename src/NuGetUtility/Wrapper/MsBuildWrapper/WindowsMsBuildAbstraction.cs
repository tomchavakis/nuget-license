using System.Management;
using System.Runtime.Versioning;

namespace NuGetUtility.Wrapper.MsBuildWrapper
{
    [SupportedOSPlatform("windows")]
    public class WindowsMsBuildAbstraction : MsBuildAbstraction
    {
        public WindowsMsBuildAbstraction()
        {
            // to support VC-projects we need to workaround : https://github.com/3F/MvsSln/issues/1
            // adding 'VCTargetsPath' to Project::GlobalProperties seem to be enough

            if (GetBestVCTargetsPath() is string path)
            {
                AddGlobalProjectProperty("VCTargetsPath", $"{path}\\");
            }
        }

        private static string? GetBestVCTargetsPath()
        {
            var cppProperties = new List<FileInfo>();

            foreach (string path in GetVisualStudioInstallPaths())
                cppProperties.AddRange(new DirectoryInfo(path).GetFiles("Microsoft.Cpp.Default.props", SearchOption.AllDirectories));

            // if multiple, assume most recent 'LastWriteTime' property is 'best'
            return cppProperties.OrderBy(f => f.LastWriteTime).LastOrDefault()?.DirectoryName;
        }

        private static IEnumerable<string> GetVisualStudioInstallPaths()
        {
            // https://learn.microsoft.com/en-us/visualstudio/install/tools-for-managing-visual-studio-instances?view=vs-2022#using-windows-management-instrumentation-wmi

            var result = new List<string>();

            try
            {
                var mmc = new ManagementClass("root/cimv2/vs:MSFT_VSInstance");

                foreach (ManagementBaseObject? vs_instance in mmc.GetInstances())
                    if (vs_instance["InstallLocation"] is string install_path)
                        result.Add(install_path);
            }
            catch (ManagementException me) when (me.Message.Contains("Invalid namespace"))
            {
                // Visual Studio might not be installed
            }

            return result;
        }
    }
}
