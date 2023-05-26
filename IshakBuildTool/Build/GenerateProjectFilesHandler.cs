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
            // For now we just use the Visual studio project file generator
            VSProjectGenerator tGenerator = new VSProjectGenerator();

            // Create the .vcxproj for the engine and adds its source files to it.
            tGenerator.CreateEngineSolutionFile(CommandLineArguments);

            
            // Crate the Solution Project Hirarchy( IshakEngine, Engine... )
            // When we have games, I would like to add the game as a RootFolder Name like ( GameName- EngineFiles- GameFiles  )
            
            

        }

        /** Creates the Projects for visual studio, in this case the .vcxproj for the Engine. */
        private static void CreateProjects(VSProjectGenerator ProjectGenerator)
        {
            
        }
    }
}
