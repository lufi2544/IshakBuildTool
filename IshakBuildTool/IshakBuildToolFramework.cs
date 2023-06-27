

// IBT
using IshakBuildTool.Build;
using IshakBuildTool.Project;
using IshakBuildTool.Test;

namespace IshakBuildTool
{
    /** Global Ishak Build Tool framework.  */
    internal class IshakBuildToolFramework
    {
            
        public IshakBuildToolFramework() 
        {

        }        

        public static void Execute(string[] args)
        {
            // Inits the Manager in charge of parsing the commandLine Args to actual args that the tool uses.
            CommandLineArgs cmdLineArgs = new CommandLineArgs(args);
            InitBuildProjectManager(cmdLineArgs);

            string buildEnviromentRootDir = GetBuildEnviromentRootDir();
            var initMessage = String.Format(
                "Starting IshakBuildTool for path: {0} ",
                buildEnviromentRootDir);

            Console.WriteLine(initMessage);
            GenerateProjectFilesHandler.GenerateProjectFiles();
        }

        static void InitBuildProjectManager(CommandLineArgs cmdArgs)
        {
            BuildProjectManager.GetInstance().Init(cmdArgs);
        }

        static string GetBuildEnviromentRootDir()
        {
            return BuildProjectManager.GetInstance().GetProjectDirectoryParams().RootDir;
        }

        public static string GetCommandLineParam(string commandLineCategory, out bool bFound)
        {
            return BuildProjectManager.GetInstance().CommandLineArgs.GetArgumentFromCategory(commandLineCategory, out bFound);
        }
    }
}