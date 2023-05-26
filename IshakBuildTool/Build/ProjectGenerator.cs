// Copyright(c) Juan Esteban Rayo Contreras. All rights reserved.

using IshakBuildTool.Configuration;
using IshakBuildTool.ProjecFile;
using IshakBuildTool.ProjectFile;
using IshakBuildTool.Test;
using IshakBuildTool.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

using IshakBuildTool.ProjectFile;

namespace IshakBuildTool.Build
{
    // Class for generating data for visual studio.
    // For now the engine will be only generating files for visual studio and windows platform.
    internal class VSProjectGenerator
    {
        private ProjectDirectory RootFolder;

        public VSProjectGenerator()
        {
            RootFolder = new ProjectDirectory("Root", "");
            bEngineProjectCreated = false;
        }

        public void CreateEngineSolutionFile(string engineDirectoryPath)
        {
            // TODO Utils make this more accessible by adding this to a file.
            string intermediatePath = engineDirectoryPath + "\\Intermediate\\ProjectFiles";
            engineSolutionFile = new SolutionFile("IshakEngine", intermediatePath);
            engineSolutionFile.GUID = BuildGUIDForProject(engineSolutionFile.path, engineSolutionFile.projectName);

            SearchAndAddSourceFilesToProjectDirectory(engineSolutionFile, new ProjectDirectory("IshakEngineDir", engineDirectoryPath));
            WriteEngineSolutionFile();
          
            RootFolder.subSolutionFiles.Add(engineSolutionFile);
        }

        void SearchAndAddSourceFilesToProjectDirectory(SolutionFile solutionFile, ProjectDirectory directory)
        {
            List<FileReference> foundSourceFiles = FileScanner.FindSourceFiles(directory);
            solutionFile.sourceFiles = foundSourceFiles;
        }

        /* Basically after creating the Solution file, in this case what we do is just write in the solution file all the obtained data. */
        void WriteEngineSolutionFile()
        {
            WriteEngineHeaderFile();
            WriteEngineProjectConfigurations();
            WriteEngineProjectGlobals();
            WriteEnginePostDefaultProps();
            WriteEngineProjectConfigurationsProps();
            WriteIntellisenseInfo();
        }

        void WriteEngineHeaderFile()
        {
            engineSolutionFileString = new StringBuilder();
            
            engineSolutionFileString.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            engineSolutionFileString.AppendLine("<Project DefaultTargets=\"Build\" ToolsVersion=\"0.17\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");
        }

        void WriteEngineProjectConfigurations()
        {
            engineSolutionFileString.AppendLine("  <ItemGroup Label=\"ProjectConfigurations\">");

            /*
            string ProjectPlatformName = PlatformTuple.Item1;
            AppendLine("    <ProjectConfiguration Include=\"{0}|{1}\">", ProjectConfigurationName, ProjectPlatformName);
            AppendLine("      <Configuration>{0}</Configuration>", ProjectConfigurationName);
            AppendLine("      <Platform>{0}</Platform>", ProjectPlatformName);
            AppendLine("    </ProjectConfiguration>");
            */

            engineConfigurations = new List<Tuple<string, Platform.EPlatform>>();

            // For now we are just gonna add a default configuration.
            AddConfiguration(IshakEngineConfiguration.Debug, Platform.EPlatform.x64);
            AddConfiguration(IshakEngineConfiguration.Development, Platform.EPlatform.x64);

            engineSolutionFileString.AppendLine("  </ItemGroup>");
        }


        void AddConfiguration(IshakEngineConfiguration config, Platform.EPlatform platform)
        {
            engineConfigurations.Add(new Tuple<string, Platform.EPlatform>(config.ToString(), platform));


            engineSolutionFileString.AppendLine(string.Format("    <ProjectConfiguration Include=\"{0}|{1}\">", config.ToString(), platform.ToString()));
            engineSolutionFileString.AppendLine(string.Format("      <Configuration>{0}</Configuration>", config.ToString()));
            engineSolutionFileString.AppendLine(string.Format("      <Platform>{0}</Platform>", platform.ToString()));
            engineSolutionFileString.AppendLine(string.Format("    </ProjectConfiguration>"));
        }

