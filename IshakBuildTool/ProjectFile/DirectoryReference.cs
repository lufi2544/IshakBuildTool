using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.ProjectFile
{
    internal class DirectoryReference
    {
        public DirectoryReference(string pathParam) 
        {
            path = pathParam;

        }

        public bool IsUnder(DirectoryReference otherDir)
        {            
            return path.Contains(otherDir.path);                       
        }

        public string path { get; set; }
    }
}
