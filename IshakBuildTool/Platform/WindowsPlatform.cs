using IshakBuildTool.ProjectFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

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
