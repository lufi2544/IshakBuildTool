// Copyright(c) Juan Esteban Rayo Contreras. All rights reserved.

using IshakBuildTool.Configuration;
using IshakBuildTool.ProjectFile;
using IshakBuildTool.Test;
using IshakBuildTool.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

using IshakBuildTool.Project;


namespace IshakBuildTool.Build
{
    // Class for generating data for visual studio.
    // For now the engine will be only generating files for visual studio and windows platform.
    internal class VSProjectGenerator
    {

        enum EVCFileType
        {
            None,
            ClCompile,
            ClInclude                
        }

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
            engineSolutionFile = new Project.ProjectFile("IshakEngine", intermediatePath);
            engineSolutionFile.GUID = BuildGUIDForProject(engineSolutionFile.path, engineSolutionFile.projectName);
            
            WriteEngineSolutionFile();
          
            RootFolder.subSolutionFiles.Add(engineSolutionFile);
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
            engineProjectFileString = new StringBuilder();
            
            engineProjectFileString.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            engineProjectFileString.AppendLine("<Project DefaultTargets=\"Build\" ToolsVersion=\"0.17\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");
        }

        void WriteEngineProjectConfigurations()
        {
            engineProjectFileString.AppendLine("  <ItemGroup Label=\"ProjectConfigurations\">");

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

            engineProjectFileString.AppendLine("  </ItemGroup>");
        }


        void AddConfiguration(IshakEngineConfiguration config, Platform.EPlatform platform)
        {
            engineConfigurations.Add(new Tuple<string, Platform.EPlatform>(config.ToString(), platform));


            engineProjectFileString.AppendLine(string.Format("    <ProjectConfiguration Include=\"{0}|{1}\">", config.ToString(), platform.ToString()));
            engineProjectFileString.AppendLine(string.Format("      <Configuration>{0}</Configuration>", config.ToString()));
            engineProjectFileString.AppendLine(string.Format("      <Platform>{0}</Platform>", platform.ToString()));
            engineProjectFileString.AppendLine(string.Format("    </ProjectConfiguration>"));
        }

        void WriteEngineProjectGlobals()
        {           
            engineProjectFileString.AppendLine("  <PropertyGroup Label=\"Globals\">");            

            engineProjectFileString.AppendLine("    <ProjectGuid>{0}</ProjectGuid>", engineSolutionFile.GUID.ToString("B").ToUpperInvariant());
            engineProjectFileString.AppendLine("    <Keyword>MakeFileProj</Keyword>");
            engineProjectFileString.AppendLine("    <RootNamespace>{0}</RootNamespace>", engineSolutionFile.projectName);
            engineProjectFileString.AppendLine("    <PlatformToolset>{0}/PlatformToolset>", "v143" );
            engineProjectFileString.AppendLine("    <MinimumVisualStudioVersion>{0}</MinimumVisualStudioVersion>", "17.0");
            engineProjectFileString.AppendLine("    <VCProjectVersion>{0}</VCProjectVersion>", "17.0");
            engineProjectFileString.AppendLine("    <NMakeUseOemCodePage>true</NMakeUseOemCodePage>"); // Fixes mojibake with non-Latin character sets (UE-102825)
            engineProjectFileString.AppendLine("    <TargetRuntime>Native</TargetRuntime>");
            engineProjectFileString.AppendLine("  </PropertyGroup>");

        }   
        
        void WriteEnginePostDefaultProps()
        {
            engineProjectFileString.AppendLine("  <Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.Default.props\" />");

            foreach (Tuple<string, Platform.EPlatform> configurationTuple in engineConfigurations)
            {
                string configurationAndPlatformName = configurationTuple.Item1 + "|" + configurationTuple.Item2.ToString();
                string conditionString = "Condition=\"'$(Configuration)|$(Platform)'=='" + configurationAndPlatformName + "'\"";

                engineProjectFileString.AppendLine("  <PropertyGroup {0} Label=\"Configuration\">", conditionString);
                engineProjectFileString.AppendLine("    <ConfigurationType>{0}</ConfigurationType>", "Makefile");
                engineProjectFileString.AppendLine("    <PlatformToolset>{0}</PlatformToolset>", "v143");
            }
        }

