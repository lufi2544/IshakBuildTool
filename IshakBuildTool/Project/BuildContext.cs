using IshakBuildTool.Project.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.Project
{

    // This is the class in charge of managing the entire solution project
    // If we had 2 Games in the Global Project, we would have the Engine Project and then the 2 Game Projects
    internal class BuildContext
    {
        public BuildContext() 
        {
            Projects= new List<Project>();
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
        

        /** List of Projects that will be in the soulution File. */
        public List<Project> Projects { get; set; }

        /** This will wrap up all the projects and is a wrapper for the .sln file.  */
        public SolutionFile? SolutionFile { get; set; }
    }
}
