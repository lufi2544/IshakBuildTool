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


    /** Class in charge of creating the Modules.  */
    internal class ModuleManager
    {
        private Dictionary<string, Module>? ModulesDictionary = new Dictionary<string, Module>();
        private ModuleAssemblyManager? ModulesAssemblyManager;        


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

            CreateAssemblyManager(engineIntermediatePath, foundModules);

            // TODO Function creates the modules.
            if (foundModules.Count > 0)
            {
                 foreach (FileReference moduleFileRef in foundModules)
                 {
                    TryCreateModule(moduleFileRef);
                 }
            }
            else
            {
                // No Modules Found, maybe a problem here.
            }            

        }

        void TryCreateModule(FileReference moduleFileRef)
        {
            Module? specificModule;            
            if (!ModulesDictionary.TryGetValue(moduleFileRef.Name, out specificModule))
            {
                ModuleBuilder? moduleBuilder =
                    GetModuleBuilderFromModuleName(moduleFileRef.Name);

                Module createdModule = new Module(moduleBuilder, moduleFileRef);                                
                ModulesDictionary.Add(moduleFileRef.Name, createdModule);
            }
        }

        private void CreateAssemblyManager(DirectoryReference engineIntemediateDirectory, List<FileReference> modulesFilesRefs)
        {
            ModulesAssemblyManager = new ModuleAssemblyManager(engineIntemediateDirectory, modulesFilesRefs);                      
        }

        private ModuleBuilder? GetModuleBuilderFromModuleName(string moduleName)
        {
            return ModulesAssemblyManager.GetModuleBuilderByName(moduleName);
        }
        

    }
}
