using IshakBuildTool.Project.Modules;
using System.Text;

namespace IshakBuildTool.Project
{
    /** Class representing a single project. */
    internal class IshakProject
    {
        public string Name { get; set; }

        /** This is the .vcxproj file. */
        public IshakProjectFile ProjectFile { get; set; }

        public List<IshakModule>? Modules { get; set; }

        Guid GUID { get; set; }

        /** Project type, for a c++ project it would be a certain id. */
        public string ProjectTypeId { get; set; }


        public IshakProject(string name, IshakProjectFile projectFile, List<IshakModule>? modules)
        {
            ProjectFile = projectFile;
            ProjectFile.SetOwnerProject(this);

            Name = name;
            Modules = modules;
            ProjectTypeId = GetProjectId();
        }

        public void SetGUID(Guid guid)
        {
            GUID = guid;    
        }
        public string GetGUID()
        {
            return GUID.ToString("B").ToUpperInvariant();
        }

        string GetProjectId()
        {
            return ProjectGlobals.CpppProjectFileId;
        }

        /** Writes the .vcxproj for this Project.  */
        public void WriteProjectFile()
        {
            ProjectFileGenerator projectFileGenerator = GetProjectFileGenerator();

            // This will fill up the ProjectFile for this project.
            projectFileGenerator.HandleProjectFileGeneration();
        }

        ProjectFileGenerator GetProjectFileGenerator()
        {                       
            return new ProjectFileGenerator(this);
        }
    }
}
