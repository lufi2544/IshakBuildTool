using IshakBuildTool.ProjectFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.Project
{
    /** This class is in charge of generating the Project Filter file, so when opening the project in Visual Studio
     *  we will have all the folder ordered and filtered by folders.
     * */
    internal class ProjectFileFilterGenerator
    {

        /** String Builder that will be the content for the Filter File when written. */
        StringBuilder ProjectFilterFileSB = new StringBuilder();

        /** Final filters for the project, representing the folders. */
        List<string> FolderFilters= new List<string>();
                              
        
        void Init() 
        {
            ProjectFilterFileSB.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            ProjectFilterFileSB.AppendLine("<Project ToolsVersion=\"17.0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");
            ProjectFilterFileSB.AppendLine("  <ItemGroup>");
        }

        
        void Finish()
        {
            ProjectFilterFileSB.AppendLine("  </ItemGroup>");
            ProjectFilterFileSB.AppendLine("</Project>");
        }

        
        void AddFile(FileReference fileRef)
        {
            if (FolderFilters.Contains(fileRef.Directory.Path))
            {
                // Filter already exists.


            }
            else
            {
                // Create new filter.
            }
        }
    }
}
