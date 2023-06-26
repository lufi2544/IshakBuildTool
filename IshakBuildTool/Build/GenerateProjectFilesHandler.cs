using IshakBuildTool.Project.Modules;

namespace IshakBuildTool.Build
{

    public class GenerateProjectFilesHandler
    {
        
        public static void GenerateProjectFiles()
        {                       
            // First we have to create the modules.
            List<IshakModule> createdModules = CreateModules();

            // Init the BuildFramework that contains the Engine Solution File
            CreateBuildContext(createdModules);

            // Create the build context( in this step the .vcxproj file is created )                        
            BuildContext ishakBuildToolBuildContext = CreateBuildContext(createdModules);

            // Creates the .sln file for the Development Enviroment
            ishakBuildToolBuildContext.CreateSolutionFile();                                                        
        }

        static List<IshakModule> CreateModules()
        {
            EntireProjectDirectoryParams dirParams = BuildProjectManager.GetInstance().GetProjectDirectoryParams();           

            ModuleManager moduleManager = new ModuleManager();
            moduleManager.DiscoverAndCreateModules(dirParams.RootDir, new ProjectFile.DirectoryReference(dirParams.IntermediateDir));

            return moduleManager.GetModules();
        }

        static BuildContext CreateBuildContext(List<IshakModule> modules)
        {
            BuildContext buildContext = new BuildContext();
            EntireProjectDirectoryParams dirParams = BuildProjectManager.GetInstance().GetProjectDirectoryParams();

            // Add Engine Project
            buildContext.AddProject("IshakEngine", dirParams.ProjectFilesDir, modules);
            
            return buildContext;
        }
        
    }
}
