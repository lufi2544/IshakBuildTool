

using IshakBuildTool.Test;
using System.Text;

namespace IshakBuildTool
{
    class Tool
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new string[]{
                    "-r", TestEnviroment.TestCommandLineArgs,
                    "-pt", TestEnviroment.TestProjectType};            
            }

            IshakBuildToolFramework.Execute(args);
        }
    }
}// IshakBuildTool
