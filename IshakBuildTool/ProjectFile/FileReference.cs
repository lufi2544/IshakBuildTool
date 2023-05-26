using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.ProjectFile
{
    internal class FileReference
    {
        public FileReference(string PathParm)
        {
            path = PathParm;
        }
             
        public DirectoryReference GetDirectory()
        {
            return new DirectoryReference(Path.GetDirectoryName(path));
        }

        public string path { get; set; }
    }
}
