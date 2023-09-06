using IshakBuildTool.Utils;
using System.Text;
using System.Linq;


namespace IshakBuildTool.ProjectFile
{
    public class DirectoryReference
    {
        public static char DirectorySeparatorChar = System.IO.Path.DirectorySeparatorChar;        
        public string Path { get; set; } = string.Empty;        
        
        public DirectoryReference() 
        {
            Path = string.Empty;                        
        }
            

        public DirectoryReference(string pathParam) 
        {
            Path = pathParam;
        }     

        public static bool operator ==(DirectoryReference dir1, DirectoryReference dir2)
        {

            if (ReferenceEquals(dir1, dir2))
            {
                return true;
            }

            if (ReferenceEquals(dir1, null)) 
            {
                return false;
            }

            if(ReferenceEquals(dir2, null)) 
            {
                return false;
            }
            
            return dir1.Path == dir2.Path;
        }

        public static bool operator !=(DirectoryReference dir1, DirectoryReference dir2)
        {
            return !(dir1 == dir2);
        }

        public bool IsValid()
        {
            return Path != string.Empty;
        }

        public bool Exist()
        {
            return DirectoryUtils.DirectoryExists(Path);
        }

        public string GetDirectoryName()
        {
            if (Path.Length == 0)
            {
                // TODO Exception
                throw new ArgumentException();
            }

            int charIdx = Path.Length - 1;
            char actualChar = Path[charIdx];
            string reversedDirName = string.Empty;
            while (actualChar != DirectorySeparatorChar)
            {
                reversedDirName += actualChar;
                
                --charIdx;
                actualChar = Path[charIdx];
            }
                        
            return new string(reversedDirName.Reverse().ToArray());
        }

        public DirectoryReference GetParentDir()
        {
            return new DirectoryReference(DirectoryUtils.GetParentDirectoryPathFromDirectory(Path));
        }
        
        public IEnumerable<DirectoryReference> GetChildDirectories()
        {
            foreach (string dirName in Directory.EnumerateDirectories(Path))
            {
                yield return new DirectoryReference(dirName);
            }            
        }

        public bool IsUnder(DirectoryReference otherDir)
        {            
            return Path.Contains(otherDir.Path);                       
        }      
    }
}
