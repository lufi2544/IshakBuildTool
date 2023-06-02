using IshakBuildTool.ProjectFile;
using IshakBuildTool.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.Project.Module
{

    /** Data referencing a stack of data related to a module creation. */
    public struct ModuleCreationData
    {
        public ModuleCreationData() 
        {
            
        }
        
        public ModuleBuilder? Builder = null;
        public Module? ModuleObject = null;
    }

    /** Class in charge of creating the Modules.  */
    internal class ModuleManager
    {

        ModuleManager() 
        { 

        }

        public void DiscoverModules(string startingPath)
        {
            List<string> moduleFilter = new List<string>();
            moduleFilter.Add("Module.cs");

            List<FileReference> foundModules = 
                FileScanner.FindFilesInDirectoryWithFilter(
                    startingPath, 
                    null,
                    null,
                    EFileScannerFilterMode.EInclusive,
                    moduleFilter);

            if (foundModules.Count > 0)
            {
                 foreach (FileReference moduleFile in foundModules)
                 {
                    ModuleBuilder? moduleBuilder = GetModuleBuilderFromModuleFile(moduleFile.path);
                    if (moduleBuilder != null)
                    {

                    }

                 }
            }
            else
            {
                // No Modules Found, maybe a problem here.
            }


        }

        private ModuleBuilder? GetModuleBuilderFromModuleFile(string modulePath)
        {

            return null;
        }

        private Module TryCreateModule(string moduleName)
        {
            if (ModulesCrationDatas == null)
            {
                ModulesCrationDatas = new Dictionary<string, ModuleCreationData>();
            }

            ModuleCreationData moduleCreationData;
            if (ModulesCrationDatas.TryGetValue(moduleName, out moduleCreationData))
            {
                // Found the module
                return moduleCreationData.ModuleObject;

            }
            else
            {
                // Module not found, so we built it.
                moduleCreationData = new ModuleCreationData();

                                
            }
            
        }

        
        private Dictionary<string, ModuleCreationData>? ModulesCrationDatas;
        private Assembly? modulesAssembly;
    }
}
