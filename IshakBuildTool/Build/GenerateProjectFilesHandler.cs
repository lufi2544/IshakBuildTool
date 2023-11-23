using IshakBuildTool.Project.Modules;
using IshakBuildTool.ToolChain;

namespace IshakBuildTool.Build
{

    public class GenerateProjectFilesHandler
    {
        
        public static List<IshakModule> GenerateProjectFiles(bool bGeneratingProjectFilesMode)
        {                       
            // First we have to create the modules.
            List<IshakModule> createdModules = CreateModules();
            
            if (bGeneratingProjectFilesMode)
            {
                // Create the build context( contains the info from all the projects that form the entire solution like Engine, Games...)                      
                BuildContext ishakBuildToolBuildContext = CreateBuildContext(createdModules);

                // Creates the .sln file for the Development Enviroment
                ishakBuildToolBuildContext.CreateSolutionFile();
            }

            return createdModules;
        }

        static List<IshakModule> CreateModules()
        {
            EntireProjectDirectoryParams dirParams = BuildProjectManager.GetInstance().GetProjectDirectoryParams();           

            ModuleManager moduleManager = new ModuleManager();
            moduleManager.DiscoverAndCreateModules(dirParams.RootDir.Path, new ProjectFile.DirectoryReference(dirParams.IntermediateDir.Path));

            return moduleManager.GetModules();
        }

        static BuildContext CreateBuildContext(List<IshakModule> modules)
        {
            BuildContext buildContext = new BuildContext();
            EntireProjectDirectoryParams dirParams = BuildProjectManager.GetInstance().GetProjectDirectoryParams();

            List<IshakModule> engineModules = new List<IshakModule>();
            List<IshakModule> gameModule = new List<IshakModule>();

            foreach (var module in modules)
            {
                if (module.Name == "Game")
                {
                    gameModule.Add(module);
                    continue;
                }

                engineModules.Add(module);                
            }

            // NOTE: In visual studio the first project in the .sln will be the default project in the IDE.

            // Add Game Project
            buildContext.AddProject("Game", dirParams.ProjectFilesDir.Path, gameModule, engineModules);

            // Add the Engine Project
            buildContext.AddProject("IshakEngine", dirParams.ProjectFilesDir.Path, engineModules, engineModules);

            return buildContext;
        }
        
    }
}
