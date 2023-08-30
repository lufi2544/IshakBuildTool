using IshakBuildTool.Platform;
using IshakBuildTool.Project.Modules;
using IshakBuildTool.ProjectFile;
using IshakBuildTool.ToolChain.Windows;
using IshakBuildTool.Utils;
using Microsoft.VisualStudio.Setup.Configuration;
using System.Diagnostics;
using System.Text;

namespace IshakBuildTool.ToolChain
{
    /** Different resources for the internal toolchain of the Tool( Compiler, Linker... ) */
    internal class IshakToolChain
    {

        struct ToolChainInstallation
        {
            
            public ToolChainInstallation(
                DirectoryReference toolChainDirParam,
                DirectoryReference redistDirParam,
                EWindowsArchitecture architechtureParam,
                VersionData versionDataParam)
            {
                ToolChainDir = toolChainDirParam;
                RedistDir = redistDirParam;  
                Architecture = architechtureParam;
                VersionData = versionDataParam;
            }


            public DirectoryReference? ToolChainDir = null; 
            DirectoryReference? RedistDir = null;
            EWindowsArchitecture Architecture;
            VersionData? VersionData = null;            
        }

        /** 
         *  Compiler for the IshakBuildTool Enviro.
         *  By default: we are gonna use the MVS compiler.
         */
        Compiler? Compiler;
        ECompilerType CompilerType = ECompilerType.MSVC;

        /** Wrapper for the Windows Platform. */
        public WindowsPlatform WindowsPlatform = new WindowsPlatform();

        VisualStudioInstallation? VisualStudioInstallation;

        ToolChainInstallation Installation { get; set; }

        public IshakToolChain()
        {                        
            SetVisualStudioInstallation();
            CreateCompiler();       
        }

        public List<DirectoryReference> GetSharedDirectoriesFromToolChain()
        {
            List<DirectoryReference> sharedDirectories = new List<DirectoryReference>();        
            
            sharedDirectories.Add(DirectoryUtils.Combine(Installation.ToolChainDir, "include"));
            sharedDirectories = sharedDirectories.Concat(WindowsPlatform.GetWindowsSDKIncludeDirs()).ToList();                      

            return sharedDirectories;
        }

        public List<FileReference> GetSharedSystemLibs()
        {
            List<FileReference> systemLinkedLibs = new List<FileReference>();
            //libPathArgs.Append("/LIBPATH:\"C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\VC\\Tools\\MSVC\\14.36.32532\\lib\\x64\"");

            // TODO PLATFORM get the actual platform here.
            systemLinkedLibs.Add(FileUtils.Combine(Installation.ToolChainDir, "lib", WindowsPlatform.GetWindowsArchitecture().ToString(), "libcmt.lib"));
            systemLinkedLibs.Add(FileUtils.Combine(Installation.ToolChainDir, "lib", WindowsPlatform.GetWindowsArchitecture().ToString(), "libcpmt.lib"));            
            systemLinkedLibs.AddRange(WindowsPlatform.GetWindowsSystemLibs());

            return systemLinkedLibs;
        }

        public DirectoryReference GetDir()
        {
            if (Installation.ToolChainDir != null)
            {
                return Installation.ToolChainDir;
            }

            // ToolChain has no dir.
            throw new InvalidOperationException();
        }


        public async Task CompileModules(List<IshakModule> modules)
        {
            await Compiler?.CompileModules(modules);
        }

        void CreateCompiler()
        {
            // We could add some other compilers here like CLang or others.
            Compiler = new Compiler(
                Installation.ToolChainDir,
                CompilerType, 
                GetSharedDirectoriesFromToolChain(), 
                GetSharedSystemLibs());
        }

        void SetVisualStudioInstallation()
        {
            if (VisualStudioInstallation != null)
            {
                // never init 2 times
                throw new SystemException { };
            }            

            VisualStudioInstallation = GetVisualStudioInstallationInTheSystem();
            // find the visual studio tool chains and set value, once the installation has been found.
            FindVisualStudioToolChainAndSetProperties();
          
        }

