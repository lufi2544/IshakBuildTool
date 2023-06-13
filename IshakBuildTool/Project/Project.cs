using IshakBuildTool.Project.Modules;

namespace IshakBuildTool.Project
{
    /** Class representing a single project. */
    internal class Project
    {
        public string Name { get; set; }

        /** This is the .vcxproj file. */
        public ProjectFile ProjectFile { get; set; }

        public List<Module> Modules { get; set; }

        public Guid GUID { get; set; }


        public Project(string name, ProjectFile projectFile, List<Module> modules)
        {
            ProjectFile = projectFile;
            ProjectFile.SetOwnerProject(this);

            Name = name;
            Modules = modules;
        }  
        
        /** Writes the .vcxproj for this Project.  */
        public void WriteProjectFile()
        {
            ProjectFileGenerator projectFileGenerator = new ProjectFileGenerator(this);

            // This will fill up the ProjectFile for this project.
            projectFileGenerator.HandleProjectFileGeneration();
        }
    }
}
