using IshakBuildTool.ProjectFile;

namespace IshakBuildTool.Utils
{
    internal class FileUtils
    {
        public static bool FileExists(FileReference fileRef)
        {
            FileInfo fileInfo = new FileInfo(fileRef.Path);
            return fileInfo.Exists;
        }

        public static FileReference Combine(DirectoryReference dirRef, params string[] valuesToCombine)
        {
            if (valuesToCombine.Length == 0)
            {
                return FileReference.Null;
            }

            string combinedPath = dirRef.Path;
            combinedPath += DirectoryReference.DirectorySeparatorChar;
            for (int idx = 0; idx < valuesToCombine.Length; ++idx)
            {
                string value = valuesToCombine[idx];

                // If we find an empty value we just return the ref directory.
                if (value == string.Empty)
                {
                    return FileReference.Null;
                }

                if (idx == valuesToCombine.Length - 1)
                {
                    combinedPath += value;
                }
                else
                {
                    combinedPath += (value + DirectoryReference.DirectorySeparatorChar);
                }
            }


            return new FileReference(combinedPath);
        }
    }
}
