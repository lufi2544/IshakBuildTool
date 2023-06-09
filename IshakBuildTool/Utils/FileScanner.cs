﻿// Copyright(c) Juan Esteban Rayo Contreras. All rights reserved.


using IshakBuildTool.ProjectFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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

    static class FileScanner
    {

        static readonly string[] DefaultExcludeFileSuffixes = new string[]
        {
            ".vcxproj",
            ".vcxproj.filters",
            ".csproj",
            ".sln"
        };

       

        // FILTERED File Scanner//

        static public List<FileReference> FindSourceFiles(string directory)
        {
            return FindFilesInDirectoryWithFilter(directory);                           
        }

        static public List<FileReference> FindFilesInDirectoryWithFilter(
            string projectDirectory,
            EFileScannerFilterMode? filterMode = null,
            List<string>? folderFilter = null,
            EFileScannerFilterMode? filesFilterMode = null,
            List<string>? fileExtensionsToFilter = null,
             bool bRecursive = true)
        {
   
            if (bRecursive)
            {
                return FindFilesInDirectoryWithFilterRecursive(projectDirectory, filterMode, folderFilter, filesFilterMode, fileExtensionsToFilter);
            }

            return FindFilesInDirectoryWithExtensionsFilter(projectDirectory, filesFilterMode, fileExtensionsToFilter);
        }

        static private List<FileReference> FindFilesInDirectoryWithFilterRecursive(
            string folderToExplore,
            EFileScannerFilterMode? filterMode,
            List<string>? folderFilter,
            EFileScannerFilterMode? filesFilterMode,
            List<string>? fileExtensionsToFilter)
        {
            List<FileReference> foundFiles = FindFilesInDirectoryWithExtensionsFilter(folderToExplore, filesFilterMode, fileExtensionsToFilter);
            List<string> foundDirectories = Directory.GetDirectories(folderToExplore).ToList();

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
                    FindFilesInDirectoryWithFilterRecursive(
                        directoryPath,
                        filterMode,
                        folderFilter,
                        filesFilterMode,
                        fileExtensionsToFilter)).ToList();

                subFolderFiles = mergedFiles;
            }

            foundFiles.AddRange(subFolderFiles);
            return foundFiles;
        }

        static private List<string> FilterDirectories(List<string> directoriesToFilter, EFileScannerFilterMode? filterMode, List<string>? filterDirectories)
        {
            // No filter will be applied, so we just return all the directories.
            if (filterDirectories == null)
            {
                return directoriesToFilter;
            }

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
            string folderToExplore,
            EFileScannerFilterMode? filterMode,
            List<string>? filesExtensionToFilter)
        {
            List<FileReference> foundFilesReferences = new List<FileReference>();
            List<string> foundFiles = Directory.GetFiles(folderToExplore).ToList();

            foreach (string file in foundFiles)
            {
                foundFilesReferences.Add(new FileReference(file));
            }

            foundFilesReferences = FilterFiles(foundFilesReferences, filterMode, filesExtensionToFilter);

            return foundFilesReferences;
        }

        static private List<FileReference> FilterFiles(List<FileReference> filesToFilter, EFileScannerFilterMode? filterMode, List<string>? filesExtensionsFilter)
        {
            return FilterFilesFromFilterMode(filesToFilter, filterMode, filesExtensionsFilter);
        }

        static private List<FileReference> FilterFilesFromFilterMode(List<FileReference> filesToFilter, EFileScannerFilterMode? filterMode, List<string>? filesExtensionFilter)
        {
            List<FileReference> filteredFiles = new List<FileReference>();
            foreach (FileReference file in filesToFilter)
            {                
                bool bFilterConditionPassed = FilterSingleFile(file, filterMode, filesExtensionFilter);
                if (bFilterConditionPassed)
                {
                    filteredFiles.Add(file);
                }
            }

            return filteredFiles;
        }
        
        private static bool FilterSingleFile(FileReference fileToFilter, EFileScannerFilterMode? filterMode, List<string>? filterExtensions)
        {
            // If not filter is passed in, then we just fill it with the default extensionsfiles.
            if (filterExtensions == null)
            {
                filterExtensions = new List<string>(DefaultExcludeFileSuffixes);                
            }

            if (filterMode == null)
            {
                filterMode = EFileScannerFilterMode.EExclusive;
            }

            foreach (String fileExtension in filterExtensions)
            {
                String fileStr = new String(Path.GetFileName(fileToFilter.Path));

                if (fileStr.Contains(fileExtension))
                {
                    if (filterMode == EFileScannerFilterMode.EInclusive)
                    {
                        return true;

                    }else if(filterMode == EFileScannerFilterMode.EExclusive)
                    {
                        return false;
                    }
                }
            }

            // If after all the filtering nothing has matched, then we have to check if we should include the file 
            // so if we are excluding .sln and .cs, all the rest have to be included
            // On the other hand, if we want to include .sln and .cs, then we have to exclude the other files.
               
            if (filterMode == EFileScannerFilterMode.EExclusive)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // For now by default we just filter the folder in an inclusive way.
        private static bool FilterSingleDirectory(FileReference directoryToFilter, List<string>? filterDirectories)
        {
            if (filterDirectories == null)
            {
                return false;
            }

            String directoryToFilterStr = directoryToFilter.Path;            
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
