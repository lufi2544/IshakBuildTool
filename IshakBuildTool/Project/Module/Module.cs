using IshakBuildTool.ProjectFile;
using IshakBuildTool.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.Project.Module
{
    /** Engine Module */
    public class Module
    {
        public Module(DirectoryReference ModuleDirParam)
        {
            ModuleDir = ModuleDirParam;


        }

        void MakePublicDirs()
        {

        }

        void MakePrivateDirs()
        {

        }

        DirectoryReference ModuleDir { get; set; }
        public List<string> PublicDirectoryRefs { get; set; }
        public List<string> PrivateDirectoryRefs { get; set; }

        public List<string> DependentModules;         
    }
}
