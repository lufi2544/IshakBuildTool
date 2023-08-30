using IshakBuildTool.Build;
using IshakBuildTool.ProjectFile;
using IshakBuildTool.Utils;
using System.Diagnostics;
using System.Text;

namespace IshakBuildTool.Project.Modules
{
    /** Engine Module */
    public class IshakModule
    {
        public FileReference ModuleFile { get; set; }
        public string Name { get; set; }

        public string? APIMacroName { get; set; } = null;
        public DirectoryReference? PublicDirectoryRef { get; set; }
        public DirectoryReference? PrivateDirectoryRef { get; set; }

        public DirectoryReference? Directory { get; set; }

        public DirectoryReference? ObjFileRef { get; set; }

        public DirectoryReference? BinariesDirectory { get; set; }

        public string? ModulesDependencyDirsString { get; set; }
        public List<FileReference>? SourceFiles { get; set; }

        ModuleManager ModuleManager;
        List<string>? PublicDependentModules;
        List<string>? PrivateDependentModules;

        public IshakModule(ModuleBuilder moduleBuilder,  FileReference moduleFileRef, ModuleManager moduleManager)
        {
            ModuleFile = moduleFileRef;  
            Directory = moduleFileRef.Directory;
            Name = moduleFileRef.GetFileNameWithoutExtension();
            BinariesDirectory = DirectoryUtils.Combine(new DirectoryReference(BuildProjectManager.GetInstance().GetProjectDirectoryParams().BinaryDir), Name.ToString());
            PublicDependentModules = moduleBuilder.PublicModuleDependencies;
            PrivateDependentModules = moduleBuilder.PrivateModuleDependencies;
            ModuleManager = moduleManager;
            
            SetAPIMacroName();
            MakePublicDirs();
            MakePrivateDir();
            BuildModuleDependentDirectoriesString(); 
            AddSourceFiles();
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
