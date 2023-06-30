using IshakBuildTool.Platform;


namespace IshakBuildTool.ToolChain
{
    /** Different resources for the internal toolchain of the Tool( Compiler, Linker... ) */
    internal class IshakToolChain
    {
        /** Compiler for the IshakBuildTool Enviro. */
        Compiler Compiler = new Compiler();

        /** Wrapper for the Windows Platform. */
        WindowsPlatform? WindowsPlatform = null;
    }
}
