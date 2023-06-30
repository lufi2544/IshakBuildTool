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

        //---- Singleton ----
        private WindowsPlatform? singleton = null;
        //----  -----

        WindowsPlatform GetInstance()
        {
            if (singleton == null)
            {
                singleton = new WindowsPlatform();
            }

            return singleton;
        }


        WindowsSDK WindowsSDK { get; set; }

        public WindowsPlatform()
        {
            WindowsSDK = new WindowsSDK();
        }


    }
}
