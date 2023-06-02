using IshakBuildTool.ProjectFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.Project.Module
{
    internal class ModuleAssemblyManager
    {
       ModuleAssemblyManager(string engineAssemblyPath) 
       {
            EngineAssemblyPath = engineAssemblyPath;
       }

        void CreateAssemblyForModules(List<FileReference> moduleCreationFiles)
        {
            // TODO create the assembly object from this files.
        }

        Assembly? GlobalAssembly;
        string EngineAssemblyPath;
    }
}
