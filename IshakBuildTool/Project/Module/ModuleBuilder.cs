
namespace IshakBuildTool.Project.Module
{
    /** This is the actual builder for a Module that HAS to exists in the Module Directory. */
    public abstract class ModuleBuilder
    {

        /** Variables that have to be defined in the constructor of the Builder. */
        public List<string> PublicModuleDependencies = new List<string>();
        public List<string> PrivateModuleDependencies = new List<string>();
    }
}

