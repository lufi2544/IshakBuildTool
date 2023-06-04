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
        
        public ModuleManager() 
        {            
        }

        /** Discovers and Creates all the modules. */
        public void DiscoverAndCreateModules(string engineRootDirPath, DirectoryReference engineIntermediatePath)
        {           
            List<string> moduleFilter = new List<string>();
            moduleFilter.Add("Module.cs");

            List<FileReference> foundModules = 
                FileScanner.FindFilesInDirectoryWithFilter(
                    engineRootDirPath, 
                    null,
                    null,
                    EFileScannerFilterMode.EInclusive,
                    moduleFilter);

            TryCreateAssemblyManager(engineIntermediatePath, foundModules);


            // TODO Function creates the modules.
            if (foundModules.Count > 0)
            {
                 foreach (FileReference moduleFile in foundModules)
                 {
                    ModuleBuilder? moduleBuilder = GetModuleBuilderFromModuleFile(moduleFile.Path);
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

        private void TryCreateAssemblyManager(DirectoryReference engineIntemediateDirectory, List<FileReference> modulesFilesRefs)
        {
            if (ModulesAssemblyManager == null)
            {
                ModulesAssemblyManager = new ModuleAssemblyManager(engineIntemediateDirectory, modulesFilesRefs);
            }            
        }

        private ModuleBuilder? GetModuleBuilderFromModuleFile(string modulePath)
        {

            return null;
        }

        private Module? TryCreateModule(string moduleName)
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
                
                return null;                            
            }            
        }

        
        private Dictionary<string, ModuleCreationData>? ModulesCrationDatas;
        private ModuleAssemblyManager? ModulesAssemblyManager;
    }
}
