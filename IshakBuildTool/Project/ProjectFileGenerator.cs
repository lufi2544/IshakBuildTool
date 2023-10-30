// Copyright(c) Juan Esteban Rayo Contreras. All rights reserved.

using IshakBuildTool.Configuration;
using IshakBuildTool.ProjectFile;
using IshakBuildTool.Utils;
using System.Text;
using IshakBuildTool.Build;
using IshakBuildTool.Project.Modules;
using IshakBuildTool.Platform;

namespace IshakBuildTool.Project
{
    // Class for generating data for visual studio.
    // For now the engine will be only generating files for visual studio and windows platform.
    internal class ProjectFileGenerator
    {

        IshakProject ProjectToHandle;
        List<Tuple<string, Platform.EPlatformArchitecture>>? EngineConfigurations;
        ProjectFileFilterGenerator? FilterGenerator = null;

        protected StringBuilder ProjectFileSB = new StringBuilder();        

        public ProjectFileGenerator(IshakProject projectToHandle)
        {
            ProjectToHandle = projectToHandle;
            FilterGenerator = new ProjectFileFilterGenerator(projectToHandle.ProjectFile);
        }

        bool IsGeneratingEngine()
        {
            return ProjectToHandle.Name != "IshakEngine";
        }

        public void HandleProjectFileGeneration()
        {
            CreateEngineSolutionFile();
        }

        public void CreateEngineSolutionFile()
        {
            // TODO Utils make this more accessible by adding this to a file.

            ProjectToHandle.SetGUID(GeneratorGlobals.BuildGUID(ProjectToHandle.ProjectFile.Path, ProjectToHandle.ProjectFile.SolutionProjectName));

            // Init FilterGenerator
            FilterGenerator.Init();
            WriteEngineSolutionFile();

        }

        /* Basically after creating the Solution file, in this case what we do is just write in the solution file all the obtained data. */
        protected virtual void WriteEngineSolutionFile()
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
            ProjectFileSB.AppendLine("<Project DefaultTargets=\"Build\" ToolsVersion=\"17.0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");                       
        }

        void WriteEngineProjectConfigurations()
        {
            ProjectFileSB.AppendLine("  <ItemGroup Label=\"ProjectConfigurations\">");

            EngineConfigurations = new List<Tuple<string, Platform.EPlatformArchitecture>>();

            // For now we are just gonna add a default configuration.
            if (IsGeneratingEngine())
            {
                AddConfiguration(IshakEngineConfiguration.BuildWithIhshakBuildTool, Platform.EPlatformArchitecture.x64);
                AddConfiguration(IshakEngineConfiguration.Invalid, Platform.EPlatformArchitecture.x64);
            }
            else
            {
                AddConfiguration(IshakEngineConfiguration.Debug, Platform.EPlatformArchitecture.x64);
                AddConfiguration(IshakEngineConfiguration.Development, Platform.EPlatformArchitecture.x64);
            }

            ProjectFileSB.AppendLine("  </ItemGroup>");
        }


        void AddConfiguration(IshakEngineConfiguration config, Platform.EPlatformArchitecture platform)
        {
            EngineConfigurations.Add(new Tuple<string, Platform.EPlatformArchitecture>(config.ToString(), platform));


            ProjectFileSB.AppendLine(string.Format("    <ProjectConfiguration Include=\"{0}|{1}\">", config.ToString(), platform.ToString()));
            ProjectFileSB.AppendLine(string.Format("      <Configuration>{0}</Configuration>", config.ToString()));
            ProjectFileSB.AppendLine(string.Format("      <Platform>{0}</Platform>", platform.ToString()));
            ProjectFileSB.AppendLine(string.Format("    </ProjectConfiguration>"));
        }