        /** Here we could get all the installations, but for now let's assume we have VS 2022 installed. */
        VisualStudioInstallation? GetVisualStudioInstallationInTheSystem()
        {            
            try
            {
                SetupConfiguration setup = new SetupConfiguration();
                IEnumSetupInstances enumerator = setup.EnumInstances();

                ISetupInstance[] instances = new ISetupInstance[1];
                for (; ;)
                {
                    int numFetched;
                    enumerator.Next(1, instances, out numFetched);

                    // No visual studio installations found
                    // TODO Exception.
                    if (numFetched == 0)
                    {
                        break;
                    }

                    ISetupInstance2 instance = (ISetupInstance2)(instances[0]);
                    if ((instance.GetState() & InstanceState.Local) != InstanceState.Local)
                    {
                        continue;
                    }

                    string version = instance.GetInstallationVersion();
                    VersionData versionData = new VersionData(version);
                    
                    

                    WindowsCompilerType windowsCompilerType;
                    if (versionData.Version >= 17)
                    {
                        windowsCompilerType = WindowsCompilerType.VisualStudio2022;
                    }else if (versionData.Version == 16)
                    {
                        windowsCompilerType = WindowsCompilerType.VisualStudio2019;
                    }
                    else
                    {
                        continue;
                    }

                    return new VisualStudioInstallation(
                        windowsCompilerType,
                        versionData,
                        new ProjectFile.DirectoryReference(instance.GetInstallationPath()));
                }

            }catch (Exception ex)
            {
                // TODO Build Exception or maybe ToolChainException
                throw new SystemException("Problems finding visual studio installation.");
            }

            return null;
            
        }
        void FindVisualStudioToolChainAndSetProperties()
        {

            

            DirectoryReference baseVSToolChainDir = DirectoryUtils.Combine(VisualStudioInstallation.Directory, "VC", "Tools", "MSVC");
            DirectoryReference redistBaseDir = DirectoryUtils.Combine(VisualStudioInstallation.Directory, "VC", "Redist", "MSVC");
            Installation = CreateVisualStudioInstallation(baseVSToolChainDir, redistBaseDir);
        }

        ToolChainInstallation CreateVisualStudioInstallation(DirectoryReference toolChainDir, DirectoryReference redistDirParam)
        {
            ToolChainInstallation installation = new ToolChainInstallation();

            if (toolChainDir.Exist())
            {                
                foreach (DirectoryReference toolChainSubDir in toolChainDir.GetChildDirectories().ToList())
                {
                    VersionData versionData;
                    if (IsValidToolChainDirForMSVC(toolChainSubDir, out versionData))
                    {
                        DirectoryReference visualStudioRedistToolChain = FindVisualStudioRedistForToolChain(toolChainSubDir, redistDirParam);
                        return CreateCppVisualStudioInstallation(toolChainSubDir, visualStudioRedistToolChain, versionData);
                    }
                }

            }

            return installation;
        }


        bool IsValidToolChainDirForMSVC(DirectoryReference toolChainDir, out VersionData toolChainVersionData)
        {
            toolChainVersionData = new VersionData();

            // See if any of the compilers exists inside the toolchain Dir.            
            FileReference compilerExe = FileUtils.Combine(toolChainDir, "bin", "Hostx86", "x64", "cl.exe");
            if (!compilerExe.Exists())
            {
                compilerExe = FileUtils.Combine(toolChainDir, "bin", "Hostx64", "x64", "cl.exe");
                if (!compilerExe.Exists())
                {
                    return false;
                }
            }

            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(compilerExe.Path);
            if (versionInfo.ProductMajorPart != 0)
            {
                toolChainVersionData = new VersionData(versionInfo.ProductMajorPart, versionInfo.ProductMinorPart, versionInfo.ProductBuildPart);
                return true;
            }

            // I think this sould not happen, TODO study
            throw new Exception();
        }

        DirectoryReference FindVisualStudioRedistForToolChain(
            DirectoryReference toolChainDir,
            DirectoryReference? optionalRedistDirParam)
        {
            DirectoryReference redistDir;
            if (optionalRedistDirParam == null)
            {
                redistDir = DirectoryUtils.Combine(toolChainDir, "redist");
            }
            else
            {
                redistDir = DirectoryUtils.Combine(optionalRedistDirParam, toolChainDir.GetDirectoryName());

                if (!redistDir.Exist() && optionalRedistDirParam.Exist())
                {
                    throw new Exception();

                }
            }

            if (redistDir != null && redistDir.Exist())
            {
                return redistDir;
            }

            return redistDir;
        }

        ToolChainInstallation CreateCppVisualStudioInstallation(DirectoryReference toolChainDir, DirectoryReference redistDir, VersionData versionData)
        {
            // TODO PLATFORM, maybe checking the platform here for different ones: ARMx64...            
            return new ToolChainInstallation(toolChainDir, redistDir, EWindowsArchitecture.x64, versionData); ;
        }
       
    }
}
