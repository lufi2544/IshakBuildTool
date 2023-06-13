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
        public string SolutionProjectName { get; set; }
        public string ProjectName { get; set; }
        public string Path { get; set; }

        string ProjectFileContent = string.Empty;

        /** This is the actual content of the file, used by Visual Studio for the IDE. */
        public string? projectFileContent;

        Project? OwnerProject;

        public ProjectFile(string soutionNameParam, string pathParam)
        {
            ProjectName = soutionNameParam;
            SolutionProjectName = soutionNameParam + ".vcxproj";
            Path = pathParam + SolutionProjectName;            
        }

        public void SetOwnerProject(Project project)
        {
            OwnerProject= project;
        }
        
        public void SetProjectFileContent(string content)
        {
            ProjectFileContent = content;            
        }        

        public void Create()
        {
            // We will not create anything if the content is not filled.
            if (ProjectFileContent == string.Empty)
            {
                return;
            }
            else
            {
                DirectoryUtils.CreateDirectoryWithContent(Path, ProjectFileContent);
            }
        }
    }
}
