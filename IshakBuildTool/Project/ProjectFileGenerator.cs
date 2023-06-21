﻿// Copyright(c) Juan Esteban Rayo Contreras. All rights reserved.

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
        
        private Project ProjectToHandle;

        StringBuilder ProjectFileSB = new StringBuilder();
        StringBuilder ProjectFileFiltersSB = new StringBuilder();
        
        List<Tuple<string, Platform.EPlatform>>? EngineConfigurations;
        ProjectFileFilterGenerator? FilterGenerator = null;

        public ProjectFileGenerator(Project projectToHandle)
        {            
            ProjectToHandle= projectToHandle;
            FilterGenerator = new ProjectFileFilterGenerator(projectToHandle.ProjectFile);
        }

        public void HandleProjectFileGeneration()
        {
            CreateEngineSolutionFile();            
        }        

        public void CreateEngineSolutionFile()
        {
            // TODO Utils make this more accessible by adding this to a file.
            
            ProjectToHandle.GUID = GeneratorGlobals.BuildGUIDForProject(ProjectToHandle.ProjectFile.Path, ProjectToHandle.ProjectFile.SolutionProjectName);

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
            ProjectFileSB = new StringBuilder();

            ProjectFileSB.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            ProjectFileSB.AppendLine("<Project DefaultTargets=\"Build\" ToolsVersion=\"0.17\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");


            ProjectFileFiltersSB.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            ProjectFileFiltersSB.AppendLine("<Project ToolsVersion=\"17.0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");            


            // Init FilterGenerator
            FilterGenerator.Init();
        }

        void WriteEngineProjectConfigurations()
        {
            ProjectFileSB.AppendLine("  <ItemGroup Label=\"ProjectConfigurations\">");

            EngineConfigurations = new List<Tuple<string, Platform.EPlatform>>();

            // For now we are just gonna add a default configuration.
            AddConfiguration(IshakEngineConfiguration.Debug, Platform.EPlatform.x64);
            AddConfiguration(IshakEngineConfiguration.Development, Platform.EPlatform.x64);

            ProjectFileSB.AppendLine("  </ItemGroup>");
        }


        void AddConfiguration(IshakEngineConfiguration config, Platform.EPlatform platform)
        {
            EngineConfigurations.Add(new Tuple<string, Platform.EPlatform>(config.ToString(), platform));


            ProjectFileSB.AppendLine(string.Format("    <ProjectConfiguration Include=\"{0}|{1}\">", config.ToString(), platform.ToString()));
            ProjectFileSB.AppendLine(string.Format("      <Configuration>{0}</Configuration>", config.ToString()));
            ProjectFileSB.AppendLine(string.Format("      <Platform>{0}</Platform>", platform.ToString()));
            ProjectFileSB.AppendLine(string.Format("    </ProjectConfiguration>"));
        }

        void WriteEngineProjectGlobals()
        {
            ProjectFileSB.AppendLine("  <PropertyGroup Label=\"Globals\">");

            ProjectFileSB.AppendLine("    <ProjectGuid>{0}</ProjectGuid>",  ProjectToHandle.GUID.ToString("B").ToUpperInvariant());
            ProjectFileSB.AppendLine("    <Keyword>MakeFileProj</Keyword>");
            ProjectFileSB.AppendLine("    <RootNamespace>{0}</RootNamespace>", ProjectToHandle.ProjectFile.ProjectName);
            ProjectFileSB.AppendLine("    <PlatformToolset>{0}</PlatformToolset>", "v143");
            ProjectFileSB.AppendLine("    <MinimumVisualStudioVersion>{0}</MinimumVisualStudioVersion>", "17.0");
            ProjectFileSB.AppendLine("    <VCProjectVersion>{0}</VCProjectVersion>", "17.0");
            ProjectFileSB.AppendLine("    <NMakeUseOemCodePage>true</NMakeUseOemCodePage>"); // Fixes mojibake with non-Latin character sets (UE-102825)
            ProjectFileSB.AppendLine("    <TargetRuntime>Native</TargetRuntime>");
            ProjectFileSB.AppendLine("  </PropertyGroup>");

        }

        void WriteEnginePostDefaultProps()
        {
            ProjectFileSB.AppendLine("  <Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.Default.props\" />");

            foreach (Tuple<string, Platform.EPlatform> configurationTuple in EngineConfigurations)
            {
                string configurationAndPlatformName = configurationTuple.Item1 + "|" + configurationTuple.Item2.ToString();
                string conditionString = "Condition=\"'$(Configuration)|$(Platform)'=='" + configurationAndPlatformName + "'\"";

                ProjectFileSB.AppendLine("  <PropertyGroup {0} Label=\"Configuration\">", conditionString);
                ProjectFileSB.AppendLine("    <ConfigurationType>{0}</ConfigurationType>", "Makefile");
                ProjectFileSB.AppendLine("    <PlatformToolset>{0}</PlatformToolset>", "v143");
                ProjectFileSB.AppendLine("  </PropertyGroup>");
            }
        }

        void WriteEngineProjectConfigurationsProps()
        {
            // TODO find a way of getting used configuration.

            foreach (var configurationTuple in EngineConfigurations)
            {
                string configurationAndPlatformName = configurationTuple.Item1 + "|" + configurationTuple.Item2.ToString();
                string conditionString = "Condition=\"'$(Configuration)|$(Platform)'=='" + configurationAndPlatformName + "'\"";
                ProjectFileSB.AppendLine("  <ImportGroup {0} Label=\"PropertySheets\">", conditionString);
                ProjectFileSB.AppendLine("    <Import Project=\"$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props\" Condition=\"exists('$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props')\" Label=\"LocalAppDataPlatform\" />");
                ProjectFileSB.AppendLine("  </ImportGroup>");

                ProjectFileSB.AppendLine("  <PropertyGroup {0}>", conditionString);
                ProjectFileSB.AppendLine("    <IncludePath />");
                ProjectFileSB.AppendLine("    <ReferencePath />");
                ProjectFileSB.AppendLine("    <LibraryPath />");
                ProjectFileSB.AppendLine("    <LibraryWPath />");
                ProjectFileSB.AppendLine("    <SourcePath />");
                ProjectFileSB.AppendLine("    <ExcludePath />");

                string projectUnusedDirectory = "$(ProjectDir)..\\Build\\Unused";
                ProjectFileSB.AppendLine("    <OutDir>{0}{1}</OutDir>", projectUnusedDirectory, Path.DirectorySeparatorChar);
                ProjectFileSB.AppendLine("    <IntDir>{0}{1}</IntDir>", projectUnusedDirectory, Path.DirectorySeparatorChar);

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
            ProjectFileSB.AppendLine("    <AdditionalOptions>{0}</AdditionalOptions>", "std:c++17");
            ProjectFileSB.AppendLine("  </PropertyGroup>");
        }

        // In the case of a game, this would be the shared files from the engine.
        void WriteIntellisenseInfo()
        {
            ProjectFileSB.AppendLine("  <PropertyGroup>");

           
            string sharedIncludeDirectories = GetIncludePathSet();
            ProjectFileSB.AppendLine("    <IncludePath>$(IncludePath);{0}</IncludePath>", sharedIncludeDirectories);
            ProjectFileSB.AppendLine("    <NMakeForcedIncludes>$(NMakeForcedIncludes)</NMakeForcedIncludes>");
            ProjectFileSB.AppendLine("    <NMakeAssemblySearchPath>$(NMakeAssemblySearchPath)</NMakeAssemblySearchPath>");
            ProjectFileSB.AppendLine("    <AdditionalOptions>{0}</AdditionalOptions>", GetCppVersion(ECppVersion.Cpp17));
            ProjectFileSB.AppendLine("  </PropertyGroup>");


            WriteIntellisenseInfoForEngineFiles();
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

            // We should add the the engine source folder as a base include here.
            includePathsStrBuilder.Append(DirectoryUtils.GetEngineSourceFolder());
            return includePathsStrBuilder.ToString();
        }

        void WriteIntellisenseInfoForEngineFiles()
        {
            ProjectFileSB.AppendLine("  <ItemGroup>");

            WriteVCCompileDataFromProjectModules();

            ProjectFileSB.AppendLine("  </ItemGroup>");
        }

        /** Iterate through all the Modules SourceFiles that this project has and write the data from them for Intellisense.  */
        void WriteVCCompileDataFromProjectModules() 
        {
            foreach (Module module in ProjectToHandle.Modules)
            {
                WriteIntellisenseInfoFromModule(module);
            }

            FilterGenerator.Finish();
        }

        void WriteIntellisenseInfoFromModule(Module module)
        {
            foreach (FileReference sourceFileRef in module.SourceFiles)
            {
                EVCFileType vcCompileType = GeneratorGlobals.GetVCFileTypeForFile(sourceFileRef);
                                                
                if (vcCompileType == EVCFileType.ClCompile)
                {
                    WriteVCCompileTypeSourceFile(sourceFileRef, module);
                }
                else
                {
                    WriteStandardVCCompileTypeForFile(sourceFileRef, vcCompileType);
                }

                FilterGenerator.AddFile(sourceFileRef);
            }            
        }

        void WriteStandardVCCompileTypeForFile(FileReference file, EVCFileType cvFileType)
        {
            string fileRelativeToProjectFile = DirectoryUtils.MakeRelativeTo(file, ProjectToHandle.ProjectFile.GetProjectFileRef()).Path;

            ProjectFileSB.AppendLine("    <{0} Include=\"{1}\"/>", cvFileType.ToString(), fileRelativeToProjectFile);
        }

        void WriteVCCompileTypeSourceFile(FileReference file, Module fileParentModule)
        {
            // Write the source file to the engineProjectFileStr and its additional source files for compiling this file, for now this second step
            // will not be necessary as there are only one folder for the engine project, but as we add modules this may be.
            // When implementing the additional module dependent files, we are gonna add them here.


            string fileRelativeToProjectFile = DirectoryUtils.MakeRelativeTo(file, ProjectToHandle.ProjectFile.GetProjectFileRef()).Path;

            ProjectFileSB.AppendLine(
                "    <{0} Include=\"{1}\">",
                EVCFileType.ClCompile.ToString(), fileRelativeToProjectFile);

            ProjectFileSB.AppendLine(
                "      <AdditionalIncludeDirectories>$(NMakeIncludeSearchPath);{0};</AdditionalIncludeDirectories>",
                fileParentModule.ModulesDependencyDirsString + GetIncludePathSet());

            ProjectFileSB.AppendLine("    </ClCompile>");
        }

        void WriteSourcePathProperty()
        {
            ProjectFileSB.AppendLine("  <PropertyGroup>");
            ProjectFileSB.Append("    <SourcePath>");

            foreach (Module module in ProjectToHandle.Modules)
            {
                ProjectFileSB.Append("{0};", module.PrivateDirectoryRef);
            }

            ProjectFileSB.AppendLine("</SourcePath>");
            ProjectFileSB.AppendLine("  </PropertyGroup>");
        }

        void CreateProjectFile()
        {
            ProjectToHandle.ProjectFile.SetProjectFileContent(ProjectFileSB.ToString());
            ProjectToHandle.ProjectFile.Create();
        }

        void ImportFinalProjectFileArguments()
        {
            ProjectFileSB.AppendLine("  <Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.targets\" />");
            ProjectFileSB.AppendLine("  <ImportGroup Label=\"ExtensionTargets\">");
            ProjectFileSB.AppendLine("  </ImportGroup>");
            ProjectFileSB.AppendLine("</Project>");
        }

    }
}
