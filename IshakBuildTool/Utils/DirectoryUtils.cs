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

        public static string MakeRelativeTo(DirectoryReference referenceDir, DirectoryReference makeRelativeToThisDir)
        {
            // See how much one is equal to the other.

            // 1-> under the same folder, but in different folders
            // 1 we iterate through the folder path, and make a new string and stop that string when 
            // they stop equal.
            // 2 from the one that is not equal, we reach to the actual makerelativeDir, count the folders
            // and that would be the amount of ../

            string otherDir = referenceDir.path;
            string relativeToDir = makeRelativeToThisDir.path;

            MakeDirectoryRelativeData otherDirRelativeData = new MakeDirectoryRelativeData(otherDir);
            MakeDirectoryRelativeData relativeDirData = new MakeDirectoryRelativeData(makeRelativeToThisDir.path);

            StringBuilder finalRelativatedPath = new StringBuilder();
            int flaggedIdx = -1;
            for(int idx = 0; idx < relativeDirData.directories.Count; ++idx)
            {
                if (!relativeDirData.directories[idx].Equals(otherDirRelativeData.directories[idx]))
                {
                    flaggedIdx= idx;
                }

                if (flaggedIdx != -1)
                {
                    finalRelativatedPath.Append("{0}")
                }
            }

            // C/Binaries/Game/Source/Public
            // C/Binaries/Game/Engine

            // ../../Engine

            // If they are not equal at all, we just return the absolute path.
            return referenceDir.path;            
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

            for (int idx = 0; idx < dir.path.Length; ++idx)
            {                
                char actualLetter = dir.path.ElementAt(idx);               
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

                directoryStr.Append(dir.path[idx]);

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
