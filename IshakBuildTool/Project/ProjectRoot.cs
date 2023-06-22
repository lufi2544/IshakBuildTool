using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.Project
{
    /** This is a class that simply wraps up the solution project Root, so in this case if we have the engine projects,
     *  along with a game project, we would have 2 Roots, one for the Engine project and another for the Game.
     *  
     *  Engine(Root) ---> IshakEngine( Project )
     *  Games ---> Shooter( Game )
     */
    internal class ProjectRoot
    {
        public string Name { get; set; }
        Guid GUID;                  

        public ProjectRoot(string name)            
        {
            Name= name;
            GUID = GeneratorGlobals.BuildGUID(Name, Name);
        }

        public string GetGUID()
        {
            return GUID.ToString("B").ToUpperInvariant();
        }

        public string GetGenericVSRootGUID()
        {
            return "{2150E333-8FDC-42A3-9474-1A3956D46DE8}";
        }
        
    }
}
