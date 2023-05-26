using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.ProjecFile
{
    public class ProjectFile
    {
        
        ProjectFile(string ProjectNameParam)                         
        {
            ProjectName = ProjectNameParam;
        }        

        Guid GetGuid()
        {
            return ProjectGuid;
        }

        public void SetProjectGuid(Guid GuidParam)
        {
            ProjectGuid = GuidParam;
        }

    
        public string? ProjectName;        

        // This is the id that visual studio uses for cpp
        // for now let's keep it simple and add only the cpp.
        public readonly string ProjectTypeId = "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}";

        Guid ProjectGuid { get; set; }
    }
}
