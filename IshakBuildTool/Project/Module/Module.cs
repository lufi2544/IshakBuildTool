using IshakBuildTool.ProjectFile;
using IshakBuildTool.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.Project.Modules
{
    /** Engine Module */
    public class Module
    {
        public FileReference moduleFile { get; set; }
        public DirectoryReference PublicDirectoryRef { get; set; }
        public DirectoryReference PrivateDirectoryRef { get; set; }

        public string ModulesDependencyDirsString { get; set; }
        public List<FileReference> SourceFiles { get; set; }

        ModuleManager ModuleManager;
        List<string> PublicDependentModules;
        List<string> PrivateDependentModules;

        public Module(ModuleBuilder moduleBuilder,  FileReference moduleFileRef, ModuleManager moduleManager)
        {
            moduleFile = moduleFileRef;

            PublicDependentModules = moduleBuilder.PublicModuleDependencies;
            PrivateDependentModules = moduleBuilder.PrivateModuleDependencies;
            ModuleManager = moduleManager;
                   
            MakePubliDirs();
            MakePrivateDir();
            BuildModuleDependentDirectoriesString(); 
            AddSourceFiles();
        }        

        void MakePubliDirs()
        {
            string modulePublicDirPath = moduleFile.Directory.Path + Path.DirectorySeparatorChar + "Public";

            // If the dir does not exist, we just create it.
            DirectoryUtils.TryCreateDirectory(modulePublicDirPath);
            PublicDirectoryRef = new DirectoryReference(modulePublicDirPath);
        }

        void MakePrivateDir()
        {
            string modulePrivateDirPath = moduleFile.Directory.Path + Path.DirectorySeparatorChar + "Private";

            // If the dir does not exist, we just create it.
            DirectoryUtils.TryCreateDirectory(modulePrivateDirPath);
            PrivateDirectoryRef = new DirectoryReference(modulePrivateDirPath);
        } 

        /** We take all the dependent modules and add their Public dirs to this module dependency dir list. */
        void BuildModuleDependentDirectoriesString()
        {
            StringBuilder moduleDependencySB = new StringBuilder();

            // We add by default the Private Dir for this module.
            moduleDependencySB.Append("{0};", PrivateDirectoryRef.Path);

            AddPublicModuleDependencies(moduleDependencySB);
            AddPrivateModuleDependencies(moduleDependencySB);

            ModulesDependencyDirsString = moduleDependencySB.ToString();
        }


        /** We take all the dependent modules and add their Private dirs to this module dependency dir list. */
        void AddPublicModuleDependencies(StringBuilder stringBuilder)
        {            
            foreach (string dependentModuleName in PublicDependentModules)
            {
                Module? dependentModule = ModuleManager.GetModuleByName(dependentModuleName);
                stringBuilder.Append("{0};", dependentModule.PublicDirectoryRef.Path);
            }
        }

        void AddPrivateModuleDependencies(StringBuilder stringBuilder)
        {
            foreach (string dependentModuleName in PrivateDependentModules)
            {
                Module? dependentModule = ModuleManager.GetModuleByName(dependentModuleName);
                stringBuilder.Append("{0};", dependentModule.PrivateDirectoryRef.Path);
            }
        }

        void AddSourceFiles()
        {
            SourceFiles = FileScanner.FindSourceFiles(moduleFile.Directory.Path);            
        }


    }
}
