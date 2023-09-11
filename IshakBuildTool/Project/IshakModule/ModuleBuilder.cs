
namespace IshakBuildTool.Project.Modules
{
    /** This is the actual builder for a Module that HAS to exists in the Module Directory. */
    public abstract class ModuleBuilder
    {
        public static string ModuleBuilderPrefix = "ModuleBuilder";

        /** Variables that have to be defined in the constructor of the Builder. */
        public List<string> PublicModuleDependencies = new List<string>();
        public List<string> PrivateModuleDependencies = new List<string>();

        public string ThirdPartyDLLName = string.Empty;
        public string ThirdPartyDLLImportName = string.Empty;
    }
}

