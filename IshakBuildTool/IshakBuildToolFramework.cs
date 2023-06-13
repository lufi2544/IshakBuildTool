

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// IBT
using IshakBuildTool.Build;
using IshakBuildTool.Project;
using IshakBuildTool.Test;

namespace IshakBuildTool
{
    /** Global Ishak Build Tool framework  */
    internal class IshakBuildToolFramework
    {
            
        public IshakBuildToolFramework() 
        {

        }

        public static void Execute(string[] commandLineArgs)
        {
            var testArgs = TestEnviroment.TestFolderPath;
            var initMessage = String.Format(
                "Starting IshakBuildTool for path: {0} ",
                testArgs);

            Console.WriteLine(initMessage);
            GenerateProjectFilesHandler.GenerateProjectFiles(commandLineArgs);
        }
    }
}
