using IshakBuildTool.ProjectFile;
using IshakBuildTool.ToolChain;
using IshakBuildTool.Utils;
using Microsoft.Win32;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using System.Text;

namespace IshakBuildTool.Platform
{


    /** Representation of the Windows SDK */
    [SupportedOSPlatform("windows")]
    internal class WindowsSDK
    {

        [SupportedOSPlatform("windows")]
        static readonly Lazy<KeyValuePair<RegistryKey, string>[]> kInstallDirRoots = new Lazy<KeyValuePair<RegistryKey, string>[]>(() =>
        new KeyValuePair<RegistryKey, string>[]
        {
            new KeyValuePair<RegistryKey, string>(Registry.CurrentUser, "SOFTWARE\\"),
            new KeyValuePair<RegistryKey, string>(Registry.LocalMachine, "SOFTWARE\\"),
            new KeyValuePair<RegistryKey, string>(Registry.CurrentUser, "SOFTWARE\\Wow6432Node\\"),
            new KeyValuePair<RegistryKey, string>(Registry.LocalMachine, "SOFTWARE\\Wow6432Node\\")
        });

        public List<DirectoryReference> IncludeDirectories = new List<DirectoryReference>();
        public List<FileReference> LibraryFiles = new List<FileReference>();

        /** Main Directories that include the SDK main headers.( Windows.h  for example) */
        List<DirectoryReference> MainSDKLibraryDirectories = new List<DirectoryReference>();
        DirectoryReference? Directory { get; set; } = null;
        VersionData WindowsVersion = new VersionData(); 
        EWindowsArchitecture Architecture;               

        public WindowsSDK() 
        {             
            // For now we are just aiming for x64 architecture, the will be not ARM64.
            Architecture = EWindowsArchitecture.x64;

            Init();
        }

        public EWindowsArchitecture GetArchitecture() 
        { 
            return Architecture; 
        }
    
        void Init()
        {
             ExploreWindowsOSRegistries();
             SetWindowsVersion();
             SetWindowsIncludeDirectories();
        }
        
        void ExploreWindowsOSRegistries()
        {            
            DirectoryReference? rootDir = null;

            if (TryReadInstalledDirFromRegistryKey32("Microsoft\\Windows Kits\\Installed Roots", "KitsRoot10", out rootDir))
            {                 
                MainSDKLibraryDirectories.Add(rootDir);
            }
            if (TryReadInstalledDirFromRegistryKey32("Microsoft\\Microsoft SDKs\\Windows\\v10.0", "InstallationFolder", out rootDir))
            {
                MainSDKLibraryDirectories.Add(rootDir);
            }
        }

    
        bool TryReadInstalledDirFromRegistryKey32(
            string keySuffix,
            string value,
            [NotNullWhen(true)] out DirectoryReference? installedDir)
        {
            
            foreach (KeyValuePair<RegistryKey, string> installationRoot in kInstallDirRoots.Value)
            {
                using (RegistryKey? key = installationRoot.Key.OpenSubKey(installationRoot.Value + keySuffix))
                {
                    if (key != null && TryReadDirFromRegistryKey(key.Name, value, out installedDir))
                    {
                        return true;
                    }
                }
            }

            installedDir = null;
            return false;
        }

       
        bool TryReadDirFromRegistryKey(string keyName, string value, [NotNullWhen(true)] out DirectoryReference? foundDir)
        {
            string? stringValue = Registry.GetValue(keyName, value, null) as string;
            if (!String.IsNullOrEmpty(stringValue))
            {
                foundDir = new DirectoryReference(stringValue);
                return true;
            }
            else
            {
                foundDir = null;
                return false;

            }
        }

        void SetWindowsVersion()
        {
            // TODO Function.
            foreach (DirectoryReference windows10Dir in MainSDKLibraryDirectories)
            {
                DirectoryReference includeRootDir = DirectoryUtils.Combine(windows10Dir, "Include");
                if (DirectoryUtils.DirectoryExists(includeRootDir))
                {
                    foreach (DirectoryReference includeDir in includeRootDir.GetChildDirectories())
                    {
                        string foundIncludeSDKVersion = includeDir.GetDirectoryName();
                        FileReference umDirRef = FileUtils.Combine(includeDir, "um", "windows.h");

                        if (FileUtils.FileExists(umDirRef))
                        {
                            Directory = windows10Dir;
                            WindowsVersion.SetVersion(foundIncludeSDKVersion);
                            WindowsVersion.Version = 10;
                            return;
                        }
                    }
                }
            }

        }

        /** We will basically explore the Windows directory and add the different important
         *  include directories.
         */
        void SetWindowsIncludeDirectories()
        {            
            if (!Directory.IsValid())
            {
                return;
            }


            if (WindowsVersion.Version < 10)
            {
                // TODO Exception
                // We do not support a windows operative system less than the 10
                throw new InvalidOperationException();
            }
                                  
            DirectoryReference windowsIncludeDirRef = DirectoryUtils.Combine(Directory, "include", WindowsVersion.VersionStr);
            IncludeDirectories.Add(DirectoryUtils.Combine(windowsIncludeDirRef, "ucrt"));
            IncludeDirectories.Add(DirectoryUtils.Combine(windowsIncludeDirRef, "shared"));
            IncludeDirectories.Add(DirectoryUtils.Combine(windowsIncludeDirRef, "um"));
            IncludeDirectories.Add(DirectoryUtils.Combine(windowsIncludeDirRef, "winrt"));

            // TODO investigate what is CPPWinRT

            /*
            if (bUseCPPWinRT)
            {
                IncludePaths.Add(DirectoryReference.Combine(IncludeRootDir, "cppwinrt"));
            }*/

            DirectoryReference windowsLibraryRootDir  = DirectoryUtils.Combine(Directory, "lib", WindowsVersion.VersionStr);            
            LibraryFiles.Add(FileUtils.Combine(windowsLibraryRootDir, "ucrt", Architecture.ToString(), "libucrt.lib"));
            LibraryFiles.Add(FileUtils.Combine(windowsLibraryRootDir, "um", Architecture.ToString(), "kernel32.lib"));            

            // TODO Add Intel specific math library when using intel

            /*
             *  IncludePaths.Add(DirectoryReference.Combine(CompilerDir, "windows", "compiler", "include"));
             *  LibraryPaths.Add(DirectoryReference.Combine(CompilerDir, "windows", "compiler", "lib", "intel64"));
             */

}
}
}
