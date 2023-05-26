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

            directories = new List<DirectoryData>();
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
                    if (idx == path.Length - 1)
                    {
                        actualDirectoryStr += actualChar;
                    }

                    directories.Add(new DirectoryData(actualDirectoryStr));
                    actualDirectoryStr = string.Empty;
                }
                else
                {
                    actualDirectoryStr += actualChar;
                }
            }
        }

        public string ConstructPathFromDirectoryIdx(int fromIdx)
        {
            StringBuilder finalPathBuilder = new StringBuilder();

            for (int idx = fromIdx; idx < directories.Count; ++idx)
            {
                DirectoryData dirData = directories[idx];

                if (idx == directories.Count - 1)
                {
                    finalPathBuilder.Append("{0}", dirData.name);

                }else if (idx > 0)
                {                    
                    finalPathBuilder.Append("{0}/", dirData.name);
                }
                                 
            }

            return finalPathBuilder.ToString();
        }

        public List<DirectoryData> directories { get; set; }
        string path;
    }
}
