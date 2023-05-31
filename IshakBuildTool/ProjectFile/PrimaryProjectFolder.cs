using IshakBuildTool.ProjecFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.ProjectFile
{
    /** Class for describing a Folder in the game. */
    internal class ProjectDirectory
    {
        public ProjectDirectory(string folderNameParam, string directoryParam)
        {
            DirectoryName = folderNameParam;
            DirectoryPath= directoryParam;
            subSolutionFiles = new List<SolutionFile>();
        }

        public ProjectDirectory(string directoryPath)
        {            
            DirectoryPath = directoryPath;
            DirectoryName = Path.GetFileName(directoryPath);                        
            subSolutionFiles = new List<SolutionFile>();
        }

        public string DirectoryPath { get; set; }

        public string? DirectoryName { get; set; }
                
        List<string> SubFolders { get; set;}

        public List<SolutionFile> subSolutionFiles { get; set; }

        /** Files inside the Folder.  ex: .h, .cpp, .cs*/
        List<string> Files { get;}
    }
}
