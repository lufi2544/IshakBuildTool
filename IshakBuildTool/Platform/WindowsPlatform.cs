using IshakBuildTool.ProjectFile;
using System.Runtime.Versioning;

namespace IshakBuildTool.Platform
{
    /** Representation of the Windows itself. */

    [SupportedOSPlatform("windows")]
    internal class WindowsPlatform
    {
        WindowsSDK WindowsSDK { get; set; }

        public WindowsPlatform()
        {
            WindowsSDK = new WindowsSDK();
        }


        public List<DirectoryReference> GetWindowsSDKIncludeDirs()
        {
            return WindowsSDK.IncludeDirectories;
        }


    }
}
