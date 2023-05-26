using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.Utils
{

    internal class DirectoryData
    {
        public DirectoryData(string nameParam)
        {
            name = nameParam;
        }

        public string name { get; set; }
    }

    internal class MakeDirectoryRelativeData
    {
        public MakeDirectoryRelativeData(string absolutePath) 
        {
            path = absolutePath;

            DivideInDirectories();
        }

        private void DivideInDirectories()
        {
            string actualDirectoryStr = string.Empty;
            for (int idx = 0; idx < path.Length; ++idx)
            {
                char actualChar = path[idx];
                if (actualChar == '/')
                {
                    directories.Add(new DirectoryData(actualDirectoryStr));
                    actualDirectoryStr = string.Empty;
                }
                else
                {
                    actualDirectoryStr += actualChar;
                }
            }
        }

        public string ConstructPathFromDirectoryIdx(int idx)
        {
            string finalBuiltPath = string.Empty;

            DirectoryData dirData = directories[idx];

            return finalBuiltPath;
        }

        public List<DirectoryData> directories { get; set; }
        string path;
    }
}
