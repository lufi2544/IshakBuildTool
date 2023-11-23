using IshakBuildTool.Project;
using IshakBuildTool.Project.Modules;

namespace IshakBuildTool.Build
{

    // This is the class in charge of managing the entire solution project
    // If we had 2 Games in the Global Project, we would have the Engine Project and then the 2 Game Projects
    internal class BuildContext
    {

        /** List of Projects that will be in the soulution File. */
        private List<IshakProject> Projects = new List<IshakProject>();

        /** Generator for the SolutionFile. */
        SolutionFileGenerator? SolutionFileGenerator { get; set; }


        public BuildContext()
        {

        }

        public void AddProject(string projectName, string projectPath, List<IshakModule> modules, List<IshakModule> dependencyModules)
        {
            IshakProject? foundProject = Projects.Find(projectParam => projectParam.Name == projectName);
            if (foundProject == null)
            {
                IshakProject createdProject = new IshakProject(
                    projectName,
                    new IshakProjectFile(projectName, projectPath),
                    modules, 
                    dependencyModules);

                createdProject.WriteProjectFile();
                Projects.Add(createdProject);
            }
        }

        /** Creates the .sln file for the BuildContext */
        public void CreateSolutionFile()
        {
            SolutionFileGenerator = new SolutionFileGenerator();
            SolutionFileGenerator.GenerateSolutionFile(
                Projects,
                BuildProjectManager.GetInstance().GetProjectDirectoryParams().RootDir.Path,
                Test.TestEnviroment.DefaultEngineName);

            Console.WriteLine("---- GENERATION COMPLETED ---");
        }
    }
}