        void WriteEngineProjectGlobals()
        {           
            engineSolutionFileString.AppendLine("  <PropertyGroup Label=\"Globals\">");            

            engineSolutionFileString.AppendLine("    <ProjectGuid>{0}</ProjectGuid>", engineSolutionFile.GUID.ToString("B").ToUpperInvariant());
            engineSolutionFileString.AppendLine("    <Keyword>MakeFileProj</Keyword>");
            engineSolutionFileString.AppendLine("    <RootNamespace>{0}</RootNamespace>", engineSolutionFile.projectName);
            engineSolutionFileString.AppendLine("    <PlatformToolset>{0}/PlatformToolset>", "v143" );
            engineSolutionFileString.AppendLine("    <MinimumVisualStudioVersion>{0}</MinimumVisualStudioVersion>", "17.0");
            engineSolutionFileString.AppendLine("    <VCProjectVersion>{0}</VCProjectVersion>", "17.0");
            engineSolutionFileString.AppendLine("    <NMakeUseOemCodePage>true</NMakeUseOemCodePage>"); // Fixes mojibake with non-Latin character sets (UE-102825)
            engineSolutionFileString.AppendLine("    <TargetRuntime>Native</TargetRuntime>");
            engineSolutionFileString.AppendLine("  </PropertyGroup>");

        }   
        
        void WriteEnginePostDefaultProps()
        {
            engineSolutionFileString.AppendLine("  <Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.Default.props\" />");

            foreach (Tuple<string, Platform.EPlatform> configurationTuple in engineConfigurations)
            {
                string configurationAndPlatformName = configurationTuple.Item1 + "|" + configurationTuple.Item2.ToString();
                string conditionString = "Condition=\"'$(Configuration)|$(Platform)'=='" + configurationAndPlatformName + "'\"";

                engineSolutionFileString.AppendLine("  <PropertyGroup {0} Label=\"Configuration\">", conditionString);
                engineSolutionFileString.AppendLine("    <ConfigurationType>{0}</ConfigurationType>", "Makefile");
                engineSolutionFileString.AppendLine("    <PlatformToolset>{0}</PlatformToolset>", "v143");
            }
        }

        void WriteEngineProjectConfigurationsProps()
        {
            // TODO find a way of getting used configuration.

            foreach (var configurationTuple in engineConfigurations)
            {
                string configurationAndPlatformName = configurationTuple.Item1 + "|" + configurationTuple.Item2.ToString();
                string conditionString = "Condition=\"'$(Configuration)|$(Platform)'=='" + configurationAndPlatformName + "'\"";
                engineSolutionFileString.AppendLine("  <ImportGroup {0} Label=\"PropertySheets\">", conditionString);
                engineSolutionFileString.AppendLine("    <Import Project=\"$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props\" Condition=\"exists('$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props')\" Label=\"LocalAppDataPlatform\" />");
                engineSolutionFileString.AppendLine("  </ImportGroup>");

                engineSolutionFileString.AppendLine("  <PropertyGroup {0}>", conditionString);
                engineSolutionFileString.AppendLine("    <IncludePath />");
                engineSolutionFileString.AppendLine("    <ReferencePath />");
                engineSolutionFileString.AppendLine("    <LibraryPath />");
                engineSolutionFileString.AppendLine("    <LibraryWPath />");
                engineSolutionFileString.AppendLine("    <SourcePath />");
                engineSolutionFileString.AppendLine("    <ExcludePath />");

                string projectUnusedDirectory = "$(ProjectDir)..\\Build\\Unused";
                engineSolutionFileString.AppendLine("    <OutDir>{0}{1}</OutDir>", projectUnusedDirectory, Path.DirectorySeparatorChar);
                engineSolutionFileString.AppendLine("    <IntDir>{0}{1}</IntDir>", projectUnusedDirectory, Path.DirectorySeparatorChar);

                WriteNMakeBuildProps();
            }

        }

