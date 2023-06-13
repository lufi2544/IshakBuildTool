// Copyright(c) Juan Esteban Rayo Contreras. All rights reserved.

using IshakBuildTool.Configuration;
using IshakBuildTool.ProjectFile;
using IshakBuildTool.Test;
using IshakBuildTool.Utils;
using System.Security.Cryptography;
using System.Text;
using IshakBuildTool.Build;
using IshakBuildTool.Project.Modules;

namespace IshakBuildTool.Project
{
    // Class for generating data for visual studio.
    // For now the engine will be only generating files for visual studio and windows platform.
    internal class ProjectFileGenerator
    {
        enum EVCFileType
        {
            None,
            ClCompile,
            ClInclude
        }
        
        private Project ProjectToHandle;

        StringBuilder EngineProjectFileString;
        List<Tuple<string, Platform.EPlatform>>? EngineConfigurations;

        public ProjectFileGenerator(Project projectToHandle)
        {
            EngineProjectFileString = new StringBuilder();
            ProjectToHandle= projectToHandle;
        }

        public void HandleProjectFileGeneration()
        {
            CreateEngineSolutionFile();            
        }        

        public void CreateEngineSolutionFile()
        {
            // TODO Utils make this more accessible by adding this to a file.
            
            ProjectToHandle.GUID = BuildGUIDForProject(ProjectToHandle.ProjectFile.Path, ProjectToHandle.ProjectFile.SolutionProjectName);

            WriteEngineSolutionFile();
            
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
            WriteSourcePathProperty(); 
            ImportFinalProjectFileArguments();
            CreateProjectFile();
        }

       

        void WriteEngineHeaderFile()
        {
            EngineProjectFileString = new StringBuilder();

            EngineProjectFileString.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            EngineProjectFileString.AppendLine("<Project DefaultTargets=\"Build\" ToolsVersion=\"0.17\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");
        }

        void WriteEngineProjectConfigurations()
        {
            EngineProjectFileString.AppendLine("  <ItemGroup Label=\"ProjectConfigurations\">");

            EngineConfigurations = new List<Tuple<string, Platform.EPlatform>>();

            // For now we are just gonna add a default configuration.
            AddConfiguration(IshakEngineConfiguration.Debug, Platform.EPlatform.x64);
            AddConfiguration(IshakEngineConfiguration.Development, Platform.EPlatform.x64);

            EngineProjectFileString.AppendLine("  </ItemGroup>");
        }


        void AddConfiguration(IshakEngineConfiguration config, Platform.EPlatform platform)
        {
            EngineConfigurations.Add(new Tuple<string, Platform.EPlatform>(config.ToString(), platform));


            EngineProjectFileString.AppendLine(string.Format("    <ProjectConfiguration Include=\"{0}|{1}\">", config.ToString(), platform.ToString()));
            EngineProjectFileString.AppendLine(string.Format("      <Configuration>{0}</Configuration>", config.ToString()));
            EngineProjectFileString.AppendLine(string.Format("      <Platform>{0}</Platform>", platform.ToString()));
            EngineProjectFileString.AppendLine(string.Format("    </ProjectConfiguration>"));
        }

        void WriteEngineProjectGlobals()
        {
            EngineProjectFileString.AppendLine("  <PropertyGroup Label=\"Globals\">");

            EngineProjectFileString.AppendLine("    <ProjectGuid>{0}</ProjectGuid>",  ProjectToHandle.GUID.ToString("B").ToUpperInvariant());
            EngineProjectFileString.AppendLine("    <Keyword>MakeFileProj</Keyword>");
            EngineProjectFileString.AppendLine("    <RootNamespace>{0}</RootNamespace>", ProjectToHandle.ProjectFile.ProjectName);
            EngineProjectFileString.AppendLine("    <PlatformToolset>{0}</PlatformToolset>", "v143");
            EngineProjectFileString.AppendLine("    <MinimumVisualStudioVersion>{0}</MinimumVisualStudioVersion>", "17.0");
            EngineProjectFileString.AppendLine("    <VCProjectVersion>{0}</VCProjectVersion>", "17.0");
            EngineProjectFileString.AppendLine("    <NMakeUseOemCodePage>true</NMakeUseOemCodePage>"); // Fixes mojibake with non-Latin character sets (UE-102825)
            EngineProjectFileString.AppendLine("    <TargetRuntime>Native</TargetRuntime>");
            EngineProjectFileString.AppendLine("  </PropertyGroup>");

        }

