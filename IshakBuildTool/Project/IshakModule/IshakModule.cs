using IshakBuildTool.Build;
using IshakBuildTool.Platform;
using IshakBuildTool.ProjectFile;
using IshakBuildTool.Utils;
using System.Diagnostics;
using System.Text;

namespace IshakBuildTool.Project.Modules
{
    /** Engine Module */
    public class IshakModule
    {        
        public bool bCompiled { get; set; }

        public bool bDllSetToCompiled { get; set; }

        public FileReference ModuleFile { get; set; }
        public string Name { get; set; }

        public string? APIMacroName { get; set; } = null;
        public DirectoryReference? PublicDirectoryRef { get; set; }
        public DirectoryReference? PrivateDirectoryRef { get; set; }

        public FileReference? ModuleDllImportFile { get; set; }
        public FileReference? ModuleDllFile { get; set; }

        public DirectoryReference? Directory { get; set; }

        public DirectoryReference? ObjFileRef { get; set; }

        public DirectoryReference? BinariesDirectory { get; set; }

        public string? ModulesDependencyDirsString { get; set; }
        public List<FileReference>? SourceFiles { get; set; }
        
        public bool bThirdParty = false;

        // TODO send to rules
        public string ThirdPartyModuleDllName = string.Empty;
        public string ThirdPartyModuleDllImportName = string.Empty;

        ModuleManager ModuleManager;
        public List<string>? PublicDependentModules;
        public List<string>? PrivateDependentModules;

        public IshakModule(ModuleBuilder moduleBuilder,  FileReference moduleFileRef, ModuleManager moduleManager)
        {            
            ModuleFile = moduleFileRef;  
            Directory = moduleFileRef.Directory;
            Name = moduleFileRef.GetFileNameWithoutExtension();
            BinariesDirectory = DirectoryUtils.Combine(new DirectoryReference(BuildProjectManager.GetInstance().GetProjectDirectoryParams().BinaryDir), Name.ToString());
            bThirdParty = Directory.Path.Contains("ThirdParty");
            ThirdPartyModuleDllName = moduleBuilder.ThirdPartyDLLName;
            ThirdPartyModuleDllImportName= moduleBuilder.ThirdPartyDLLImportName;

            if (bThirdParty)
            {
                ModuleDllImportFile = new FileReference(FileUtils.Combine(new DirectoryReference(BuildProjectManager.GetInstance().GetProjectDirectoryParams().BinaryDir), ThirdPartyModuleDllImportName).Path);
                ModuleDllFile = new FileReference(FileUtils.Combine(new DirectoryReference(BuildProjectManager.GetInstance().GetProjectDirectoryParams().BinaryDir), ThirdPartyModuleDllName).Path);
            }
            else
            {
                ModuleDllImportFile = new FileReference(FileUtils.Combine(new DirectoryReference(BuildProjectManager.GetInstance().GetProjectDirectoryParams().BinaryDir), Name + BinaryTypesExtension.StaticLib).Path);
                ModuleDllFile = new FileReference(FileUtils.Combine(new DirectoryReference(BuildProjectManager.GetInstance().GetProjectDirectoryParams().BinaryDir), Name + BinaryTypesExtension.DynamicLib).Path);            
            }
            PublicDependentModules = moduleBuilder.PublicModuleDependencies;
            PrivateDependentModules = moduleBuilder.PrivateModuleDependencies;
            ModuleManager = moduleManager;
            
            SetAPIMacroName();
            MakePublicDirs();
            MakePrivateDir();
            BuildModuleDependentDirectoriesString(); 
            AddSourceFiles();

        }        
        
        // TODO BUILDREFACTOR MAKE DIRREF LIST WORK !!
        public List<string> GetIncludeDirsForThisModuleWhenCompiling()
        {
            List<string> directoryReferences = new List<string>();

            foreach (FileReference file in SourceFiles)
            {
                if (file.FileType != EFileType.Header)
                {
                    continue;
                }
                if (!directoryReferences.Contains(file.Directory.Path))
                {
                    directoryReferences.Add(file.Directory.Path);
                }
            }

            return directoryReferences;
        }


        public List<FileReference> GetHeaderFiles()
        {
            List<FileReference> headerFiles = new List<FileReference>();
            
            foreach(var file in SourceFiles)
            {
                if (file.FileType == EFileType.Header)
                {
                    headerFiles.Add(file);
                }
            }

            return headerFiles;
        }
        
        void SetAPIMacroName()
        {
            APIMacroName = Name + "_API";
        }
        void MakePublicDirs()
        {
            string modulePublicDirPath = ModuleFile.Directory.Path + Path.DirectorySeparatorChar + "Public";

            // If the dir does not exist, we just create it.
            DirectoryUtils.TryCreateDirectory(new DirectoryReference(modulePublicDirPath));
            PublicDirectoryRef = new DirectoryReference(modulePublicDirPath);
        }

        void MakePrivateDir()
        {
            string modulePrivateDirPath = ModuleFile.Directory.Path + Path.DirectorySeparatorChar + "Private";

            // If the dir does not exist, we just create it.
            DirectoryUtils.TryCreateDirectory(new DirectoryReference(modulePrivateDirPath));
            PrivateDirectoryRef = new DirectoryReference(modulePrivateDirPath);
        } 

        /** We take all the dependent modules and add their Public dirs to this module dependency dir list. */
        void BuildModuleDependentDirectoriesString()
        {
            StringBuilder moduleDependencySB = new StringBuilder();


            AddDefaultDirectories(ref moduleDependencySB);

            AddPublicModuleDependencies(moduleDependencySB);
            AddPrivateModuleDependencies(moduleDependencySB);

            ModulesDependencyDirsString = moduleDependencySB.ToString();
        }

        void AddDefaultDirectories(ref StringBuilder moduleDependencySB)
        {
            // We add by default the Private Dir for this module.
            moduleDependencySB.Append("{0};", PrivateDirectoryRef.Path);
            moduleDependencySB.Append("{0};", ModuleFile.Directory.Path);

        }


        /** We take all the dependent modules and add their Public dirs to this module dependency dir list. */
        void AddPublicModuleDependencies(StringBuilder stringBuilder)
        {            
            foreach (string dependentModuleName in PublicDependentModules)
            {
                IshakModule? dependentModule = ModuleManager.GetModuleByName(dependentModuleName);
                stringBuilder.Append("{0};", dependentModule.PublicDirectoryRef.Path);
            }

            // We add this own module Public Directory since we want to include the files in a Relative way to the Private one
            // this means that if we are in the Renderer module, we want to do just:  #include "Renderer.h" not "Modules/Public/Renderer/Renderer.h"
            stringBuilder.Append("{0};", PublicDirectoryRef.Path);
        }

        void AddPrivateModuleDependencies(StringBuilder stringBuilder)
        {
            foreach (string dependentModuleName in PrivateDependentModules)
            {
                IshakModule? dependentModule = ModuleManager.GetModuleByName(dependentModuleName);
                stringBuilder.Append("{0};", dependentModule.PrivateDirectoryRef.Path);
            }
        }

        void AddSourceFiles()
        {
            SourceFiles = FileScanner.FindSourceFiles(ModuleFile.Directory.Path);            
        }


    }
}
