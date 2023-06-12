using IshakBuildTool.Project;
using IshakBuildTool.Project.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.Build
{
    public class GenerateProjectFilesHandler
    {
        
        public static void GenerateProjectFiles(string[] args)
        {
            
            // First we have to create the modules.
            List<Module> createdModules = CreateModules();

            // Init the BuildFramework that contains the Engine Solution File
            CreateBuildContext(createdModules);

            // Create the build context                        
            BuildContext ishakBuildToolBuildContext = CreateBuildContext(createdModules);

            // 


            // Create the .vcxproj for the engine and adds its source files to it.
            //tGenerator.CreateEngineSolutionFile(CommandLineArguments);


            // Crate the Solution Project Hirarchy( IshakEngine, Engine... )
            // When we have games, I would like to add the game as a RootFolder Name like ( GameName- EngineFiles- GameFiles  )



    }

        /** Creates the Projects for visual studio, in this case the .vcxproj for the Engine. */
        private static void CreateProjects(ProjectFileGenerator ProjectGenerator)
        {
            
        }

        private static List<Module> CreateModules()
        {
            string engineIntermediateDir = Test.TestEnviroment.TestIntermediateFolder;
            string engineRootPath = Test.TestEnviroment.TestFolderPath;

            ModuleManager moduleManager = new ModuleManager();
            moduleManager.DiscoverAndCreateModules(engineRootPath, new ProjectFile.DirectoryReference(engineIntermediateDir));

            return moduleManager.GetModules();
        }

        private static BuildContext CreateBuildContext(List<Module> modules)
        {
            BuildContext buildContext = new BuildContext();

            // Add Engine Project
            buildContext.AddProject("IshakEngine", Test.TestEnviroment.TestProjectFilesFolder, modules);

            

            return buildContext;
        }
        
    }
}
