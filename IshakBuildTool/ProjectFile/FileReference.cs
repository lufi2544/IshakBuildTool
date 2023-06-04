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
            FullPath = PathParm;
        }
             
        public DirectoryReference GetDirectory()
        {
            return new DirectoryReference(Path.GetDirectoryName(FullPath));
        }

        public string GetPathWithoutFileExtension()
        {
            string noFileExtensionPath = string.Empty;
            for (int charIdx = 0; charIdx <  FullPath.Length; ++charIdx)
            {
                char actualChar = FullPath[charIdx];
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

            return FullPath;
        }

        public static FileStream Open(FileReference fileRef, FileMode fileMode)
        {
            return File.Open(fileRef.FullPath, fileMode);
        }

        public FileReference ChangeExtensionCopy(string newExtension)
        {
            string changedExtensionFile = Path.ChangeExtension(FullPath, newExtension);
            return new FileReference(changedExtensionFile);
        }


        public string FullPath { get; set; }
    }
}
