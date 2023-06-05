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
        public Module(ModuleBuilder moduleBuilder,  FileReference moduleFileRef)
        {
            moduleFile = moduleFileRef;

            PublicDirectoryRefs = moduleBuilder.PublicModuleDependencies;
            PrivateDirectoryRefs= moduleBuilder.PrivateModuleDependencies;
        }

        void MakePublicDirs()
        {

        }

        void MakePrivateDirs()
        {

        }

        public FileReference moduleFile { get; set; }
        public List<string> PublicDirectoryRefs { get; set; }
        public List<string> PrivateDirectoryRefs { get; set; }

        public List<string> DependentModules;         
    }
}
