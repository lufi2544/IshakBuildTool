
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IshakBuildTool.Project;

namespace IshakBuildTool.ProjectFile
{
    /** Class for describing a Folder in the game. */
    internal class ProjectDirectory
    {
        public ProjectDirectory(string folderNameParam, string directoryParam)
        {
            DirectoryName = folderNameParam;
            DirectoryPath= directoryParam;
            subSolutionFiles = new List<Project.ProjectFile>();
        }

        public ProjectDirectory(string directoryPath)
        {            
            DirectoryPath = directoryPath;
            DirectoryName = Path.GetFileName(directoryPath);                        
            subSolutionFiles = new List<Project.ProjectFile>();
        }

        public string DirectoryPath { get; set; }

        public string? DirectoryName { get; set; }
                
        List<string> SubFolders { get; set;}

        public List<Project.ProjectFile> subSolutionFiles { get; set; }

        /** Files inside the Folder.  ex: .h, .cpp, .cs*/
        List<string> Files { get;}
    }
}
