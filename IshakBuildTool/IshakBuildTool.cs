using IshakBuildTool.Test;

using IshakBuildTool.Build;
using IshakBuildTool.Utils;
using IshakBuildTool.ProjectFile;

namespace IshakBuildTool
{
    class Tool
    {
        static void Main(string[] args)
        {
            var testArgs = TestEnviroment.TestFolderPath;            
            var initMessage = String.Format("Starting IshakBuildTool for path: {0} ", testArgs);
            Console.WriteLine(initMessage);

            GenerateProjectFilesHandler.GenerateProjectFiles(testArgs);

            string path = DirectoryUtils.MakeRelativeTo(new DirectoryReference("C/"), new DirectoryReference("A/"));

            Console.WriteLine(path);
        }

    }


}// IshakBuildTool
