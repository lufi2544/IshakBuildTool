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
    internal class IshakProjectFile
    {
        public string SolutionProjectName { get; set; }

        public string ProjectName { get; set; }

        public string Path { get; set; }

        /** This is the actual content of the file, used by Visual Studio for the IDE. */
        string ProjectFileContent = string.Empty;

        IshakProject? OwnerProject = null;

        public IshakProjectFile(string soutionNameParam, string pathParam)
        {
            ProjectName = soutionNameParam;
            SolutionProjectName = soutionNameParam + ".vcxproj";
            Path = pathParam + SolutionProjectName;
        }

        public FileReference GetProjectFileRef()
        {
            return new FileReference(Path);
        }

        public void SetOwnerProject(IshakProject project)
        {
            OwnerProject = project;
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
                // TODO Exception 
                return;
            }
            else
            {
                DirectoryUtils.CreateFileWithContent(Path, ProjectFileContent);
            }
        }
    }
}
