using IshakBuildTool.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.ProjectFile
{
    public class DirectoryReference
    {
        public DirectoryReference(string pathParam) 
        {
            Path = pathParam;

        }

        DirectoryReference GetParentDir()
        {
            return new DirectoryReference(DirectoryUtils.GetParentDirectoryPathFromDirectory(Path));
        }

        public bool IsUnder(DirectoryReference otherDir)
        {            
            return Path.Contains(otherDir.Path);                       
        }

        public string Path { get; set; }        
    }
}
