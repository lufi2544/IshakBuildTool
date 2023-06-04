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
            Path = PathParm;
        }
             
        public DirectoryReference GetDirectory()
        {
            return new DirectoryReference(System.IO.Path.GetDirectoryName(Path));
        }

        public string GetPathWithoutFileExtension()
        {
            string noFileExtensionPath = string.Empty;
            for (int charIdx = 0; charIdx <  Path.Length; ++charIdx)
            {
                char actualChar = Path[charIdx];
                if (actualChar == '.')
                {
                    return noFileExtensionPath;
                }

                noFileExtensionPath += actualChar;
            }

            if (noFileExtensionPath == string.Empty)
            {
               // TODO Exception throw exception if the file ref does not have a file with a file extension.
            }

            return Path;
        }

        public string GetFileNameWithoutExtension()
        {
            string fileName = string.Empty;
            bool bCanAddToFileName = false;
            for (int charIdx = Path.Length - 1; charIdx < Path.Length; --charIdx)
            {
                char actualChar = Path[charIdx];
                if (actualChar == '\\') 
                {
                    return fileName.Substring(fileName.Length - 1, 0);
                }

                if (actualChar == '.')
                {
                    bCanAddToFileName= true;
                    continue;
                }
               
                if (bCanAddToFileName)
                {
                    fileName += actualChar;
                }
            }

            return fileName;
        }

        public static FileStream Open(FileReference fileRef, FileMode fileMode)
        {
            return File.Open(fileRef.Path, fileMode);
        }

        public FileReference ChangeExtensionCopy(string newExtension)
        {
            string changedExtensionFile = System.IO.Path.ChangeExtension(Path, newExtension);
            return new FileReference(changedExtensionFile);
        }


        public string Path { get; set; }
    }
}
