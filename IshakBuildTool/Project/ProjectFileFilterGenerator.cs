using IshakBuildTool.ProjectFile;
using System.Text;

using IshakBuildTool.Utils;

namespace IshakBuildTool.Project
{
    /** This class is in charge of generating the Project Filter file, so when opening the project in Visual Studio
     *  we will have all the folder ordered and filtered by folders.
     * */
    internal class ProjectFileFilterGenerator
    {

        /** String Builder that will be the content for the Filter File when written. */
        StringBuilder ProjectFilterFileSB = new StringBuilder();

        /** Final filters for the project, representing the folders. */
        List<string> FolderFilters= new List<string>();

        FileReference FilterFileReference;


        public ProjectFileFilterGenerator(ProjectFile projectFileToHandleFilters) 
        {
            string filterFilePath = projectFileToHandleFilters.Path + ".filters";
            FilterFileReference= new FileReference(filterFilePath);            
        }

        public void Init() 
        {
            ProjectFilterFileSB.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            ProjectFilterFileSB.AppendLine("<Project ToolsVersion=\"17.0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");
            ProjectFilterFileSB.AppendLine("  <ItemGroup>");
        }

        
        public void Finish()
        {
            ProjectFilterFileSB.AppendLine("  </ItemGroup>");
            ProjectFilterFileSB.AppendLine("</Project>");
            DirectoryUtils.CreateDirectoryWithContent(FilterFileReference.Path, ProjectFilterFileSB.ToString());
        }
        
               
        public void AddFile(FileReference fileRef)
        {
            FileReference relativePathToFilterFile;
            string fileFilterName = string.Empty;
            List<string> fileFiltersToCreate = GetFilterInfoForFile(fileRef, out relativePathToFilterFile, ref fileFilterName);

            CreateFiltersForFile(fileFiltersToCreate);
            WriteFileData(fileRef, relativePathToFilterFile.Path, fileFilterName);
        }

        List<string> GetFilterInfoForFile(FileReference fileRef, out FileReference relativePathToFilterFile, ref string fileFilterEntireName)
        {
            // TODO make better architecture.
            relativePathToFilterFile = DirectoryUtils.MakeRelativeTo(fileRef, FilterFileReference);                        
            fileFilterEntireName =  DirectoryUtils.RemoveFolderMoveCharsFromDirRef(relativePathToFilterFile.Directory);
            return GetFilterListFromFilterName(ref fileFilterEntireName);
        }

        List<string > GetFilterListFromFilterName(ref string filterName)
        {
            List<string> filters = new List<string>();
            string actualDirName = string.Empty;
            for (int charIdx = 0; charIdx < filterName.Length; ++charIdx)
            {
                char actualChar = filterName[charIdx];
                if (actualChar == '\\')
                {
                    filters.Add(actualDirName);
                    actualDirName += actualChar;
                }
                else if (charIdx == filterName.Length - 1)
                {
                    actualDirName += actualChar;
                    filters.Add(actualDirName);
                }
                else
                { 
                    actualDirName += actualChar;
                }                
            }
            filterName = actualDirName;

            return filters;
        }

        void CreateFiltersForFile(List<string> filterList)
        {
            foreach (string filterName in filterList)
            {
                if (!DoesFilterExists(filterName))
                {
                    AddFilter(filterName);
                }
            }
        }

        bool DoesFilterExists(string filterName)
        {
            return FolderFilters.Contains(filterName);
        }

        void AddFilter(string filterName)
        {            
            ProjectFilterFileSB.AppendLine("    <Filter Include=\"{0}\">", filterName);
            ProjectFilterFileSB.AppendLine("      <UniqueIdentifier>{0}</UniqueIdentifier>", GeneratorGlobals.BuildGuid(filterName).ToString("B").ToUpperInvariant());
            ProjectFilterFileSB.AppendLine("    </Filter>");

            FolderFilters.Add(filterName);
        }

        void WriteFileData(FileReference fileRef, string fileRelativePathToFilterFile, string filterName)
        {
            EVCFileType VCFIleType = GeneratorGlobals.GetVCFileTypeForFile(fileRef);
            ProjectFilterFileSB.AppendLine("    <{0} Include=\"{1}\">", VCFIleType, fileRelativePathToFilterFile);
            HandleFilterWriting(filterName);
            ProjectFilterFileSB.AppendLine("    </{0}>", VCFIleType);
        }

        void HandleFilterWriting(string filterName)
        {
            if (filterName == string.Empty)
            {
                return;
            }

            ProjectFilterFileSB.AppendLine("      <Filter>{0}</Filter>", filterName);
        }      
    }
}
