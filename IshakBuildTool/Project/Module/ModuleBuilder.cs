using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.Project.Module
{
    /** This is the actual builder for a Module that HAS to exists in the Module Directory. */
    public abstract class ModuleBuilder
    {
        


        /** Variables that have to be defined in the constructor of the Builder. */
        public string[] PublicModuleDependencies { get; set; }
        public string[] PrivateModuleDependencies { get; set;}
    }
}
