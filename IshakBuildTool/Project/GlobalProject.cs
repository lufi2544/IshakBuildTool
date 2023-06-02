using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.Project
{

    // This is the class in charge of managing the entire solution project
    // If we had 2 Games in the Global Project, we would have the Engine Project and then the 2 Game Projects
    internal class GlobalProject
    {
        public GlobalProject() { }


        /** List of Projects that will be in the soulution File. */
        public List<Project> Projects { get; set; }

        /** This will wrap up all the projects and will be used for building the .sln file.  */
        SolutionFile SolutionFile { get; set; }
    }
}
