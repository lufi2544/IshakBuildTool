using IshakBuildTool.ProjectFile;
using IshakBuildTool.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.Project
{
    /*This is an object that wraps up a .vcxproj file.  */
    internal class ProjectFile
    {
        public ProjectFile(string soutionNameParam, string pathParam)
        {
            projectName = soutionNameParam;
            solutionProjectName = soutionNameParam + ".vcxproj";
            path = pathParam + solutionProjectName;            
        }

        public void SetOwnerProject(Project project)
        {
            OwnerProject= project;
        }

        public string solutionProjectName { get; set; }
        public string projectName { get; set; }
        public string path { get; set; }

        /** This is the actual content of the file, used by Visual Studio for the IDE. */
        public string projectFileContent;
        
        Project OwnerProject;
    }
}
