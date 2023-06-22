

using IshakBuildTool.Configuration;

namespace IshakBuildTool.Test
{
    // Making this class internal only
    internal class TestEnviroment
    {        
        public static string TestFolderPath = "C:\\IshakEngine";
        public static string TestProjectName = "IshakEngine";
        public static string TestFolderPathSource = "C:\\IshakEngine\\Source";
        public static string TestIntermediateFolder = TestFolderPath + "\\Intermediate";
        public static string TestProjectFilesFolder = TestIntermediateFolder + "\\ProjectFiles\\";
        public static IshakEngineConfiguration DefaultConfiguration = IshakEngineConfiguration.Debug;
        public static string DefaultConfigurationName = "DebugEngine";
        public static string DefaultEngineName = "IshakEngine";
        public static Platform.EPlatform DefaultPlatform = Platform.EPlatform.x64;
    }
}