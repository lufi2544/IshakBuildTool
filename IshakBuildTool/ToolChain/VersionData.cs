

namespace IshakBuildTool.ToolChain
{
    public struct VersionData
    {
        public int Version { get; set; } = 0;
        public string VersionStr { get; set; } = string.Empty;
        
        int[]? VersionContainer = null;

        public VersionData()
        {
            Version = 0;
            VersionStr = string.Empty;            
        }

        public VersionData(params int[] versionsParam)
        {
            VersionContainer = versionsParam;
        }

        public VersionData(string version)
        {
            SetVersion(version);
        }

        public int GetVersionNumberFromContainer(int idx)
        {
            return VersionContainer[idx];
        }

        public void SetVersion(string versionToSet)
        {
            VersionStr = versionToSet;

            string cachedVersion = string.Empty;
            foreach (var letter in VersionStr)
            {
                if (letter == '.')
                {
                    break;
                }
                cachedVersion += letter;
            }

            Version = int.Parse(cachedVersion);
        }
    }

}
