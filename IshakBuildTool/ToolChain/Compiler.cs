
using IshakBuildTool.Project.Modules;
using IshakBuildTool.ProjectFile;
using IshakBuildTool.Utils;
using System.Diagnostics;
using System.Text;
using IshakBuildTool.Platform;

namespace IshakBuildTool.ToolChain
{

    public enum ECompilerType
    {
        /** For now let's just compile with Microsoft Visual++ */
        MSVC
    }


    /** Defines the compiler for the Tool */
    public class Compiler
    {
        public FileReference? CompilerFile { get; set; } = null;
        public ECompilerType CompilerType { get; set; } = ECompilerType.MSVC;

        string GenericIncludePathSet { get; set; } = string.Empty;

        List<DirectoryReference> SystemIncludeDirs = new List<DirectoryReference>();
        List<FileReference> SystemSharedLibs = new List<FileReference>();

        // TODO REFACTOR
        DirectoryReference ToolChainDirectory { get; set; }

        public Compiler() 
        {

        }

        public Compiler(
            DirectoryReference toolChainDir,
            ECompilerType compilerTypeParam,
            List<DirectoryReference> systemIncludeDirs,
            List<FileReference> systemSharedLibs)
        {
            CompilerFile = FileUtils.Combine(toolChainDir, "bin", "Hostx64", "x64", "cl.exe");
            CompilerType = compilerTypeParam;
            SystemIncludeDirs = systemIncludeDirs;
            SystemSharedLibs = systemSharedLibs;
            ToolChainDirectory = toolChainDir;
        }


        /**
         *  This is the first step of the build process Compiles a list of modules, this basically generates the .obj file for every .cpp source file 
         *  in a module.
         */
        public async Task CompileModules(List<IshakModule> modules)
        {
            //------------------------
            // TODO REFACTOR For now we are just gonna add all the .h files for compiling.
            // In the future I am gonna build this recursively.

            // MODULE DEPENDENCY HANDLER

            List<FileReference> files = new List<FileReference>(); 
            foreach (var module in modules)
            {
                files.AddRange(module.SourceFiles);
            }

            //-----------------------


            // Compile every Module
            List<Task> compilerTasks = new List<Task>();    
            foreach (IshakModule moduleToCompile in modules) 
            {                                
                List<Task> moduleCompileTasks = CompileModuleAsync(moduleToCompile, files);
                compilerTasks.AddRange(moduleCompileTasks);
            }

            await Task.WhenAll(compilerTasks);
        }

