using IshakBuildTool.ProjectFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.ToolChain.Windows
{
    /** Class that defines the installation of Visual Studio and its properties. */
    internal class VisualStudioInstallation
    {   

        public DirectoryReference Directory { get; set; }

        VersionData Version { get; set; }

        WindowsCompilerType CompilerType { get; set; }

        public  VisualStudioInstallation(WindowsCompilerType compilerTypeParam, VersionData versionDataParam, DirectoryReference dirRef) 
        {
            CompilerType = compilerTypeParam;
            Version = versionDataParam;
            Directory = dirRef;
        }
    }
}
