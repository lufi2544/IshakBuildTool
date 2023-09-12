

using IshakBuildTool.Globals;

namespace IshakBuildTool
{
    class Tool
    {
        static void Main(string[] args)
        {
            string[] args_0 = {
            "-r", "C:\\IshakEngine",
            "-pt", "Application",
            "-bm", IshakCommandArgrType. Compile};

            string[] args_1 = {
            "-r", "C:\\IshakEngine",
            "-pt", "Application" ,"-bm", "1"};

            int i = 0;
            string[] executeArgs;

            if (i == 0)
            {
                executeArgs = args_0;
            }
            else
            {
                executeArgs = args_1;
            }

            if (args.Length == 0)
            {
                return;
            }

            IshakBuildToolFramework.Execute(args);
        }
    }
}// IshakBuildTool