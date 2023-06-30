using IshakBuildTool.Build;
using IshakBuildTool.ProjectFile;
using IshakBuildTool.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

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


        /** Main Directories that include the SDK main headers.( Windows.h  for example) */
        List<DirectoryReference> MainSDKLibraryDirectories = new List<DirectoryReference>();   
        DirectoryReference Directory { get; set; }

        string Version = string.Empty;

        public WindowsSDK() 
        {
            Init();
        }
    
        void Init()
        {
             ExploreWindowsOSRegistries();
             SetWindowsVersion();
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
                            Version = foundIncludeSDKVersion;
                            break;
                        }
                    }
                }
            }

        }
    }
}
