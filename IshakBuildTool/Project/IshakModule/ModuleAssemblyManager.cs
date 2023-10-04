using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis.Emit;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

// IBT namespaces
using IshakBuildTool.Utils;
using IshakBuildTool.ProjectFile;

namespace IshakBuildTool.Project.Modules
{
    /** Class in charge of creating the Assembly for the engine modules.*/
    internal class ModuleAssemblyManager
    {

        private readonly string assemblyBinaryFilePath = "Build\\Modules\\";
        private readonly string assemblyBinaryExtension = ".dll";
        private readonly string assemblyBinaryName = "ModulesBinary";        

        private Assembly? ModulesAssembly;
        bool bCompilationMode = false;


        public ModuleAssemblyManager(DirectoryReference engineIntermediateDirectory, List<FileReference> moduleFilesRefs, bool bCompilation)
        {
            FileReference completeAssemblyPath =
                FileUtils.Combine(engineIntermediateDirectory, assemblyBinaryFilePath + assemblyBinaryName + assemblyBinaryExtension);            

            bCompilationMode = bCompilation;
            
            CreateAssemblyForModules(completeAssemblyPath, moduleFilesRefs);
        }

        public Assembly? GetModulesAssembly()
        {
            return ModulesAssembly;
        }

        public ModuleBuilder? GetModuleBuilderByName(string moduleName, Assembly? modulesAssembly)
        {
            if (ModulesAssembly == null)
            {
                // TODO Exception
                // Trying to Get a module builder when the Assembly has not even been created yet.
            }
            
            Type? specifigModuleBuilder = GetModulesAssembly().GetType(moduleName + ModuleBuilder.ModuleBuilderPrefix);
            if (specifigModuleBuilder == null)
            {
                // TODO Exception

                // Maybe added a module and did not regenerate Project Files. If after regeneration this happens again, 
                // check the Module.cs and take a look at the ModuleBuilder.
                throw new InvalidOperationException();
            }

            // In this case the Modules do not take any parammeter, so we extract the default param from it.
            Type[] emptyParams = Array.Empty<Type>();
            ConstructorInfo? constructor = specifigModuleBuilder.GetConstructor(emptyParams);
            if (constructor == null)
            {
                // TODO Exception
            }
            
            // As the module builder is an abstract interface, we can not create an instance of it directly, so we recurr to an UnitializedObject.
            ModuleBuilder builderObject = (ModuleBuilder)FormatterServices.GetUninitializedObject(specifigModuleBuilder);
            constructor.Invoke(builderObject, new object[] { });

            return builderObject;
        }


        void CreateAssemblyForModules(FileReference assemblyFileRef, List<FileReference> modulesCreationFiles)
        {            
            ModulesAssembly = HandleModulesAssemblyCompilation(assemblyFileRef, modulesCreationFiles);
        }

        Assembly? HandleModulesAssemblyCompilation(FileReference assemblyFileRef, List<FileReference> modulesCreationFiles)
        {
            // Create Directory For Assembly if not existed.
            DirectoryUtils.TryCreateDirectory(assemblyFileRef.Directory);

            List<SyntaxTree> parsedModulesSyntaxTrees;
            List<MetadataReference> metadataReferences;            
            ParseModulesFilesToSourceCode(modulesCreationFiles, out parsedModulesSyntaxTrees, out metadataReferences);

            CSharpCompilation modulesRunTimeCompilation = CreateCompilationObjectForModules(parsedModulesSyntaxTrees, metadataReferences);
            return ExecuteCompilation(assemblyFileRef, modulesRunTimeCompilation);
        }

        /** Parses all the Module.cs files to actual source code, so all of the files are read and then parsed. **/
        void ParseModulesFilesToSourceCode(
            List<FileReference> modulesCreationFiles,
            out List<SyntaxTree> parsedMosulesSyntaxTrees, 
            out List<MetadataReference> metadataReferences)
        {
            parsedMosulesSyntaxTrees = ParseModulesFiles(modulesCreationFiles);
            metadataReferences= CreateMetadataReferences();
        }

