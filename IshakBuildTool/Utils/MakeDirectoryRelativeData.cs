
using System.Text;

namespace IshakBuildTool.Utils
{

    internal class DirectoryData
    {
        public DirectoryData(string nameParam)
        {
            Name = nameParam;
        }

        public string Name { get; set; }
    }

    internal class MakeDirectoryRelativeData
    {

        public List<DirectoryData> Directories { get; set; }
        string Path;

        public MakeDirectoryRelativeData(string absolutePath) 
        {
            Path = absolutePath;

            Directories = new List<DirectoryData>();
            DivideInDirectories();
        }

        private void DivideInDirectories()
        {
            string actualDirectoryStr = string.Empty;
            for (int idx = 0; idx < Path.Length; ++idx)
            {
                char actualChar = Path[idx];
                if (actualChar  == '\\' || idx == Path.Length - 1)
                {
                    if (idx == Path.Length - 1)
                    {
                        actualDirectoryStr += actualChar;
                    }

                    Directories.Add(new DirectoryData(actualDirectoryStr));
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

            for (int idx = fromIdx; idx < Directories.Count; ++idx)
            {
                DirectoryData dirData = Directories[idx];

                if (idx == Directories.Count - 1)
                {
                    finalPathBuilder.Append("{0}", dirData.Name);

                }
                else if (idx > 0)
                {
                    finalPathBuilder.Append("{0}" + "\\", dirData.Name);
                }

            }

            return finalPathBuilder.ToString();
        }
    }
}
