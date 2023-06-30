

using IshakBuildTool.Test;

namespace IshakBuildTool
{
    class Tool
    {
        static void Main(string[] args)
        {
            string[] argss = {
            "-r", "C:\\IshakEngine",
            "-pt", "Application"};             
            IshakBuildToolFramework.Execute(argss);
        }
    }
}// IshakBuildTool
