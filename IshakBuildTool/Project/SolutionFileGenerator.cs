
using System.Text;
using IshakBuildTool.Utils;

namespace IshakBuildTool.Project
{
    /** Class in charge of generating the Solution file for the current BuildContext. */
    internal class SolutionFileGenerator
    {

        StringBuilder SolutionFileSB = new StringBuilder();
        Dictionary<ProjectRoot, Project> projectsWithRootDictionary = new Dictionary<ProjectRoot, Project>();

        public SolutionFileGenerator()
        {

        }

        public void GenerateSolutionFile(
            List<Project> projects,
            string directoryPath,
            string solutionFileName)
        {
            SolutionFile solutionFileToGenerate = new SolutionFile(directoryPath, solutionFileName);

            WriteHeaderForFile();
            AddProjects(projects);         
            WriteConfigurations(projects);
            WriteSolutionProperties();
            LinkRootsAndProject();
            WriteExtraSolutionFileProperties(solutionFileToGenerate);

            GenerateSolutionFile(solutionFileToGenerate);
        }

        void WriteHeaderForFile()
        {
            SolutionFileSB.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
            SolutionFileSB.AppendLine("# Visual Studio Version 17");
            SolutionFileSB.AppendLine("VisualStudioVersion = 17.0.31314.256");
            SolutionFileSB.AppendLine("MinimumVisualStudioVersion = 10.0.40219.1");
        }

        void AddProjects(List<Project> projects)
        {
            foreach (Project project in projects)
            {
                AddProjectToSolutionFile(project);
            }
        }

        void AddProjectToSolutionFile(Project project)
        {
            ProjectRoot rootProject = CreateRootProjectFolder(project);
            projectsWithRootDictionary.Add(rootProject, project);

            WriteProjectInSolutionFile(rootProject, project);            
        }

                
        ProjectRoot CreateRootProjectFolder(Project project)
        {

            // TODO decide weather we just use the Projects Alone or we use RootFolders, either way I think it is okay.
            string rootName = string.Empty;
            if (project.Name.Contains("Engine"))
            {
                rootName = "Engine";
            }
            else
            {
                rootName = "Game";
            }

            return new ProjectRoot(rootName);            
        }

        void WriteProjectInSolutionFile(ProjectRoot projectRoot, Project project)
        {
            WriteRootInSolutionFile(projectRoot);
            WriteProjectInSolutionFile(project);            
        }

        void WriteRootInSolutionFile(ProjectRoot projectRoot)
        {
            SolutionFileSB.AppendLine("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"", projectRoot.GetGenericVSRootGUID(), projectRoot.Name, projectRoot.Name, projectRoot.GetGUID());
            SolutionFileSB.AppendLine("EndProject");
        }

        void WriteProjectInSolutionFile(Project project)
        {
            SolutionFileSB.AppendLine("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"", project.ProjectTypeId, project.Name, project.ProjectFile.Path, project.GetGUID());
            SolutionFileSB.AppendLine("EndProject");
        }

        void WriteConfigurations(List<Project> projects)
        {
            SolutionFileSB.AppendLine("Global"); 
            foreach (Project project in projects)
            {
                SolutionFileSB.AppendLine("     GlobalSection(SolutionConfigurationPlatforms) = preSolution");
                SolutionFileSB.AppendLine("         {0}|{1} = {0}|{1}", Test.TestEnviroment.DefaultConfigurationName, Test.TestEnviroment.DefaultPlatform.ToString());
                SolutionFileSB.AppendLine("     EndGlobalSection");


                // TODO Function to implement parameters in the project Globa Section.

                SolutionFileSB.AppendLine("     GlobalSection(ProjectConfigurationPlatforms) = postSolution");
                SolutionFileSB.AppendLine("         {0}.{1}|{2}.ActiveCfg = {3}|{2}",
                    project.GetGUID(),
                    Test.TestEnviroment.DefaultConfigurationName,
                    Test.TestEnviroment.DefaultPlatform.ToString(), 
                    Test. TestEnviroment.DefaultConfiguration.ToString());

                SolutionFileSB.AppendLine("         {0}.{1}|{2}.Build.0 = {3}|{2}",
                  project.GetGUID(),
                  Test.TestEnviroment.DefaultConfigurationName,
                  Test.TestEnviroment.DefaultPlatform.ToString(),
                  Test.TestEnviroment.DefaultConfiguration.ToString());

                SolutionFileSB.AppendLine("     EndGlobalSection");
            }
        }

        void WriteSolutionProperties()
        {
            SolutionFileSB.AppendLine("     GlobalSection(SolutionProperties) = preSolution");
            SolutionFileSB.AppendLine("         HideSolutionNode = FALSE");
            SolutionFileSB.AppendLine("     EndGlobalSection");
        }

        void LinkRootsAndProject()
        {
            // We link the projects here to their roots, the prupose of this is to have the projects in different root folders.
            // Engine(Root) ---- IshakEngine(Project)
            // Game(Root) ---- Shooter(Project)
            SolutionFileSB.AppendLine("     GlobalSection(NestedProjects) = preSolution");
            foreach (var rootProjectPair in projectsWithRootDictionary) 
            {
                SolutionFileSB.AppendLine("         {0} = {1}", rootProjectPair.Value.GetGUID(), rootProjectPair.Key.GetGUID());
            }
            SolutionFileSB.AppendLine("     EndGlobalSection");
        }

        void WriteExtraSolutionFileProperties(SolutionFile solutionFile)
        {
            /*
            SolutionFileSB.AppendLine("GlobaSection(ExtensibilityGlobals) = postSolution");
            SolutionFileSB.AppendLine("     SolutionGuid = {0}", GeneratorGlobals.BuildGUID(solutionFile.SolutionFileRef.Name, solutionFile.SolutionFileRef.Name).ToString("B").ToUpperInvariant());
            SolutionFileSB.AppendLine("EndGlobalSection");
            */

            SolutionFileSB.AppendLine("EndGlobal");
        }

        void GenerateSolutionFile(SolutionFile solutionFile)
        {
            DirectoryUtils.CreateFileWithContent(solutionFile.SolutionFileRef.Path, SolutionFileSB.ToString());
        }

    }
}
