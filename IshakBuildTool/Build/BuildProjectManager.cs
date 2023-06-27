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
        
        public string RootDir { get; set;} = string.Empty;
        public string SourceDir { get; set; } = string.Empty;
        public string IntermediateDir { get; set; } = string.Empty;
        public string ProjectFilesDir { get; set; } = string.Empty;

        public string ProjectType { get; set; } = string.Empty;        
    }
    /** Manager that will hold info when building the project like the Source Dir, Intermediate, etc.d */
    internal class BuildProjectManager
    {
        static BuildProjectManager? SingleTonProjectManager = null; 

        public CommandLineArgs CommandLineArgs { get; set; }
      
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

            localProjectDirectoryParams.RootDir = foundArg;
            localProjectDirectoryParams.ProjectType = foundProjectTypeArg;

            SetUpFolders(ref localProjectDirectoryParams);
            ThisEntireProjectDirectoryParams = localProjectDirectoryParams;           
        }            

        void SetUpFolders(ref EntireProjectDirectoryParams outParams)
        {
            string BaseDir = outParams.RootDir + Path.DirectorySeparatorChar;

            string SourceDir = BaseDir + "Source" + Path.DirectorySeparatorChar;
            string IntermediateDir = BaseDir + "Intermediate" + Path.DirectorySeparatorChar;
            string ProjectFilesDir = IntermediateDir + "ProjectFiles" + Path.DirectorySeparatorChar;

            outParams.SourceDir = SourceDir;
            outParams.IntermediateDir = IntermediateDir;
            outParams.ProjectFilesDir = ProjectFilesDir;
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
