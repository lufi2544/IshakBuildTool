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

        public bool IsValid()
        {
            return Path != string.Empty;
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
