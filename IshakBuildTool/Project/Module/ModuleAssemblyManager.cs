using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis.Emit;
using System.Runtime.InteropServices;

using IshakBuildTool.Utils;
using IshakBuildTool.ProjectFile;

namespace IshakBuildTool.Project.Module
{
    internal class ModuleAssemblyManager
    {
        ModuleAssemblyManager(FileReference engineAssemblyPath, List<FileReference> moduleFilesRefs)
        {
            CreateAssemblyForModules(engineAssemblyPath, moduleFilesRefs);
        }

        void CreateAssemblyForModules(FileReference assemblyPath, List<FileReference> moduleCreationFiles)
        {
            // This method will basically convert the Module.cs files to actual C# files taht
            // the compiler can read, so we can extract the data later on when we need.

            // Create the parse options for parsing the modules files to Source Files.
            CSharpParseOptions parseOptions = new CSharpParseOptions(
                languageVersion: LanguageVersion.Latest,
                kind: SourceCodeKind.Regular
                /* Add any preprocessor declarations here if needed. */
                );

            List<SyntaxTree> modulesSyntaxTrees = new List<SyntaxTree>();

            foreach (FileReference moduleSourceFile in moduleCreationFiles)
            {
                SourceText moduleSource = SourceText.From(File.ReadAllText(moduleSourceFile.FullPath), System.Text.Encoding.UTF8);
                SyntaxTree tree = CSharpSyntaxTree.ParseText(moduleSource, parseOptions, moduleSourceFile.FullPath);

                IEnumerable<Diagnostic> parseDiagnostic = tree.GetDiagnostics();
                if (parseDiagnostic.Any())
                {
                    // Print Error
                    foreach (Diagnostic diagnostic in parseDiagnostic)
                    {
                        StringBuilder diagnosticStrB = new StringBuilder();
                        diagnosticStrB.AppendLine(
                            "Diagnostic When Parsing Module {0} . Diagnostic : Severity {1}, Descrpition: {2}",
                            moduleSourceFile.FullPath,
                            diagnostic.Severity,
                            diagnostic.Descriptor.Description.ToString()
                            );

                    }

                }

                modulesSyntaxTrees.Add(tree);
            }


            DirectoryInfo assemblyPathInfo = Directory.CreateDirectory(assemblyPath.FullPath);

            // Add the metadata references
            List<MetadataReference> metadataReferences = new List<MetadataReference>();
            metadataReferences.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(Assembly.Load("System.IO").Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(Assembly.Load("System.IO.FileSystem").Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(Assembly.Load("System.Linq").Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(Assembly.Load("System.Private.Xml").Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(Assembly.Load("System.Private.Xml.Linq").Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(Assembly.Load("System.Text.RegularExpressions").Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(Assembly.Load("System.Console").Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(Assembly.Load("System.Runtime.Extensions").Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(Assembly.Load("Microsoft.Extensions.Logging.Abstractions").Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location));

            // process start dependencies
            metadataReferences.Add(MetadataReference.CreateFromFile(Assembly.Load("System.ComponentModel.Primitives").Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(Assembly.Load("System.Diagnostics.Process").Location));

            // registry access
            metadataReferences.Add(MetadataReference.CreateFromFile(Assembly.Load("Microsoft.Win32.Registry").Location));

            // RNGCryptoServiceProvider, used to generate random hex bytes
            metadataReferences.Add(MetadataReference.CreateFromFile(Assembly.Load("System.Security.Cryptography.Algorithms").Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(Assembly.Load("System.Security.Cryptography.Csp").Location));

            metadataReferences.Add(MetadataReference.CreateFromFile(typeof(FileReference).Assembly.Location));


            CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(
                outputKind: OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Debug,
                warningLevel: 4,
                assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default,
                reportSuppressedDiagnostics: true
                );

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName: assemblyPath.GetPathWithoutFileExtension(),
                syntaxTrees: modulesSyntaxTrees,
                references: metadataReferences,
                options: compilationOptions
                );

            using (FileStream AssemblyStream = FileReference.Open(assemblyPath, FileMode.Create))
            {
                FileReference assemblyPDBFile = assemblyPath.ChangeExtensionCopy(".pdb");
                using (FileStream? PdbStream =
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? FileReference.Open(assemblyPDBFile, FileMode.Open)
                    : null)
                {
                    EmitOptions EmitOptions = new EmitOptions(
                        includePrivateMembers: true
                    );

                    EmitResult compilationResult = compilation.Emit(
                        peStream: AssemblyStream,
                        pdbStream: PdbStream,
                        options: EmitOptions);

                    // Print Error
                    foreach (Diagnostic diagnostic in compilationResult.Diagnostics)
                    {
                        StringBuilder diagnosticStrB = new StringBuilder();
                        diagnosticStrB.AppendLine(
                            "Diagnostic When Compiling Assembly Module Binary . Diagnostic : Severity {1}, Descrpition: {2}",
                            diagnostic.Severity,
                            diagnostic.Descriptor.Description.ToString()
                            );

                    }

                    if (!compilationResult.Success)
                    {
                        // TODO Exception The Compilation Was not successful


                    }

                }
            }

           ModulesAssembly = Assembly.LoadFile(assemblyPath.FullPath);
        }


        private Assembly? ModulesAssembly;
    }
}
        
    

