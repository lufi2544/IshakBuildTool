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
            if (dirRelativeData.directories.Count > 1)
            {
                return dirRelativeData.ConstructPathFromDirectoryIdx(dirRelativeData.directories.Count - 1);
            }

            return string.Empty;
        }

        public static string MakeRelativeTo(DirectoryReference referenceDir, DirectoryReference makeRelativeToThisDir)
        {
            // See how much one is equal to the other.

            // 1-> under the same folder, but in different folders
            // 1 we iterate through the folder path, and make a new string and stop that string when 
            // they stop equal.
            // 2 from the one that is not equal, we reach to the actual makerelativeDir, count the folders
            // and that would be the amount of ../

            string otherDir = referenceDir.Path;
            string relativeToDir = makeRelativeToThisDir.Path;

            MakeDirectoryRelativeData otherDirRelativeData = new MakeDirectoryRelativeData(otherDir);
            MakeDirectoryRelativeData relativeDirData = new MakeDirectoryRelativeData(makeRelativeToThisDir.Path);

            int flaggedIdx = -1;
            for(int idx = 0; idx < relativeDirData.directories.Count; ++idx)
            {
                if (!relativeDirData.directories[idx].name.Equals(otherDirRelativeData.directories[idx].name))
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

                    flaggedIdx = idx - 1;
                    break;
                }                                                    
            }
            

            // Checked if we got some DirReference that was not equal
            StringBuilder finalRelativatedPath = new StringBuilder();            
            if (flaggedIdx != -1)
            {


                int pointsNum = relativeDirData.directories.Count - flaggedIdx;
                string partialPath = otherDirRelativeData.ConstructPathFromDirectoryIdx(flaggedIdx - 1);

                StringBuilder pointsStringB = new StringBuilder();
                for (int pointsIdx = 0; pointsIdx < pointsNum; ++pointsIdx)
                {
                    pointsStringB.Append("../");
                }

                finalRelativatedPath.Append(pointsStringB.ToString() + partialPath);

            }
            else
            {
                // Nothing equal, so we return the absolute path.
                finalRelativatedPath.Append(referenceDir.Path);
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
