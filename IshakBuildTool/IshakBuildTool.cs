

using IshakBuildTool.Test;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

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
