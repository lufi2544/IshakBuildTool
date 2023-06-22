using IshakBuildTool.Project.Modules;

namespace IshakBuildTool.Project
{

    // This is the class in charge of managing the entire solution project
    // If we had 2 Games in the Global Project, we would have the Engine Project and then the 2 Game Projects
    internal class BuildContext
    {

        /** List of Projects that will be in the soulution File. */
        private List<Project> Projects = new List<Project>();

        /** Generator for the SolutionFile. */               
        SolutionFileGenerator? SolutionFileGenerator { get; set; }


        public BuildContext() 
        {            

        }

        public void AddProject(string projectName, string projectPath, List<Module> modules)
        {
            Project? foundProject = Projects.Find(projectParam => projectParam.Name == projectName);
            if (foundProject == null)
            {
                Project createdProject = new Project(
                    projectName,
                    new ProjectFile(projectName, projectPath),
                    modules
                    );

                createdProject.WriteProjectFile();
                Projects.Add(createdProject);   
            }
        }

        /** Creates the .sln file for the BuildContext */
        public void CreateSolutionFile()
        {
            SolutionFileGenerator = new SolutionFileGenerator();

            // TODO TestEnviro
            SolutionFileGenerator.GenerateSolutionFile(Projects, Test.TestEnviroment.TestFolderPath,  Test.TestEnviroment.DefaultEngineName);
        }
    }
}