        void WriteEngineProjectGlobals()
        {
            ProjectFileSB.AppendLine("  <PropertyGroup Label=\"Globals\">");

            ProjectFileSB.AppendLine("    <ProjectGuid>{0}</ProjectGuid>", ProjectToHandle.GetGUID());
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

            
            foreach (Tuple<string, Platform.EPlatformArchitecture> configurationTuple in EngineConfigurations)
            {
                string configurationAndPlatformName = configurationTuple.Item1 + "|" + configurationTuple.Item2.ToString();
                string conditionString = "Condition=\"'$(Configuration)|$(Platform)'=='" + configurationAndPlatformName + "'\"";

                ProjectFileSB.AppendLine("  <PropertyGroup {0} Label=\"Configuration\">", conditionString);                
                ProjectFileSB.AppendLine("    <ConfigurationType>{0}</ConfigurationType>", GetAppType());
                ProjectFileSB.AppendLine("    <PlatformToolset>{0}</PlatformToolset>", "v143");
                ProjectFileSB.AppendLine("  </PropertyGroup>");
            }

            ProjectFileSB.AppendLine("  <Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.props\" />");
            ProjectFileSB.AppendLine("  <ImportGroup Label=\"ExtensionSettings\" />");
            ProjectFileSB.AppendLine("  <PropertyGroup Label=\"UserMacros\" />");

        }

