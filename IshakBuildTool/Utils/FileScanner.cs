﻿// Copyright(c) Juan Esteban Rayo Contreras. All rights reserved.

using IshakBuildTool.ProjecFile;
using IshakBuildTool.ProjectFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.Utils
{
    enum EFileScannerFilterMode
    {
        EInclusive,
        EExclusive
    }

    internal class FilterMaker
    {

        static public List<FileReference> MakeScannerFilter(List<string> filterFiles)
        {
            List<FileReference> result = new List<FileReference>();
            foreach (var file in filterFiles)
            {
                result.Add(new FileReference(file));
            }

            return result;
        }

    }

    internal class FileScanner
    {

        // STARNDARD File Scanner //
        static public List<FileReference> FindFilesInDirectory(ProjectDirectory projectFolder, bool bRecursive = false)
        {
            if (bRecursive)
            {
                return FindFilesInDirectoryRecursive(projectFolder);
            }

            return FindFilesInDirectory(projectFolder);
        }

        static private List<FileReference> FindFilesInDirectoryRecursive(ProjectDirectory folderToExplore)
        {
            List<FileReference> foundFiles = FindFilesInDirectory(folderToExplore);
            List<string> foundDirectories = Directory.GetDirectories(folderToExplore.DirectoryPath).ToList();

            // No more subdirectories, so we jus return the found files in the one we are exploring.
            if (foundDirectories.Count == 0)
            {
                return foundFiles;
            }

            // Explore subdirectories for files
            List<FileReference> subFolderFiles = new List<FileReference>();
            foreach (string directoryPath in foundDirectories)
            {
                var mergedFiles = subFolderFiles.Concat(FindFilesInDirectoryRecursive(new ProjectDirectory(directoryPath))).ToList();
                subFolderFiles = mergedFiles;
            }

            foundFiles.AddRange(subFolderFiles);
            return foundFiles;
        }

        static private List<FileReference> FindFilesInDirectory(ProjectDirectory folderToExplore)
        {
            List<FileReference> foundFilesReferences = new List<FileReference>();
            List<string> foundFiles = Directory.GetFiles(folderToExplore.DirectoryPath).ToList();

            foreach (string file in foundFiles)
            {
                foundFilesReferences.Add(new FileReference(file));
            }


            return foundFilesReferences;
        }


        // FILTERED File Scanner//

        static public List<FileReference> FindSourceFiles(ProjectDirectory directory)
        {
            return FindFilesInDirectoryWithFilter(
                directory,
                EFileScannerFilterMode.EInclusive,
                new List<string> { "Source" },
                new List<string> { ".h", ".cpp" });
        }

        static public List<FileReference> FindFilesInDirectoryWithFilter(
            ProjectDirectory projectDirectory,
            EFileScannerFilterMode filterMode,
            List<string> folderFilter,
            List<string> fileExtensionsToFilter,
             bool bRecursive = true)
        {
            if (bRecursive)
            {
                return FindFilesInDirectoryRecursiveWithFilter(projectDirectory, filterMode, folderFilter, fileExtensionsToFilter);
            }

            return FindFilesInDirectoryWithExtensionsFilter(projectDirectory, fileExtensionsToFilter);
        }

        static private List<FileReference> FindFilesInDirectoryRecursiveWithFilter(
            ProjectDirectory folderToExplore,
            EFileScannerFilterMode filterMode,
            List<string> folderFilter,
            List<string> fileExtensionsToFilter)
        {
            List<FileReference> foundFiles = FindFilesInDirectoryWithExtensionsFilter(folderToExplore, fileExtensionsToFilter);
            List<string> foundDirectories = Directory.GetDirectories(folderToExplore.DirectoryPath).ToList();

            var foundFilteredDirectories = FilterDirectories(foundDirectories, filterMode, folderFilter);
        
            // No more subdirectories, so we jus return the found files in the one we are exploring.
            if (foundFilteredDirectories.Count == 0)
            {
                return foundFiles;
            }

            // Explore subdirectories for files
            List<FileReference> subFolderFiles = new List<FileReference>();
            foreach (string directoryPath in foundFilteredDirectories)
            {
                var mergedFiles = subFolderFiles.Concat(
                    FindFilesInDirectoryRecursiveWithFilter(
                        new ProjectDirectory(directoryPath),
                        filterMode, folderFilter,
                        fileExtensionsToFilter)).ToList();

                subFolderFiles = mergedFiles;
            }

            foundFiles.AddRange(subFolderFiles);
            return foundFiles;
        }

        static private List<string> FilterDirectories(List<string> directoriesToFilter, EFileScannerFilterMode filterMode, List<string> filterDirectories)
        {
            List<string> filteredDirectories = new List<string>();
                    
            foreach (string directoryPath in directoriesToFilter)
            {
                
                bool bIsFileInFilter = FilterSingleDirectory(new  FileReference(directoryPath), filterDirectories);
              

                bool condition = false;
                if (filterMode == EFileScannerFilterMode.EInclusive)
                {
                    condition = bIsFileInFilter;

                }
                else if (filterMode == EFileScannerFilterMode.EExclusive)
                {

                    condition = !bIsFileInFilter;
                }

                if (condition)
                {
                    filteredDirectories.Add(directoryPath);
                }
            }


            return filteredDirectories;
        }

        static private List<FileReference> FindFilesInDirectoryWithExtensionsFilter(
            ProjectDirectory folderToExplore,
            List<string> filesExtensionToFilter)
        {
            List<FileReference> foundFilesReferences = new List<FileReference>();
            List<string> foundFiles = Directory.GetFiles(folderToExplore.DirectoryPath).ToList();

            foreach (string file in foundFiles)
            {
                foundFilesReferences.Add(new FileReference(file));
            }

            foundFilesReferences = FilterFiles(foundFilesReferences, filesExtensionToFilter);

            return foundFilesReferences;
        }

        static private List<FileReference> FilterFiles(List<FileReference> filesToFilter, List<string> filesExtensionsFilter)
        {
            return FilterFilesFromFilterMode(filesToFilter, filesExtensionsFilter);
        }

        static private List<FileReference> FilterFilesFromFilterMode(List<FileReference> filesToFilter, List<string> filesExtensionFilter)
        {
            List<FileReference> filteredFiles = new List<FileReference>();
            foreach (FileReference file in filesToFilter)
            {

                bool bFileHasExtension = FilterSingleFile(file, filesExtensionFilter);
                if (bFileHasExtension)
                {
                    filteredFiles.Add(file);
                }
            }

            return filteredFiles;
        }

        // For now by default we just filter the folder in an inclusive way.
        private static bool FilterSingleFile(FileReference fileToFilter, List<string> filterExtensions)
        {            
            foreach (String fileExtension in filterExtensions)
            {
                String fileStr = new String(Path.GetFileName(fileToFilter.path));                
                if (fileStr.Contains(fileExtension))
                {
                    return true;
                }
            }

            return false;
        }

        // For now by default we just filter the folder in an inclusive way.
        private static bool FilterSingleDirectory(FileReference directoryToFilter, List<string> filterDirectories)
        {
            String directoryToFilterStr = directoryToFilter.path;            
            foreach (String filterDirectory in filterDirectories)
            {                                                
                if (directoryToFilterStr.Contains(filterDirectory))
                {
                    return true;
                }
            }

            return false;
        }


    }
}