        // Writes the bat file to use when compiling the solution.
        void WriteNMakeBuildProps()
        {
            // TODO make a proper implementation for compiling.

            /*
            engineSolutionFileString.AppendLine("    <NMakeBuildCommandLine>{0} {1}</NMakeBuildCommandLine>", EscapePath(NormalizeProjectPath(Builder.BuildScript)), BuildArguments);
            engineSolutionFileString.AppendLine("    <NMakeReBuildCommandLine>{0} {1}</NMakeReBuildCommandLine>", EscapePath(NormalizeProjectPath(Builder.RebuildScript)), BuildArguments);
            engineSolutionFileString.AppendLine("    <NMakeCleanCommandLine>{0} {1}</NMakeCleanCommandLine>", EscapePath(NormalizeProjectPath(Builder.CleanScript)), BuildArguments);
            engineSolutionFileString.AppendLine("    <NMakeOutput>{0}</NMakeOutput>", NormalizeProjectPath(NMakePath.FullName));
            */

            // TODO make language standard
            engineSolutionFileString.AppendLine("    <AdditionalOptions>{0}</AdditionalOptions>", "std:c++17");
            engineSolutionFileString.AppendLine("  </PropertyGroup>");
        }

        // In the case of a game, this would be the shared files from the engine.
        void WriteIntellisenseInfo()
        {
            engineSolutionFileString.AppendLine("  <PropertyGroup>");


            // TODO make the include paths Set( in this case the Source file for the engine )

            string sharedIncludeDirectories = GetIncludePathSet();
            engineSolutionFileString.AppendLine("    <IncludePath>$(IncludePath);{0}</IncludePath>");
            


        }

        string GetIncludePathSet()
        {
            StringBuilder includePathsStrBuilder = new StringBuilder();
            // Get the Directory Paths For including in the project
            // .
            // 

            // Absolute path for including in the Include Category
          
            foreach (FileReference sourceFile in engineSolutionFile.sourceFiles)
            {
                string directory = DirectoryUtils.GetPublicOrPrivateDirectoryPathFromDirectory(sourceFile.GetDirectory());

                if (!includePathsStrBuilder.ToString().Contains(directory))
                {                    
                    includePathsStrBuilder.AppendLine(directory + ";");        
                }
            }
            

             return includePathsStrBuilder.ToString();
        }

        public bool bEngineProjectCreated { get; set; }
        SolutionFile engineSolutionFile { get; set; }
        StringBuilder engineSolutionFileString;
        List<Tuple<string, Platform.EPlatform>> engineConfigurations;