        void WriteEngineProjectConfigurationsProps()
        {
            // TODO find a way of getting used configuration.

            foreach (var configurationTuple in engineConfigurations)
            {
                string configurationAndPlatformName = configurationTuple.Item1 + "|" + configurationTuple.Item2.ToString();
                string conditionString = "Condition=\"'$(Configuration)|$(Platform)'=='" + configurationAndPlatformName + "'\"";
                engineProjectFileString.AppendLine("  <ImportGroup {0} Label=\"PropertySheets\">", conditionString);
                engineProjectFileString.AppendLine("    <Import Project=\"$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props\" Condition=\"exists('$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props')\" Label=\"LocalAppDataPlatform\" />");
                engineProjectFileString.AppendLine("  </ImportGroup>");

                engineProjectFileString.AppendLine("  <PropertyGroup {0}>", conditionString);
                engineProjectFileString.AppendLine("    <IncludePath />");
                engineProjectFileString.AppendLine("    <ReferencePath />");
                engineProjectFileString.AppendLine("    <LibraryPath />");
                engineProjectFileString.AppendLine("    <LibraryWPath />");
                engineProjectFileString.AppendLine("    <SourcePath />");
                engineProjectFileString.AppendLine("    <ExcludePath />");

                string projectUnusedDirectory = "$(ProjectDir)..\\Build\\Unused";
                engineProjectFileString.AppendLine("    <OutDir>{0}{1}</OutDir>", projectUnusedDirectory, Path.DirectorySeparatorChar);
                engineProjectFileString.AppendLine("    <IntDir>{0}{1}</IntDir>", projectUnusedDirectory, Path.DirectorySeparatorChar);

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
            engineProjectFileString.AppendLine("    <AdditionalOptions>{0}</AdditionalOptions>", "std:c++17");
            engineProjectFileString.AppendLine("  </PropertyGroup>");
        }

        // In the case of a game, this would be the shared files from the engine.
        void WriteIntellisenseInfo()
        {
            engineProjectFileString.AppendLine("  <PropertyGroup>");


            // TODO make the include paths Set( in this case the Source file for the engine )

            string sharedIncludeDirectories = GetIncludePathSet();
            engineProjectFileString.AppendLine("    <IncludePath>$(IncludePath);{0}</IncludePath>", sharedIncludeDirectories);
            engineProjectFileString.AppendLine("    <NMakeForcedIncludes>$(NMakeForcedIncludes)</NMakeForcedIncludes>");
            engineProjectFileString.AppendLine("    <NMakeAssemblySearchPath>$(NMakeAssemblySearchPath)</NMakeAssemblySearchPath>");
            engineProjectFileString.AppendLine("    <AdditionalOptions>{0}</AdditionalOptions>", GetCppVersion(ECppVersion.Cpp17));
            engineProjectFileString.AppendLine("  </PropertyGroup>");


            WriteIntellisenseInfoForEngineFiles();
        }

        void WriteIntellisenseInfoForEngineFiles()
        {
            engineProjectFileString.AppendLine("  <ItemGroup>");

            // Here basically we iterate through all the engine source code files and we determine its way of adding 
            // to the compiler compile way.

            StringBuilder compilerCompileMacrosB = new StringBuilder();
            foreach (FileReference sourceFile in engineSolutionFile.sourceFiles)
            {
                EVCFileType vcCompileType = GetVCFileTypeForFile(sourceFile);

                WriteVCCompileTypeForStandardFile(vcCompileType, sourceFile);

                if (vcCompileType == EVCFileType.ClCompile)
                {
                    WriteVCCompileTypeSourceFile(sourceFile);
                }                
            }
            
        }
        void WriteVCCompileTypeForStandardFile(EVCFileType vcCompileType, FileReference file)
        {
            engineProjectFileString.AppendLine("    <{0} Include=\"{1}\"/>", vcCompileType.ToString(), file.Path);
        }

        void WriteVCCompileTypeSourceFile(FileReference file)
        {
            // Write the source file to the engineProjectFileStr and its additional source files for compiling this file, for now this second step
            // will not be necessary as there are only one folder for the engine project, but as we add modules this may be.
            // When implementing the additional module dependent files, we are gonna add them here.
        
            engineProjectFileString.AppendLine("      <AdditionalIncludeDirectories>$(NMakeIncludeSearchPath);{0}", GetDependencyFilesForSourceFile(file));
            engineProjectFileString.AppendLine("    </ClCompile>");
        }

        string GetDependencyFilesForSourceFile(FileReference file)
        {
            // Add a BuildEnviroment for a sourceFile and then the files from that build enviroment.
            // Module{ Modules{ return paths(private and public) }   };
            // For now we will just return the EngineSourceFileDir

            // This is fine for now as we are building the engine and all the source 
            string publicPrvivateDir = DirectoryUtils.GetPublicOrPrivateDirectoryPathFromDirectory(file.Directory);

            // TODO Delete once working.
            return publicPrvivateDir;
        }

        EVCFileType GetVCFileTypeForFile(FileReference file)
        {
            if (file.Path.Contains(".h"))
            {
                return EVCFileType.ClInclude;

            }else if (file.Path.Contains(".cpp"))
            {
                return EVCFileType.ClCompile;
            }

            return EVCFileType.None;
        }



        string GetCppVersion(ECppVersion version)
        {
            switch (version)
            {
                case ECppVersion.Cpp17:
                    return "/std:c++17";

                case ECppVersion.Cpp20:
                    return "/std:c++20";
            }

            return string.Empty;
        }

        string GetIncludePathSet()
        {
            StringBuilder includePathsStrBuilder = new StringBuilder();                      
          
            // As modules are added to the engine, we would have to change the way we scann the files and store them
            // but for now this should be fine.
            foreach (FileReference sourceFile in engineSolutionFile.sourceFiles)
            {                
                // For now we just support source files under the Private Directory.
                string directory = DirectoryUtils.GetPublicOrPrivateDirectoryPathFromDirectory(sourceFile.Directory);

                directory = DirectoryUtils.MakeRelativeTo(new DirectoryReference(directory), new DirectoryReference(engineSolutionFile.path));

                if (!includePathsStrBuilder.ToString().Contains(directory))
                {                    
                    includePathsStrBuilder.AppendLine(directory + ";");        
                }
            }           

             return includePathsStrBuilder.ToString();
        }

        public bool bEngineProjectCreated { get; set; }
        Project.ProjectFile engineSolutionFile { get; set; }


        StringBuilder engineProjectFileString;
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
