using IshakBuildTool.Project;
using IshakBuildTool.Project.Modules;

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

            // Create the build context( in this step the .vcxproj file is created )                        
            BuildContext ishakBuildToolBuildContext = CreateBuildContext(createdModules);

            // Creates the .sln file for the Development Enviroment
            ishakBuildToolBuildContext.CreateSolutionFile();                                                        
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