        string GetAppType()
        {
            return BuildProjectManager.GetInstance().GetProjectDirectoryParams().ProjectType;            
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

                EntireProjectDirectoryParams projectDirectoryParams = BuildProjectManager.GetInstance().GetProjectDirectoryParams();


                // TODO REVIEW Uneal does this with an Unused directory, maybe I should handle this the same way? so I do not
                // have to touch the engine .exe ? ... to study.
                ProjectFileSB.AppendLine("    <OutDir>{0}</OutDir>", projectDirectoryParams.BinaryDir);
                ProjectFileSB.AppendLine("    <IntDir>{0}</IntDir>", projectDirectoryParams.BinaryDir);

                WriteNMakeBuildProps();
            }

        }

        // Writes the bat file to use when compiling the solution.
        void WriteNMakeBuildProps()
        {
            if (IsGeneratingEngine())
            {
                EntireProjectDirectoryParams projectDirectoryParams = BuildProjectManager.GetInstance().GetProjectDirectoryParams();
                // For now I will just call the script without any additional args.
                ProjectFileSB.AppendLine("    <NMakeBuildCommandLine>@rem Nothing to do.</NMakeBuildCommandLine>");
                ProjectFileSB.AppendLine("    <NMakeReBuildCommandLine>@rem Nothing to do.</NMakeReBuildCommandLine>");

                // TODO Add Clean Script
                //ProjectFileSB.AppendLine("    <NMakeCleanCommandLine>{0} {1}</NMakeCleanCommandLine>", EscapePath(NormalizeProjectPath(Builder.CleanScript)), BuildArguments);
                ProjectFileSB.AppendLine("    <NMakeOutput>@rem Nothing to do.</NMakeOutput>");
            }
            else
            {

                EntireProjectDirectoryParams projectDirectoryParams = BuildProjectManager.GetInstance().GetProjectDirectoryParams();
                // For now I will just call the script without any additional args.
                ProjectFileSB.AppendLine("    <NMakeBuildCommandLine>{0}</NMakeBuildCommandLine>", projectDirectoryParams.CompileEngineScriptPath);
                ProjectFileSB.AppendLine("    <NMakeReBuildCommandLine>{0}</NMakeReBuildCommandLine>", projectDirectoryParams.CompileEngineScriptPath);

                // TODO Add Clean Script
                //ProjectFileSB.AppendLine("    <NMakeCleanCommandLine>{0} {1}</NMakeCleanCommandLine>", EscapePath(NormalizeProjectPath(Builder.CleanScript)), BuildArguments);
                ProjectFileSB.AppendLine("    <NMakeOutput>{0}</NMakeOutput>", projectDirectoryParams.EngineExecutablePath);


                // TODO make language standard
                ProjectFileSB.AppendLine("    <AdditionalOptions>{0}</AdditionalOptions>", "/std:c++17");
            }
            ProjectFileSB.AppendLine("  </PropertyGroup>");
        }

        // In the case of a game, this would be the shared files from the engine.
        void WriteIntellisenseInfo()
        {

            if (!IsGeneratingEngine())
            {
                ProjectFileSB.AppendLine("  <PropertyGroup>");


                string sharedIncludeDirectories = GetSharedIncludeFilesPaths();
                ProjectFileSB.AppendLine("    <IncludePath>$(IncludePath);{0}</IncludePath>", sharedIncludeDirectories);
                ProjectFileSB.AppendLine("    <NMakeForcedIncludes>$(NMakeForcedIncludes)</NMakeForcedIncludes>");
                ProjectFileSB.AppendLine("    <NMakeAssemblySearchPath>$(NMakeAssemblySearchPath)</NMakeAssemblySearchPath>");
                ProjectFileSB.AppendLine("    <AdditionalOptions>{0}</AdditionalOptions>", GetCppVersion(ECppVersion.Cpp17));
                ProjectFileSB.AppendLine("  </PropertyGroup>");
            }


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

        string GetSharedIncludeFilesPaths()
        {
            StringBuilder includePathsStrBuilder = new StringBuilder();

            // We should add the the engine source folder as a base include here.
            includePathsStrBuilder.AppendFormat("{0};", DirectoryUtils.GetEngineSourceDir());     
            
            List<DirectoryReference> toolChainIncludeFiles = IshakBuildToolFramework.ToolChain.GetSharedDirectoriesFromToolChain();
            foreach (DirectoryReference dir in toolChainIncludeFiles)
            {
                includePathsStrBuilder.AppendFormat("{0};", dir.Path);
            }

            return includePathsStrBuilder.ToString();
        }

        protected void WriteIntellisenseInfoForEngineFiles()
        {
            ProjectFileSB.AppendLine("  <ItemGroup>");

            WriteVCCompileDataFromProjectModules();

            ProjectFileSB.AppendLine("  </ItemGroup>");
        }

        /** Iterate through all the Modules SourceFiles that this project has and write the data from them for Intellisense.  */
        void WriteVCCompileDataFromProjectModules()
        {
            foreach (IshakModule module in ProjectToHandle.Modules)
            {
                WriteIntellisenseInfoFromModule(module);
            }

            FilterGenerator.Finish();
        }

        void WriteIntellisenseInfoFromModule(IshakModule module)
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
                    WriteStandardVCCompileTypeForFile(sourceFileRef, vcCompileType, module);
                }

                FilterGenerator.AddFile(sourceFileRef);
            }
        }

        void WriteStandardVCCompileTypeForFile(FileReference file, EVCFileType cvFileType, IshakModule module)
        {
            string fileRelativeToProjectFile = DirectoryUtils.MakeRelativeTo(file, ProjectToHandle.ProjectFile.GetProjectFileRef()).Path;

            ProjectFileSB.AppendLine("    <{0} Include=\"{1}\"/>", cvFileType.ToString(), fileRelativeToProjectFile);
        }

        void WriteVCCompileTypeSourceFile(FileReference file, IshakModule fileParentModule)
        {
            // Write the source file to the engineProjectFileStr and its additional source files for compiling this file, for now this second step
            // will not be necessary as there are only one folder for the engine project, but as we add modules this may be.
            // When implementing the additional module dependent files, we are gonna add them here.


            string fileRelativeToProjectFile = DirectoryUtils.MakeRelativeTo(file, ProjectToHandle.ProjectFile.GetProjectFileRef()).Path;

            ProjectFileSB.AppendLine(
                "    <{0} Include=\"{1}\">",
                EVCFileType.ClCompile.ToString(), fileRelativeToProjectFile);

            ProjectFileSB.AppendLine(
                "      <AdditionalIncludeDirectories>$(NMakeIncludeSearchPath);{0}</AdditionalIncludeDirectories>",
                fileParentModule.ModulesDependencyDirsString + GetSharedIncludeFilesPaths());

            ProjectFileSB.AppendLine("    </ClCompile>");
        }

        protected void WriteSourcePathProperty()
        {
            ProjectFileSB.AppendLine("  <PropertyGroup>");
            ProjectFileSB.Append("    <SourcePath>");

            foreach (IshakModule module in ProjectToHandle.DependencyModules)
            {
                ProjectFileSB.Append("{0};", module.PrivateDirectoryRef.Path);
            }

            ProjectFileSB.AppendLine("</SourcePath>");
            ProjectFileSB.AppendLine("  </PropertyGroup>");
        }

        protected void CreateProjectFile()
        {
            ProjectToHandle.ProjectFile.SetProjectFileContent(ProjectFileSB.ToString());
            ProjectToHandle.ProjectFile.Create();

            ProjectFileSB.Clear();
        }

        protected void ImportFinalProjectFileArguments()
        {
            ProjectFileSB.AppendLine("  <Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.targets\" />");
            ProjectFileSB.AppendLine("  <ImportGroup Label=\"ExtensionTargets\">");
            ProjectFileSB.AppendLine("  </ImportGroup>");
            ProjectFileSB.AppendLine("</Project>");
        }

    }
}
