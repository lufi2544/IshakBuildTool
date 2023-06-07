using IshakBuildTool.ProjectFile;
using IshakBuildTool.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.Project.Modules
{


    /** Class in charge of creating the Modules.  */
    public class ModuleManager
    {
        private Dictionary<string, Module>? ModulesDictionary = new Dictionary<string, Module>();
        private Dictionary<string, FileReference> ModulesFileRefsDictionary = new Dictionary<string, FileReference>();
        private ModuleAssemblyManager? ModulesAssemblyManager;        


        public ModuleManager() 
        {            
        }


        public List<Module> GetModules()
        {
            List<Module> modules = new List<Module>();
            foreach (var modulePair in ModulesDictionary)
            {
                modules.Add(modulePair.Value);
            }

            return modules;
        }

        /** Discovers and Creates all the modules. */
        public void DiscoverAndCreateModules(string engineRootDirPath, DirectoryReference engineIntermediatePath)
        {
            //---- TODO Logger ----
            System.Console.WriteLine();
            System.Console.WriteLine("Scanning Modules....");
            //----

            List<FileReference> foundModulesFiles = ScanModules(engineRootDirPath);

            //---- TODO Logger ----
            StringBuilder stringBuilder= new StringBuilder();
            stringBuilder.AppendLine("Discovered {0} Modules.", foundModulesFiles.Count);
            System.Console.WriteLine(stringBuilder);
            //----

            CreateAssemblyManager(engineIntermediatePath, foundModulesFiles);
            CreateModules(foundModulesFiles);         
        }
        

        List<FileReference> ScanModules(string engineRootDirPath)
        {
            List<string> moduleFilter = new List<string>();
            moduleFilter.Add("Module.cs");

            List<FileReference> foundModuleFiles =  FileScanner.FindFilesInDirectoryWithFilter(
                    engineRootDirPath,
                    filesFilterMode: EFileScannerFilterMode.EInclusive,
                    fileExtensionsToFilter: moduleFilter);

            CacheModulesFilesInfo(foundModuleFiles);

            return foundModuleFiles;
        }

        void CacheModulesFilesInfo(List<FileReference> foundModuleFiles)
        {
            foreach (FileReference moduleFileRef in foundModuleFiles)
            {
                string moduleName = moduleFileRef.GetFileNameWithoutExtension();
                if (!ModulesFileRefsDictionary.ContainsKey(moduleName))
                {
                    ModulesFileRefsDictionary.Add(moduleName, moduleFileRef);
                }
            }
        }

        public Module? GetModuleByName(string moduleName)
        {
            Module? specificModule;
            if (!ModulesDictionary.TryGetValue(moduleName, out specificModule))
            {
                // ModuleNotFound then we create it
                FileReference? moduleFileRef;
                if (!ModulesFileRefsDictionary.TryGetValue(moduleName, out moduleFileRef))
                {
                    throw new InvalidOperationException();
                }

                specificModule = TryCreateModule(moduleFileRef);                
            }

            return specificModule;
        }

        void CreateModules(List<FileReference> modulesFiles)
        {
            // TODO Function creates the modules.
            if (modulesFiles.Count > 0)
            {
                foreach (FileReference moduleFileRef in modulesFiles)
                {                    
                    TryCreateModule(moduleFileRef);
                }
            }
            else
            {
                // No Modules Found, maybe a problem here.
            }              
        }

        private Module? TryCreateModule(FileReference moduleFileRef)
        {
            string moduleName = moduleFileRef.GetFileNameWithoutExtension();

            Module? specificModule;            
            if (!ModulesDictionary.TryGetValue(moduleName, out specificModule))
            {                
                ModuleBuilder? moduleBuilder = GetModuleBuilderFromModuleName(moduleName);
                specificModule =  CreateSpecificModule(moduleName, moduleBuilder, moduleFileRef);
            }

            return specificModule;
        }

        Module CreateSpecificModule(string moduleName, ModuleBuilder builder, FileReference moduleFileRef)
        {
            Module createdModule = new Module(builder, moduleFileRef, this);
            ModulesDictionary.Add(moduleName, createdModule);

            return createdModule;
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
