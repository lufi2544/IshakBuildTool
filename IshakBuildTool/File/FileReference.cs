using IshakBuildTool.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.ProjectFile
{
    public enum EFileType
    {
        None,
        Header,
        Source,
        Module
    }

    public class FileReference
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public DirectoryReference Directory { get; set; }

        public EFileType FileType { get; set; }

        public static readonly FileReference Null = new FileReference("");

        public FileReference(string PathParm)
        {
            Path = PathParm;
            Name = System.IO.Path.GetFileName(PathParm);
            Directory = GetDirectory();
            SetFileType();
        }

        public bool Exists()
        {
            return FileUtils.FileExists(this);
        }

        void SetFileType()
        {
            if (Name.Contains(".h") || Name.Contains(".hpp"))
            {
                FileType= EFileType.Header;

            }else if (Name.Contains(".cpp"))
            {
                FileType= EFileType.Source;

            }else if (Name.Contains(".Module."))
            {
                FileType= EFileType.Module;
            }
            else
            {
                FileType = EFileType.None;
            }            
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
    }
}
