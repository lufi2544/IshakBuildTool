

using IshakBuildTool.Test;
using System.Text;

namespace IshakBuildTool
{
    class Tool
    {
        static void Main(string[] args)
        {
            string[] lArgs = new string[]{ "-r", Test.TestEnviroment.TestCommandLineArgs };            
            IshakBuildToolFramework.Execute(lArgs);
        }
    }
}// IshakBuildTool