        public void GenerateProjectFiles(string projectPath)
        {
            var projectName = TestEnviroment.TestProjectName;
           
            // Configure
            // Create the c++ project
            var vsSolutionProject = projectName + ".sln";

            string solutionPath = projectPath + vsSolutionProject;

            StringBuilder VSSolutionFileContent = new StringBuilder();

            // TODO Create the visual studio Content
            VSSolutionFileContent.AppendLine();
            VSSolutionFileContent.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
            VSSolutionFileContent.AppendLine("# Visual Studio Version 17");
            VSSolutionFileContent.AppendLine("VisualStudioVersion = 17.0.31314.256");
            VSSolutionFileContent.AppendLine("MinimumVisualStudioVersion = 10.0.40219.1");
            

            var solutionGUID = BuildGUIDForProject("ISHE", "Engine");
            string FolderGUID = solutionGUID.ToString();
            VSSolutionFileContent.AppendLine("Project(\"" + BuildGlobals.VSSolutionHash + "\") = \"" + "Engine" + "\", \"" + "Engine" + "\", \"" + FolderGUID + "\"");
            VSSolutionFileContent.AppendLine("EndProject");


            string CppProjectType = "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}";

            var engineGUID = BuildGUIDForProject("ISHE", projectName);
            string FolderGUID2 = engineGUID.ToString();

            string VSProjectCustomExtension = BuildGlobals.DefaultIntermediateFolder + projectName + BuildGlobals.VSProjectExtension;

            VSSolutionFileContent.AppendLine("Project(\"" + CppProjectType + "\") = \"" + projectName + "\", \"" + VSProjectCustomExtension + "\", \"" + FolderGUID2 + "\"");
            VSSolutionFileContent.AppendLine("EndProject");


            VSSolutionFileContent.AppendLine("Global");
            VSSolutionFileContent.AppendLine("	GlobalSection(NestedProjects) = preSolution");
            VSSolutionFileContent.AppendLine("		" + engineGUID.ToString("B").ToUpperInvariant() + " = " + solutionGUID.ToString("B"));

            VSSolutionFileContent.AppendLine("	EndGlobalSection");

            VSSolutionFileContent.AppendLine("EndGlobal");

            

            CreateVisualStudioSolutionFile(solutionPath, VSSolutionFileContent.ToString());
            CreateVSFile();
        }

        private void AddProjectHirarchy(ref StringBuilder out_VSSolution)
        {
          
        }

        private void CreateVSFile()
        {
            // General Stuff

            // Project file header

            StringBuilder VCProjectFileContent = new StringBuilder();
            
            VCProjectFileContent.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            VCProjectFileContent.AppendLine("<Project DefaultTargets=\"Build\" ToolsVersion=\"17.0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");
            VCProjectFileContent.AppendLine("  <ItemGroup Label=\"ProjectConfigurations\">");


            // Configurations

            VCProjectFileContent.AppendLine("    <ProjectConfiguration Include=\"DebugEngine|x64\">");
            VCProjectFileContent.AppendLine("      <Configuration>DebugEngine</Configuration>");
            VCProjectFileContent.AppendLine("      <Platform>x64</Platform>");
            VCProjectFileContent.AppendLine("    </ProjectConfiguration>");

            VCProjectFileContent.AppendLine("  </ItemGroup>");
            VCProjectFileContent.AppendLine("</Project>");


            CreateDirectory(Test.TestEnviroment.TestIntermediateFolder + "IshakEngine.vcxproj", VCProjectFileContent.ToString());
        }

        private Guid BuildGUIDForProject(string parentPath, string projectName)
        {
            string PathForMakingGUID = String.Format("{0}/{1}", parentPath, projectName);

            return MakeMd5Guid(Encoding.UTF8.GetBytes(PathForMakingGUID));
        }

        static Guid MakeMd5Guid(byte[] Input)
        {
            byte[] Hash = MD5.Create().ComputeHash(Input);
            Hash[6] = (byte)(0x30 | (Hash[6] & 0x0f)); // 0b0011'xxxx Version 3 UUID (MD5)
            Hash[8] = (byte)(0x80 | (Hash[8] & 0x3f)); // 0b10xx'xxxx RFC 4122 UUID
            Array.Reverse(Hash, 0, 4);
            Array.Reverse(Hash, 4, 2);
            Array.Reverse(Hash, 6, 2);
            return new Guid(Hash);
        }

        public void CreateVisualStudioSolutionFile(string filePath, string content)
        {

            CreateDirectory(filePath, content);
        }

        private void CreateDirectory(string filePath, string content)
        {
            bool bFileExists = File.Exists(filePath);

            if (bFileExists)
            {
                //  for now let's do nothing here.
            }
            else
            {
                //create the solution

                var info = Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                File.WriteAllText(filePath, content, Encoding.UTF8);

            }
        }

    }
}
