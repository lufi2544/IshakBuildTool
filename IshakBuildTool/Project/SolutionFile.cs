using System.Text;
using IshakBuildTool.ProjectFile;

namespace IshakBuildTool.Project
{
    /** Wrapper for the .sln file. In this case we have an architecture that involves having the IshakEngine below a "Engine" Project, and the Games below a "Games" project.  */
    // TODO Refactor when we implement games.
    internal class SolutionFile
    {
        StringBuilder SolutionFileBuilder = new StringBuilder();
        List<IshakProject> SolutionFileProjects = new List<IshakProject>();
        public readonly FileReference? SolutionFileRef;

        public SolutionFile()
        {

        }

        public SolutionFile(string directoryPath, string name)
        {
            string path = directoryPath + Path.DirectorySeparatorChar + name + ".sln";
            SolutionFileRef= new FileReference(path);
        }
    }
}
