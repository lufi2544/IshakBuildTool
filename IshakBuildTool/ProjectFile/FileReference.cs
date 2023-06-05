using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.ProjectFile
{
    public class FileReference
    {
        public FileReference(string PathParm)
        {
            Path = PathParm;
            Name = GetFileNameWithoutExtension();
            Directory = GetDirectory();
        }

        private string GetFileNameWithoutExtension()
        {
            string fileName = string.Empty;
            bool bCanAddToFileName = false;
            for (int charIdx = Path.Length - 1; charIdx < Path.Length; --charIdx)
            {
                char actualChar = Path[charIdx];
                if (actualChar == '\\')
                {
                    var name = fileName.Reverse().ToArray();
                    return new string(name);
                }

                // If we reach to a point we start adding characters to the fileName
                // but if this is the non first point encountered, then we just delete 
                // the fileName content and start writing again
                //
                // E.g: 
                // "Core.Module.cs" -> will return "Core" 
                if (actualChar == '.')
                {
                    bCanAddToFileName = true;

                    if (fileName.Length > 0)
                    {
                        fileName = string.Empty;
                    }
                    continue;
                }

                if (bCanAddToFileName)
                {
                    fileName += actualChar;
                }
            }

            return fileName;
        }

        private DirectoryReference GetDirectory()
        {
            return new DirectoryReference(System.IO.Path.GetDirectoryName(Path));
        }

        public string GetNameWithoutPathExtension()
        {
            string pureRawName = string.Empty;

            for (int charIdx = Path.Length - 1; charIdx > 0; --charIdx)
            {
                char actualChar = Path[charIdx];
                if (actualChar == '\\')
                {
                    break;
                }


            }

            // Process the reverse string
            if (pureRawName.Length > 0)
            {
                pureRawName.Reverse();
            }

            return pureRawName;
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

        public static FileStream Open(FileReference fileRef, FileMode fileMode)
        {
            return File.Open(fileRef.Path, fileMode);
        }

        public FileReference ChangeExtensionCopy(string newExtension)
        {
            string changedExtensionFile = System.IO.Path.ChangeExtension(Path, newExtension);
            return new FileReference(changedExtensionFile);
        }


        public string Name { get; set; }        
        public string Path { get; set; }      
        public DirectoryReference Directory { get; set; }
    }
}
