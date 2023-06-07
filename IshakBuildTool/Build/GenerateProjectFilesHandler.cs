using IshakBuildTool.Project.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.Build
{
    public class GenerateProjectFilesHandler
    {

     
        public static void GenerateProjectFiles(string CommandLineArguments)
        {
            
            // First we have to create the modules.
            CreateModules();


            //Create the Project Files and the Create the Solution files for them.

            // For now we just use the Visual studio project file generator
            //VSProjectGenerator tGenerator = new VSProjectGenerator();



            // Create the .vcxproj for the engine and adds its source files to it.
            //tGenerator.CreateEngineSolutionFile(CommandLineArguments);

            
            // Crate the Solution Project Hirarchy( IshakEngine, Engine... )
            // When we have games, I would like to add the game as a RootFolder Name like ( GameName- EngineFiles- GameFiles  )
            
            

        }

        /** Creates the Projects for visual studio, in this case the .vcxproj for the Engine. */
        private static void CreateProjects(ProjectGenerator ProjectGenerator)
        {
            
        }

        private static void CreateModules()
        {
            string engineIntermediateDir = Test.TestEnviroment.TestIntermediateFolder;
            string engineRootPath = Test.TestEnviroment.TestFolderPath;
            ModuleManager moduleManager = new ModuleManager();
            moduleManager.DiscoverAndCreateModules(engineRootPath, new ProjectFile.DirectoryReference(engineIntermediateDir));
        }
    }
}
