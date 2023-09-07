

// IBT
using IshakBuildTool.Build;
using IshakBuildTool.Globals;
using IshakBuildTool.Project.Modules;
using IshakBuildTool.ProjectFile;
using IshakBuildTool.ToolChain;

namespace IshakBuildTool
{
    /** Global Ishak Build Tool framework.  */
    internal class IshakBuildToolFramework
    {

        public static IshakToolChain? ToolChain { get; set; } = null;

        public IshakBuildToolFramework() 
        {

        }        

        public static void Execute(string[] args)
        {
            // Inits the Manager in charge of parsing the commandLine Args to actual args that the tool uses.
            CommandLineArgs cmdLineArgs = new CommandLineArgs(args);
            InitBuildProjectManager(cmdLineArgs);
            CreateToolChain();


            // TODO Add an argument for compiling
            
            string buildEnviromentRootDir = GetBuildEnviromentRootDir();
            var initMessage = String.Format(
                "Starting IshakBuildTool for path: {0} ",
                buildEnviromentRootDir);

            Console.WriteLine(initMessage);

            // TODO change the return.

            bool bFoundGenerationArgument;
            string generationArg = IshakBuildToolFramework.GetCommandLineParam("-bm", out bFoundGenerationArgument);            
            bool bAreWeGeneratingProjectFiles = generationArg == IshakCommandArgrType.GenerateProjectFiles;
            List<IshakModule> modules = GenerateProjectFilesHandler.GenerateProjectFiles(bAreWeGeneratingProjectFiles);


            // TODO FUNCTION

            bool bFoundCompilationArgument;
            string compilationArg = GetCommandLineParam("-bm", out bFoundCompilationArgument);             
            bool bAreWeCompiling = compilationArg == IshakCommandArgrType.Compile;

            if (bAreWeCompiling)
            {
                Console.WriteLine();
                Console.WriteLine("----  Compilation Started  -----");
                Console.WriteLine();

                // TODO Add to a single Function BuildMo
                // Build all the modules => Compiling / Linking
                ToolChain.CompileAndLinkModules(modules).GetAwaiter().GetResult();

                // For now
                Console.WriteLine();
                Console.WriteLine("----  Compilation Finished  ----");
                Console.WriteLine();

                // Create the Exe
                ToolChain.CreateExecutable(modules).GetAwaiter().GetResult();
            }
        }

        static void CreateToolChain()
        {
            ToolChain = new IshakToolChain();
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