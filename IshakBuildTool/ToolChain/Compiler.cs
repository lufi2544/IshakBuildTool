
using IshakBuildTool.Project.Modules;
using IshakBuildTool.ProjectFile;
using IshakBuildTool.Utils;
using System.Diagnostics;
using System.Text;
using IshakBuildTool.Platform;
using IshakBuildTool.Build;

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

        /**
         * Class in charge of compiling the modules in a way that recursively the files will be generated for the module that we're compiling 
         * and the modules that this is dependant on. So for example:
         * 
         *  -> if we are compiling the Renderer and is dependant in Core and Window modules:
         *      - We compile Renderer source files.
         *      - We check if the Core.dll has been created, if not, then we compile its files and create it.
         *      
         *      - Upon creation we see if the dependant modules on Core are OK(compiled and linked).
         *          - if not: then same process for the Core dependant modules.
         *          - if yes: then we link the Core.dll for the Renderer.
         *          
         *      - Continue the process with the other modules.
         */
        class DependencyCompilationHandler
        {

            Dictionary<string, IshakModule> ModuleNameToModuleMap = new Dictionary<string, IshakModule>();
            Compiler Compiler;

             ModuleDependencyTree ModuleDependencyTree;

            public DependencyCompilationHandler(Compiler compiler, List<IshakModule> engineModules)
            {
                Compiler = compiler;
                foreach (IshakModule module in engineModules)
                {
                    ModuleNameToModuleMap.Add(module.Name, module);
                }
                Compiler.ModuleNameToModuleMap = ModuleNameToModuleMap;

                // Create the module Dependecy Graph Nodes.
                Dictionary<string, ModuleDependencyTreeNode> moduleToNodeMap = new Dictionary<string, ModuleDependencyTreeNode>();
                foreach (IshakModule module in engineModules)
                {
                    ModuleDependencyTreeNode node = new ModuleDependencyTreeNode(module);
                    moduleToNodeMap.Add(module.Name, node);
                }

                // Resolve the dependencies between nodes
                foreach (KeyValuePair<string, ModuleDependencyTreeNode> moduleToNode in moduleToNodeMap)
                {
                    foreach (string publicModuleName in moduleToNode.Value.Module.PublicDependentModules)
                    {
                        ModuleDependencyTreeNode? dependantModuleNode;
                        moduleToNodeMap.TryGetValue(publicModuleName, out dependantModuleNode);

                        if (dependantModuleNode == null)
                        {
                            // Dependant module with invalid name.
                            throw new InvalidOperationException();
                        }

                        moduleToNode.Value.SetDepencency(dependantModuleNode);
                    }


                    foreach (string privateModuleName in moduleToNode.Value.Module.PrivateDependentModules)
                    {
                        ModuleDependencyTreeNode? dependantModuleNode;
                        moduleToNodeMap.TryGetValue(privateModuleName, out dependantModuleNode);

                        if (dependantModuleNode == null)
                        {
                            // Dependant module with invalid name.
                            throw new InvalidOperationException();
                        }

                        moduleToNode.Value.SetDepencency(dependantModuleNode);
                    }
                }
                

                // TODO OPTMIZE THIS iterations.
                List<ModuleDependencyTreeNode> graphNodes = new List<ModuleDependencyTreeNode>();
                foreach (KeyValuePair<string, ModuleDependencyTreeNode> moduleNameToNode in moduleToNodeMap)
                {
                    graphNodes.Add(moduleNameToNode.Value);
                }

                // Crete the Dependency Graph.
                ModuleDependencyTree = new ModuleDependencyTree(graphNodes);                
            }

            public async Task BuildModules()
            {
                List<IshakModule> modulesReadyToBuild = ModuleDependencyTree.GetDependentSortedModules();

                // TODO VARIABLE
                SemaphoreSlim semaphore = new SemaphoreSlim(2);

                await Parallel.ForEachAsync(modulesReadyToBuild, async (module, cancellatinToken) =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        await Compiler.CompileModule(module);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });


                foreach (IshakModule module in modulesReadyToBuild)
                {
                    await Compiler.LinkModule(module);
                }
            }


            // TODO BUILDREFACTOR Make a struct for the data that we extract from the modules.
            // TODO BUILDREFACTOR Change this to the Linker Object                

        }

        Dictionary<string, IshakModule> ModuleNameToModuleMap = new Dictionary<string, IshakModule>();
        public FileReference? CompilerFile { get; set; } = null;
        public ECompilerType CompilerType { get; set; } = ECompilerType.MSVC;

        string GenericIncludePathSet { get; set; } = string.Empty;

        List<DirectoryReference> SystemIncludeDirs = new List<DirectoryReference>();
        List<FileReference> SystemSharedLibs = new List<FileReference>();
        
        DirectoryReference ToolChainDirectory { get; set; }

        // TODO REMOVE
        List<FileReference> AllEngineIncludes = new List<FileReference>();

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
         *  
         *  Basically the flow will be: 
         *  -> Compile a Module -> Link with dependant modules ->(dependant module) not created yet, then we compile and and link dependant modules... and so on.
         *  
         *  Recursively we are gonna compile and link the different modules.
         *  
         */
        public async Task CompileModules(List<IshakModule> engineModules)
        {
            //------------------------
            // TODO REFACTOR For now we are just gonna add all the .h files for compiling.
            // In the future I am gonna build this recursively.

            // MODULE DEPENDENCY HANDLER            

            //-----------------------
            
            DependencyCompilationHandler dependencyCompilationHandler = new DependencyCompilationHandler(this, engineModules);           
            await dependencyCompilationHandler.BuildModules();            
        }


        List<FileReference> GetIncludeFilesForModule(IshakModule module)
        {
            // Check the module dependant modules and get their files.
            List<FileReference> moduleIncludeFiles = new List<FileReference>();

            foreach (string moduleDependentFile in module.PublicDependentModules)
            {
                IshakModule? dependantModule;
                ModuleNameToModuleMap.TryGetValue(moduleDependentFile, out dependantModule);

                if (dependantModule == null)
                {
                    // TODO EXPECTION
                    // dependant module does not exist
                    throw new InvalidOperationException();
                }

                moduleIncludeFiles.AddRange(dependantModule.GetHeaderFiles());
            }
            
            foreach (string moduleDependentFile in module.PrivateDependentModules)
            {
                IshakModule? dependantModule;
                ModuleNameToModuleMap.TryGetValue(moduleDependentFile, out dependantModule);

                if (dependantModule == null)
                {
                    // TODO EXPECTION
                    // dependant module does not exist
                    throw new InvalidOperationException();
                }

                moduleIncludeFiles.AddRange(dependantModule.GetHeaderFiles());
            }

            return moduleIncludeFiles;
        }

        private Task CompileModule(IshakModule module)
        {
         
            List<Task> compileTasks = new List<Task>();
            // Compile the Source files for the Module
            foreach (FileReference sourceFile in module.SourceFiles)
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

                foreach (FileReference dependentFile in GetIncludeFilesForModule(module))
                {
                    systemIncludesPathString.AppendFormat("/I\"{0}\"", dependentFile.Directory.Path);
                    systemIncludesPathString.Append(" ");
                }

                foreach ( string dirRef in module.GetIncludeDirsForThisModuleWhenCompiling())
                {
                    systemIncludesPathString.AppendFormat("/I\"{0}\"", dirRef);
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
                                compilingLog.AppendFormat("Compile: {0}", eventArgs.Data);
                                Console.WriteLine(compilingLog.ToString());
                            }
                        };


                        compileProcess.BeginOutputReadLine();
                        compileProcess.WaitForExit();
                    }
                });


                compileTasks.Add(compileTask);
            }

            // TODO BUILDREFACTOR find a better way of tracking the module compilation and linking completion.
            module.bCompiled = true;
            
            return Task.WhenAll(compileTasks);
        }

        bool IsModuleDLLCrated(IshakModule module)
        {
            // TODO BUILDREFACTOR Cache this somewhere.            
            return module.bDllSetToCompiled;
        }

        List<FileReference> GetObjFilesForModule(IshakModule module)
        {
            List<FileReference> intermediateObjFiles = new List<FileReference>();

            // Get the .obj file for every source file.
            foreach (FileReference sourceFile in module.SourceFiles)
            {
                if (sourceFile.FileType != EFileType.Source)
                {
                    continue;
                }

                FileReference objFile = FileUtils.Combine(module.BinariesDirectory, sourceFile.GetFileNameWithoutExtension() + BinaryTypesExtension.ObjFile);               
                intermediateObjFiles.Add(objFile);
            }

            return intermediateObjFiles;
        }

        List<IshakModule> GetModuleLinkDependencies(IshakModule module)
        {
            List<IshakModule> dependencyModules = new List<IshakModule>();
            foreach (var mod in module.PublicDependentModules)
            {
                IshakModule? moduleDependency;
                ModuleNameToModuleMap.TryGetValue(mod, out moduleDependency);
                if (moduleDependency == null)
                {
                    throw new InvalidDataException();
                }

                dependencyModules.Add(moduleDependency);
            }

            foreach (var mod in module.PrivateDependentModules)
            {
                IshakModule? moduleDependency;
                ModuleNameToModuleMap.TryGetValue(mod, out moduleDependency);
                if (moduleDependency == null)
                {
                    throw new InvalidDataException();
                }

                dependencyModules.Add(moduleDependency);
            }

            return dependencyModules;
        }


        // TODO Here, Explore a new object of priority tasks.
        Task LinkModule(IshakModule module)
        {
            
            // LINKING
            FileReference linkerFile = FileUtils.Combine(ToolChainDirectory, "bin", "Hostx64", "x64", "link.exe");

            // Get the .obj file for every source file in the module.
            List<FileReference> moduleObjFiles = GetObjFilesForModule(module);            

            StringBuilder linkingArgs = new StringBuilder();
            linkingArgs.Append("/DLL ");
            StringBuilder objFilesString = new StringBuilder();
            FileReference moduleDllFile = new FileReference(FileUtils.Combine(module.BinariesDirectory, module.Name + BinaryTypesExtension.DynamicLib).Path);
            linkingArgs.AppendFormat("/OUT:\"{0}\" ", moduleDllFile.Path);




            // Set the System Shared Library Paths for the Linker
            StringBuilder libPathArgs = new StringBuilder();

            
            // IMPORTANT: I have to make sure that the modules do not depend the one to the other, if that would be the way to go
            // for the architecture, we would have problem here, because the modules would enter in an infinite loop where they try to create to eachother
            // and therefore there is none of them created yet, so we would have to add a list or something to track their construction before 
            // they are actually constructed to avoid infinite loops.
            foreach (IshakModule dependentModule in GetModuleLinkDependencies(module))
            {                                
                libPathArgs.AppendFormat("/LIBPATH:\"{0}\" {1} ", dependentModule.ModuleDllFile.Directory.Path, dependentModule.Name);
            }

            // Adding the system libraries like STL and other windows include files.
            foreach (FileReference systemLibFile in SystemSharedLibs)
            {
                libPathArgs.AppendFormat("/LIBPATH:\"{0}\" {1} ", systemLibFile.Directory.Path, systemLibFile.Name);
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


            Task linkTask = Task.Run(() => {
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

              
            
            module.bDllSetToCompiled = true;

            return linkTask;
        }

    }

}
