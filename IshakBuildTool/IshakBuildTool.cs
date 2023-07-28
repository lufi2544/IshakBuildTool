

using IshakBuildTool.Test;

namespace IshakBuildTool
{
    class Tool
    {
        static void Main(string[] args)
        {
            string[] argss = {
            "-r", "C:\\IshakEngine",
            "-pt", "Application" };//, "-bm", "1"};             
            IshakBuildToolFramework.Execute(argss);
        }
    }
}// IshakBuildTool