        private List<Task> CompileModuleAsync(IshakModule module, List<FileReference> allfiles)
        {
            List<Task> moduleCompileTaskList = new List<Task>();
            List<FileReference>? moduleSourceFiles = module.SourceFiles;


            // LINKING
            List<FileReference> moduleObjFiles = new List<FileReference>();

            var linkerFile = FileUtils.Combine(ToolChainDirectory, "bin", "Hostx64", "x64", "link.exe");
            // Compile the Source files for the Module
            foreach (FileReference sourceFile in moduleSourceFiles)
            {
                if (sourceFile.FileType != EFileType.Source)
                {
                    continue;
                }

                FileReference objFile = FileUtils.Combine(module.BinariesDirectory, sourceFile.GetFileNameWithoutExtension() + BinaryTypesExtension.ObjFile);                
                moduleObjFiles.Add(objFile);
            }

            StringBuilder linkingArgs = new StringBuilder();
            linkingArgs.Append("/DLL ");
            StringBuilder objFilesString = new StringBuilder();
            FileReference moduleDllFile = new FileReference(FileUtils.Combine(module.BinariesDirectory, module.Name + BinaryTypesExtension.DynamicLib).Path);
            linkingArgs.AppendFormat("/OUT:\"{0}\" ", moduleDllFile.Path);

            
            // Set the System Shared Library Paths for the Linker
            StringBuilder libPathArgs = new StringBuilder();
                        
           
            foreach (FileReference systemLibFile in SystemSharedLibs)
            {
                libPathArgs.AppendFormat("/LIBPATH:\"{0}\" {1} ", systemLibFile.Directory.Path, systemLibFile.Name);            
            }
            

            if (module.Name == "Core")
            {
                libPathArgs.Append("/LIBPATH:\"C:\\IshakEngine\\Binaries\\Renderer\"");
                libPathArgs.Append(" Renderer.lib ");
            }


            foreach (FileReference objFile in moduleObjFiles)
            {
                linkingArgs.Append(objFile.Path);
                linkingArgs.Append(" ");
                linkingArgs.Append(libPathArgs.ToString());                
            }            

            // Start the process                    
            ProcessStartInfo linkerProccessStartInfo = new ProcessStartInfo(linkerFile.Path, linkingArgs.ToString());
            linkerProccessStartInfo.RedirectStandardOutput = true;
            linkerProccessStartInfo.RedirectStandardError = true;
            linkerProccessStartInfo.UseShellExecute = false;
            linkerProccessStartInfo.CreateNoWindow = true;


            Task linkTask = Task.Run(() =>
            {
                using (var compileProcess = Process.Start(linkerProccessStartInfo))
                {

                    compileProcess.OutputDataReceived += (sender, eventArgs) =>
                    {
                        StringBuilder compilingLog = new StringBuilder();
                        if (eventArgs.Data != null)
                        {
                            compilingLog.AppendFormat("Link: {0}", eventArgs.Data);
                            Console.WriteLine(compilingLog.ToString());
                        }
                    };


                    compileProcess.BeginOutputReadLine();
                    compileProcess.WaitForExit();
                }
            });


            moduleCompileTaskList.Add(linkTask);


            // Compile the Source files for the Module
            foreach (FileReference sourceFile in moduleSourceFiles)
            {                
                if (sourceFile.FileType != EFileType.Source)
                {
                    continue;
                }

                StringBuilder systemIncludesPathString = new StringBuilder();
                foreach (var dir in SystemIncludeDirs)
                {
                    systemIncludesPathString.AppendFormat("/I\"{0}\"", dir.Path);
                    systemIncludesPathString.Append(" ");
                }

                foreach (var file in allfiles)
                {
                    systemIncludesPathString.AppendFormat("/I\"{0}\"", file.Directory.Path);
                    systemIncludesPathString.Append(" ");
                }
                
                DirectoryUtils.TryCreateDirectory(module.BinariesDirectory);

                var fileDir = FileUtils.Combine(module.BinariesDirectory, sourceFile.GetFileNameWithoutExtension() + BinaryTypesExtension.ObjFile);
                    

                StringBuilder args = new StringBuilder();
                args.Append("/EHsc /c ");
                args.AppendFormat("/Fo\"{0}\"", fileDir.Path);

                args.Append(' ');
                args.Append(sourceFile.Path);
                args.Append(" ");
                args.Append(systemIncludesPathString.ToString());


                // Start the process                    
                ProcessStartInfo compilerProccessStartInfo = new ProcessStartInfo(CompilerFile.Path, args.ToString());
                compilerProccessStartInfo.RedirectStandardOutput = true;
                compilerProccessStartInfo.RedirectStandardError = true;
                compilerProccessStartInfo.UseShellExecute = false;
                compilerProccessStartInfo.CreateNoWindow = true;


                Task compileTask = Task.Run(() =>
                {
                    using (var compileProcess = Process.Start(compilerProccessStartInfo))
                    {
                        
                        compileProcess.OutputDataReceived += (sender, eventArgs) =>
                        {
                            StringBuilder compilingLog = new StringBuilder();
                            if (eventArgs.Data != null)
                            {
                                compilingLog.AppendFormat("Compile: {0}",eventArgs.Data);                                
                                Console.WriteLine(compilingLog.ToString());
                            }
                        };  
                        
                        
                        compileProcess.BeginOutputReadLine();
                        compileProcess.WaitForExit();
                    }
                });
                

                moduleCompileTaskList.Add(compileTask);                                         
            }



            return moduleCompileTaskList;
        }
    }
}
