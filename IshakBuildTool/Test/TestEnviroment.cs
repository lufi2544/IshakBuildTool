

using IshakBuildTool.Configuration;

namespace IshakBuildTool.Test
{
    // Making this class internal only
    internal class TestEnviroment
    {        
        // ProjectGlobals
        public static string TestProjectName = "IshakEngine";
        public static string DefaultConfigurationName = "DebugEngine";
        public static string DefaultEngineName = "IshakEngine";
        public static IshakEngineConfiguration DefaultConfiguration = IshakEngineConfiguration.Debug;
        public static Platform.EPlatform DefaultPlatform = Platform.EPlatform.x64;

        public static string TestCommandLineArgs = "C:\\IshakEngine";
        public static string TestProjectType = "Application";
    }
}