        void WriteEnginePostDefaultProps()
        {
            EngineProjectFileString.AppendLine("  <Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.Default.props\" />");

            foreach (Tuple<string, Platform.EPlatform> configurationTuple in EngineConfigurations)
            {
                string configurationAndPlatformName = configurationTuple.Item1 + "|" + configurationTuple.Item2.ToString();
                string conditionString = "Condition=\"'$(Configuration)|$(Platform)'=='" + configurationAndPlatformName + "'\"";

                EngineProjectFileString.AppendLine("  <PropertyGroup {0} Label=\"Configuration\">", conditionString);
                EngineProjectFileString.AppendLine("    <ConfigurationType>{0}</ConfigurationType>", "Makefile");
                EngineProjectFileString.AppendLine("    <PlatformToolset>{0}</PlatformToolset>", "v143");
                EngineProjectFileString.AppendLine("  </PropertyGroup>");
            }
        }

        void WriteEngineProjectConfigurationsProps()
        {
            // TODO find a way of getting used configuration.

            foreach (var configurationTuple in EngineConfigurations)
            {
                string configurationAndPlatformName = configurationTuple.Item1 + "|" + configurationTuple.Item2.ToString();
                string conditionString = "Condition=\"'$(Configuration)|$(Platform)'=='" + configurationAndPlatformName + "'\"";
                EngineProjectFileString.AppendLine("  <ImportGroup {0} Label=\"PropertySheets\">", conditionString);
                EngineProjectFileString.AppendLine("    <Import Project=\"$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props\" Condition=\"exists('$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props')\" Label=\"LocalAppDataPlatform\" />");
                EngineProjectFileString.AppendLine("  </ImportGroup>");

                EngineProjectFileString.AppendLine("  <PropertyGroup {0}>", conditionString);
                EngineProjectFileString.AppendLine("    <IncludePath />");
                EngineProjectFileString.AppendLine("    <ReferencePath />");
                EngineProjectFileString.AppendLine("    <LibraryPath />");
                EngineProjectFileString.AppendLine("    <LibraryWPath />");
                EngineProjectFileString.AppendLine("    <SourcePath />");
                EngineProjectFileString.AppendLine("    <ExcludePath />");

                string projectUnusedDirectory = "$(ProjectDir)..\\Build\\Unused";
                EngineProjectFileString.AppendLine("    <OutDir>{0}{1}</OutDir>", projectUnusedDirectory, Path.DirectorySeparatorChar);
                EngineProjectFileString.AppendLine("    <IntDir>{0}{1}</IntDir>", projectUnusedDirectory, Path.DirectorySeparatorChar);

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
            EngineProjectFileString.AppendLine("    <AdditionalOptions>{0}</AdditionalOptions>", "std:c++17");
            EngineProjectFileString.AppendLine("  </PropertyGroup>");
        }

        // In the case of a game, this would be the shared files from the engine.
        void WriteIntellisenseInfo()
        {
            EngineProjectFileString.AppendLine("  <PropertyGroup>");

           
            string sharedIncludeDirectories = GetIncludePathSet();
            EngineProjectFileString.AppendLine("    <IncludePath>$(IncludePath);{0}</IncludePath>", sharedIncludeDirectories);
            EngineProjectFileString.AppendLine("    <NMakeForcedIncludes>$(NMakeForcedIncludes)</NMakeForcedIncludes>");
            EngineProjectFileString.AppendLine("    <NMakeAssemblySearchPath>$(NMakeAssemblySearchPath)</NMakeAssemblySearchPath>");
            EngineProjectFileString.AppendLine("    <AdditionalOptions>{0}</AdditionalOptions>", GetCppVersion(ECppVersion.Cpp17));
            EngineProjectFileString.AppendLine("  </PropertyGroup>");


            WriteIntellisenseInfoForEngineFiles();
        }

        string GetIncludePathSet()
        {
            StringBuilder includePathsStrBuilder = new StringBuilder();

            // We should add the the engine source folder as a base include here.
            includePathsStrBuilder.Append(DirectoryUtils.GetEngineSourceFolder());
            return includePathsStrBuilder.ToString();
        }

        void WriteIntellisenseInfoForEngineFiles()
        {
            EngineProjectFileString.AppendLine("  <ItemGroup>");

            WriteVCCompileDataFromProjectModules();
            EngineProjectFileString.AppendLine("  </ItemGroup>");
        }

        /** Iterate through all the Modules SourceFiles that this project has and write the data from them for Intellisense.  */
        void WriteVCCompileDataFromProjectModules() 
        {
            foreach (Module module in ProjectToHandle.Modules)
            {
                WriteIntellisenseInfoFromModule(module);
            }
        }

        void WriteIntellisenseInfoFromModule(Module module)
        {
            foreach (FileReference sourceFileRef in module.SourceFiles)
            {
                EVCFileType vcCompileType = GetVCFileTypeForFile(sourceFileRef);
                                                
                if (vcCompileType == EVCFileType.ClCompile)
                {
                    WriteVCCompileTypeSourceFile(sourceFileRef, module);
                }
                else
                {
                    WriteStandardVCCompileTypeForFile(sourceFileRef, vcCompileType);
                }

            }
        }

        void WriteStandardVCCompileTypeForFile(FileReference file, EVCFileType cvFileType)
        {
            EngineProjectFileString.AppendLine("    <{0} Include=\"{1}\"/>", cvFileType.ToString(), file.Path);
        }

        void WriteVCCompileTypeSourceFile(FileReference file, Module fileParentModule)
        {
            // Write the source file to the engineProjectFileStr and its additional source files for compiling this file, for now this second step
            // will not be necessary as there are only one folder for the engine project, but as we add modules this may be.
            // When implementing the additional module dependent files, we are gonna add them here.
            EngineProjectFileString.AppendLine("    <{0} Include=\"{1}\">", EVCFileType.ClCompile.ToString(), file.Path);
            EngineProjectFileString.AppendLine("      <AdditionalIncludeDirectories>$(NMakeIncludeSearchPath);{0};</AdditionalIncludeDirectories>", fileParentModule.ModulesDependencyDirsString + GetIncludePathSet());
            EngineProjectFileString.AppendLine("    </ClCompile>");
        }

        void WriteSourcePathProperty()
        {
            EngineProjectFileString.AppendLine("  <PropertyGroup>");
            EngineProjectFileString.Append("    <SourcePath>");

            foreach (Module module in ProjectToHandle.Modules)
            {
                EngineProjectFileString.Append("{0};", module.PrivateDirectoryRef);
            }

            EngineProjectFileString.AppendLine("</SourcePath>");
            EngineProjectFileString.AppendLine("  </PropertyGroup>");
        }

        void CreateProjectFile()
        {
            ProjectToHandle.ProjectFile.SetProjectFileContent(EngineProjectFileString.ToString());
            ProjectToHandle.ProjectFile.Create();
        }

        EVCFileType GetVCFileTypeForFile(FileReference file)
        {
            if (file.Path.Contains(".h"))
            {
                return EVCFileType.ClInclude;

            }
            else if (file.Path.Contains(".cpp"))
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

        void ImportFinalProjectFileArguments()
        {
            EngineProjectFileString.AppendLine("  <Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.targets\" />");
            EngineProjectFileString.AppendLine("  <ImportGroup Label=\"ExtensionTargets\">");
            EngineProjectFileString.AppendLine("  </ImportGroup>");
            EngineProjectFileString.AppendLine("</Project>");
        }

        private Guid BuildGUIDForProject(string projectPath, string projectName)
        {
            string PathForMakingGUID = string.Format("{0}/{1}", projectPath, projectName);
            return MakeMd5Guid(Encoding.UTF8.GetBytes(PathForMakingGUID));
        }

        static Guid MakeMd5Guid(byte[] Input)
        {
            byte[] Hash = MD5.Create().ComputeHash(Input);
            Hash[6] = (byte)(0x30 | Hash[6] & 0x0f); // 0b0011'xxxx Version 3 UUID (MD5)
            Hash[8] = (byte)(0x80 | Hash[8] & 0x3f); // 0b10xx'xxxx RFC 4122 UUID
            Array.Reverse(Hash, 0, 4);
            Array.Reverse(Hash, 4, 2);
            Array.Reverse(Hash, 6, 2);
            return new Guid(Hash);
        }

    }
}