        /** Basically parses the pure text of the Module.cs files to actually source files that the compiler can read. */
        List<SyntaxTree> ParseModulesFiles(List<FileReference> modulesCreationFiles)
        {            

            // Create the parse options for parsing the modules files to Source Files.
            CSharpParseOptions parseOptions = new CSharpParseOptions(
                languageVersion: LanguageVersion.Latest,
                kind: SourceCodeKind.Regular
                /* Add any preprocessor declarations here if needed. */
                );

            List<SyntaxTree> modulesSyntaxTrees = new List<SyntaxTree>();
            foreach (FileReference moduleSourceFile in modulesCreationFiles)
            {
                SourceText moduleSource = SourceText.From(File.ReadAllText(moduleSourceFile.Path), System.Text.Encoding.UTF8);
                SyntaxTree tree = CSharpSyntaxTree.ParseText(moduleSource, parseOptions, moduleSourceFile.Path);

                IEnumerable<Diagnostic> parseDiagnostic = tree.GetDiagnostics();
                if (parseDiagnostic.Any())
                {
                    // Print Error
                    foreach (Diagnostic diagnostic in parseDiagnostic)
                    {
                        StringBuilder diagnosticStrB = new StringBuilder();
                        diagnosticStrB.AppendLine(
                            "Diagnostic When Parsing Module {0} . Diagnostic : Severity {1}, Descrpition: {2}",
                            moduleSourceFile.Path,
                            diagnostic.Severity,
                            diagnostic.Descriptor.Description.ToString()
                            );

                    }

                }

                modulesSyntaxTrees.Add(tree);
            }


            return modulesSyntaxTrees;
        }

        /** Adds any metadata references that are needed for the compilation process, 
         * as the base class for the ModuleBuilder and some other SystemClasses for the final binary to compile.*/
        List<MetadataReference> CreateMetadataReferences()
        {
            // Add the metadata references
            List<MetadataReference> metadataReferences = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),

                // System mandatory metadata references
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location),

                // Ishak Build Tool metadata references
                MetadataReference.CreateFromFile(typeof(IshakBuildTool.Project.Modules.ModuleBuilder).Assembly.Location)
            };

            return metadataReferences;
        }

        /** Creates a CSharpCompilation object that holds the data for the compile about the Modules Binary file compilation. */
        CSharpCompilation CreateCompilationObjectForModules(List<SyntaxTree> modulesSyntaxTrees, List<MetadataReference> modulesMetadataReferences)
        {
            CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(
                outputKind: OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Debug,
                warningLevel: 4,
                assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default,
                reportSuppressedDiagnostics: true
                );

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName: assemblyBinaryName,
                syntaxTrees: modulesSyntaxTrees,
                references: modulesMetadataReferences,
                options: compilationOptions
                );

            return compilation;
        }

        /** 
         * <summary>
         *      Submits the compilation directly to the compiler.                          
         * </summary>
         * 
         * <returns> The EmitResult about the compilation.</returns>         
         */
        EmitResult EmitCompilation(CSharpCompilation compilation, FileStream AssemblyStream, FileStream? pdbStream)
        {
            EmitOptions EmitOptions = new EmitOptions(
            includePrivateMembers: true);     

            return compilation.Emit(
                peStream: AssemblyStream,
                pdbStream: pdbStream,
                options: EmitOptions);            
        }

        Assembly? ExecuteCompilation(FileReference assemblyFileRef, CSharpCompilation modulesRunTimeCompilation)
        {
            if (!bCompilationMode)
            {           
                using (FileStream assemblyStream =
                        FileReference.Open(assemblyFileRef, FileMode.Create))
                {
                    FileReference assemblyPDBFile = assemblyFileRef.ChangeExtensionCopy(".pdb");

                    using (FileStream? pdbStream =
                            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                            ? FileReference.Open(assemblyPDBFile, FileMode.Create)
                            : null)
                    {
                        EmitResult compilationResult = EmitCompilation(modulesRunTimeCompilation, assemblyStream, pdbStream);

                        // TODO Logger Print Error if the severity is error we should crash the program
                        foreach (Diagnostic diagnostic in compilationResult.Diagnostics)
                        {
                            StringBuilder diagnosticStrB = new StringBuilder();
                            diagnosticStrB.AppendLine(
                                "Diagnostic When Compiling Assembly Module Binary . Diagnostic :{0}",
                                diagnostic.ToString()
                                );

                            System.Console.WriteLine(diagnosticStrB.ToString());

                        }

                        if (!compilationResult.Success)
                        {
                            // TODO Exception
                            return null;
                        }

                        System.Console.WriteLine("Compiled Modules Assembly....Success");

                    }
                }
            }

            return Assembly.LoadFile(assemblyFileRef.Path);
        }
        
    }
}
        
    

