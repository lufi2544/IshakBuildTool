using IshakBuildTool.ProjectFile;
using IshakBuildTool.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.Build
{

    /** Struct that holds info about all the project important directories,
     * so they can be accessed all along the project. */
    public struct EntireProjectDirectoryParams
    {
        public EntireProjectDirectoryParams() 
        {
            
        }
        
        public DirectoryReference RootDir { get; set;} = new DirectoryReference();
        public DirectoryReference SourceDir { get; set; }  = new DirectoryReference();
        public DirectoryReference IntermediateDir { get; set; } = new DirectoryReference();
        public DirectoryReference ProjectFilesDir { get; set; } = new DirectoryReference();
        public DirectoryReference BinaryDir { get; set; } = new DirectoryReference();

        public string ProjectType { get; set; } = string.Empty;        

        public string BuildEngineScriptPath { get; set; } = string.Empty;
        public string CompileEngineScriptPath { get; set; } = string.Empty;

        public string EngineExecutablePath { get; set; } = string.Empty;
        public string EngineExecutableDir { get; set; } = string.Empty;
    }
    /** Manager that will hold info when building the project like the Source Dir, Intermediate, etc.d */
    internal class BuildProjectManager
    {
        static BuildProjectManager? SingleTonProjectManager = null; 

        public CommandLineArgs? CommandLineArgs { get; set; } = null;
      
        private EntireProjectDirectoryParams ThisEntireProjectDirectoryParams { get; set; }


        static public BuildProjectManager GetInstance()
        {
            if (SingleTonProjectManager == null)
            {
                SingleTonProjectManager = new BuildProjectManager();
            }

            return SingleTonProjectManager; 
        }

        public void Init(CommandLineArgs args)
        {            
            CommandLineArgs = args;
            EntireProjectDirectoryParams localProjectDirectoryParams = new EntireProjectDirectoryParams();

            bool bFoundRootDirArgumentCategory;

            // We are gonna find the directory for the engine
            string foundArg = args.GetArgumentFromCategory("-r", out bFoundRootDirArgumentCategory);
            if (bFoundRootDirArgumentCategory == false)
            {
                // TODO Exception
                throw new Exception();
            }

            string foundProjectTypeArg = args.GetArgumentFromCategory("-pt", out bFoundRootDirArgumentCategory);

            if(bFoundRootDirArgumentCategory == false) 
            {
                throw new Exception(); 
            }


            bool bFromScrpit = false;
            DirectoryReference thisDir = new DirectoryReference(Directory.GetCurrentDirectory().ToString());
            if (!thisDir.IsUnder("net"))
            {
                bFromScrpit = true;
            }


            DirectoryReference? rootDir = null;
            if (bFromScrpit)
            {
                rootDir = new DirectoryReference("../../../../IshakEngine");
            }
            else
            {
                rootDir = new DirectoryReference("../../../../../../../IshakEngine");
            }

            
            if (rootDir.Exist() == false)
            {
                throw new ExecutionEngineException() { };
            }
            else
            {
                localProjectDirectoryParams.RootDir = rootDir;
            }
            
            localProjectDirectoryParams.ProjectType = foundProjectTypeArg;

            SetUpFolders(ref localProjectDirectoryParams);
            ThisEntireProjectDirectoryParams = localProjectDirectoryParams;           
        }            

        void SetUpFolders(ref EntireProjectDirectoryParams outParams)
        {
            DirectoryReference BaseDir = outParams.RootDir;

            string SourceDir = BaseDir.Path + DirectoryReference.DirectorySeparatorChar + "Source" + DirectoryReference.DirectorySeparatorChar;
            string IntermediateDir = BaseDir.Path + DirectoryReference.DirectorySeparatorChar  + "Intermediate" + DirectoryReference.DirectorySeparatorChar;
            string ProjectFilesDir = IntermediateDir + "ProjectFiles" + DirectoryReference.DirectorySeparatorChar;

            outParams.BinaryDir = new DirectoryReference(BaseDir.Path + DirectoryReference.DirectorySeparatorChar + "Binaries");
            outParams.SourceDir = new DirectoryReference(SourceDir);
            outParams.IntermediateDir = new DirectoryReference(IntermediateDir);
            outParams.ProjectFilesDir = new DirectoryReference(ProjectFilesDir);

            StringBuilder compileEngineScrpitPath = new StringBuilder();

            compileEngineScrpitPath.AppendFormat("{0}", BaseDir.Path + DirectoryReference.DirectorySeparatorChar + "CompileIshakEngine.bat");
            
            outParams.CompileEngineScriptPath = compileEngineScrpitPath.ToString();

            // TODO BUILD REFACTOR
            outParams.EngineExecutablePath = BaseDir.Path + DirectoryReference.DirectorySeparatorChar + "Binaries" + DirectoryReference.DirectorySeparatorChar + "IshakEngine.exe";
            
        }


        public EntireProjectDirectoryParams GetProjectDirectoryParams()
        {
            // TODO Exception
            /*
            if (EntireProjectDirectoryParams == null)
            {
                throw new InvalidOperationException();
            }
            */

            return ThisEntireProjectDirectoryParams;
        }  
                
    }
}
