using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IshakBuildTool.ProjectFile;

namespace IshakBuildTool.Utils
{
    internal class DirectoryUtils
    {      

        public static void CreateFileWithContent(string path, string content, bool bOverride = true)
        {
            bool bFileExists = File.Exists(path);

            if (bFileExists && !bOverride)
            {
                //  for now let's do nothing here.
            }
            else
            {
                //create the solution

                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                // In this case the UFT8.Encoding will add the BOM marker that is just 2 bytes of memory 
                // to the file lenght, contrary to creating a UTF8 encoding directly --> new UTF8Encoding.
                File.WriteAllText(path, content, Encoding.UTF8);

            }
        }

        public static bool IsUnderEngineFolder(DirectoryReference dirRef)
        {
            string engineSourceFolder = GetEngineSourceFolder();
            return dirRef.Path.Contains(engineSourceFolder);
        }

        public static string GetEngineSourceFolder()
        {            
            // The Ishak Build Tool must always be under the Source Folder.            
            return GetPathUntilDirectory(Environment.CurrentDirectory, "Source");            
        }

        /** Returns the path ultil the diretory specified and ignoring the rest of the dirs after.
         * 
         * E.g:
         * Source/Engine/Module/Renderer ---- UntilDir: Module
         * Return: Source/Engine/Module
         */
        private static string GetPathUntilDirectory(string entireDir, string ultilThiDirName)
        {
            string actualDirName = string.Empty;
            string formattedDir = string.Empty;

            for (int idx = 0; idx < entireDir.Length; ++idx)
            {
                char actualDirChar = entireDir[idx];
                if (actualDirChar == '\\')
                {
                    // When reached to a '\' we compare the dir name,
                    // if is the one wanted, we return the path
                    if (actualDirName == ultilThiDirName)
                    {
                        return formattedDir;
                    }
                    else
                    {                        
                        actualDirName = string.Empty;
                    }
                }
                else
                {                    
                    actualDirName += actualDirChar;
                }

                formattedDir += actualDirChar;

            }

            return string.Empty;
        }

        public static void TryCreateDirectory(string dirPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);

            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
        }
        public static string GetParentDirectoryPathFromDirectory(string dirPath)
        {
            if (dirPath.Equals(string.Empty))
            {
                return dirPath;
            }

            MakeDirectoryRelativeData dirRelativeData = new MakeDirectoryRelativeData(dirPath);
          
            // We have to make sure that we have more than 1 directory for getting the parentDir
            if (dirRelativeData.Directories.Count > 1)
            {
                return dirRelativeData.ConstructPathFromDirectoryIdx(dirRelativeData.Directories.Count - 1);
            }

            return string.Empty;
        }

        /** Removes the '..\' from passed in path. */
        public static string RemoveFolderMoveCharsFromDirRef(DirectoryReference dirRef)
        {
            MakeDirectoryRelativeData pathRelativeData = new MakeDirectoryRelativeData(dirRef.Path);

            StringBuilder removedPointsSB = new StringBuilder();
            int dirsCount = pathRelativeData.Directories.Count;

            for (int idx = 0; idx < dirsCount; ++idx)
            {
                string actualDirName = pathRelativeData.Directories[idx].Name;

                if (actualDirName != ".." )
                {
                    if (idx == dirsCount - 1)
                    {
                        removedPointsSB.Append(actualDirName);
                    }
                    else
                    {
                        removedPointsSB.Append(actualDirName + '\\');
                    }
                }
            }

            return removedPointsSB.ToString();
        }

        public static FileReference MakeRelativeTo(FileReference targetDir, FileReference parentDir)
        {
            string relativePath = MakeRelativeTo(targetDir.Directory, parentDir.Directory);

            return new FileReference(relativePath + "\\" + targetDir.Name);            
        }        

        /** Makes the parent Dir relative to the target Dir. */
        public static string MakeRelativeTo(DirectoryReference targetDir, DirectoryReference parentFile)
        {
            // See how much one is equal to the other.

            string otherDir = targetDir.Path;
            string parentFileDir = parentFile.Path;

            MakeDirectoryRelativeData targetDirRelativeData = new MakeDirectoryRelativeData(otherDir);
            MakeDirectoryRelativeData parentFileRelativeData = new MakeDirectoryRelativeData(parentFileDir);

            int flaggedIdx = -1;
            for(int idx = 0; idx < parentFileRelativeData.Directories.Count; ++idx)
            {
                if (!parentFileRelativeData.Directories[idx].Name.Equals(targetDirRelativeData.Directories[idx].Name))
                {
                    if (idx == 0)
                    {
                        break;
                    }

                    // Construct path from the flagged idx - 1 because this is whre the directories start to differ.
                    // Adds the ../ that correspond to the folders count needed for reaching that folder.                                                           
                    //
                    //-> Refered Dir:         C/Engine/Binaries/Game/Space/Source...
                    //                        0    1      2      3     4     5
                    //                                 flagged
                    //
                    //->RelativeToThis:      C/Engine/Build/Game
                    //                        0   1     2     3  
                    //
                    // Final Relative Path: ../../Binaries/Game/Space/Source... 

                    flaggedIdx = idx;
                    break;
                }                                                    
            }
            
            // the flaggedIdx is the idx where we need to go with the target file.

            // Checked if we got some DirReference that was not equal
            StringBuilder finalRelativatedPath = new StringBuilder();            
            if (flaggedIdx != -1)
            {


                int pointsNum = parentFileRelativeData.Directories.Count - flaggedIdx;
                string partialPath = targetDirRelativeData.ConstructPathFromDirectoryIdx(flaggedIdx);

                StringBuilder pointsStringB = new StringBuilder();
                for (int pointsIdx = 0; pointsIdx < pointsNum; ++pointsIdx)
                {
                    pointsStringB.Append("..\\");
                }

                finalRelativatedPath.Append(pointsStringB.ToString() + partialPath);

            }
            else
            {
                // Nothing equal, so we return the absolute path.
                finalRelativatedPath.Append(targetDir.Path);
            }
          
            return finalRelativatedPath.ToString();
        }


        // TODO make relative to the project .sln 
       public static string GetPublicOrPrivateDirectoryPathFromDirectory(DirectoryReference dir)
       {
            const string publicStr = "Public";           
            const string privateStr = "Private";

            bool bScanningPublic = false;
            bool bScanningPrivate = false;

            int strIdx = 0;
            StringBuilder directoryStr = new StringBuilder();

            for (int idx = 0; idx < dir.Path.Length; ++idx)
            {                
                char actualLetter = dir.Path.ElementAt(idx);               
                char privateStrLetter = privateStr.ElementAt(strIdx);
                if (privateStrLetter == actualLetter)
                {                    
                    bScanningPrivate = true;
                    bScanningPublic = false;

                    ++strIdx;
                }
                else
                {
                    char publicStrLetter = publicStr.ElementAt(strIdx);
                    if (publicStrLetter == actualLetter)
                    {
                        bScanningPublic= true;
                        bScanningPrivate = false;

                        ++strIdx;
                    }
                    else
                    {
                        strIdx = 0;
                    }
                }

                directoryStr.Append(actualLetter);

                if (bScanningPublic)
                {
                    if (strIdx == publicStr.Length)
                    {
                        return directoryStr.ToString();
                    }
                }else if (bScanningPrivate)
                {
                    if (strIdx == privateStr.Length)
                    {
                        return directoryStr.ToString();
                    }

                }
                

            }

            return string.Empty;
       }
    }
}
