using IshakBuildTool.Project.Modules;

namespace IshakBuildTool.Build
{

    public class GenerateProjectFilesHandler
    {
        
        public static void GenerateProjectFiles()
        {                       
            // First we have to create the modules.
            List<IshakModule> createdModules = CreateModules();
            
            // Create the build context( contains the info from all the projects that form the entire solution like Engine, Games...)                      
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
