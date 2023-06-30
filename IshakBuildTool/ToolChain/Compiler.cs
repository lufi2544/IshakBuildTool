using IshakBuildTool.ProjectFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.ToolChain
{

    public enum ECompilerType
    {
        /** For now let's just compile with Microsoft Visual++ */
        MVSC
    }

    /** Defines the compiler for the Tool */
    public class Compiler
    {
        public DirectoryReference Directory { get; set; } = new DirectoryReference();
        public ECompilerType CompilerType { get; set; } = ECompilerType.MVSC;

        public Compiler() { }
    }
}
