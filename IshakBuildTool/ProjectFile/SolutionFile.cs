using IshakBuildTool.ProjectFile;
using IshakBuildTool.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.ProjecFile
{
    /*This is an object that wraps up a .vcxproj file.  */
    internal class SolutionFile
    {
        public SolutionFile(string soutionNameParam, string pathParam) 
        {
            projectName = soutionNameParam;
            solutionProjectName = soutionNameParam + ".vcxproj";
            path = pathParam + solutionProjectName;

            // We fill the source files here directly


        }

        void CreateSolutionFiles()
        {
            
        }      
      
       public string solutionProjectName { get; set; }
       public string projectName { get; set; }             
       public string path { get; set; }

       public List<FileReference> sourceFiles { get; set; }
       public Guid GUID { get; set; }
    }
}
