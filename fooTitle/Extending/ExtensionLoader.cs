using System;
using System.Collections;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.IO;

using fooTitle.Layers;
using fooTitle.Geometries;

namespace fooTitle.Extending {

    // thrown when the extension couldn't have been compiled, because it contained errors
    class ExtensionCompileException : ApplicationException {
        public CompilerErrorCollection Errors;

        public ExtensionCompileException(CompilerErrorCollection errors) 
            : base("Couldn't compile extension, errors in source occured") {

            Errors = errors;
            Console.Write(ToString());
        }

        public override String ToString() {
            String res = "";
            foreach (CompilerError e in Errors) {
                res += e.ToString() + "\n";
            }
            return res;
        }

    }

    /// Responsible for loading extensions and caching the
    /// compiled version
    class ExtensionLoader {
        protected CSharpCodeProvider codeProvider;
        protected ICodeCompiler compiler;
        protected ElementFactory[] factories;

        public ExtensionLoader(ElementFactory[] _factories) {
            codeProvider = new CSharpCodeProvider();
            compiler = codeProvider.CreateCompiler();
            factories = _factories;
        }

        // generates the security permissions for the compiled plugin
        protected Evidence getEvidenceForCompiledAssembly() {
            // TODO
            PermissionSet requested = new PermissionSet(PermissionState.None);
            PermissionSet optional = new PermissionSet(PermissionState.None);
            PermissionSet denied = new PermissionSet(PermissionState.None);

            FileIOPermission fileIO = new FileIOPermission(PermissionState.None);
            fileIO.AddPathList(FileIOPermissionAccess.Read, "/home/xplasil/test");
            requested.AddPermission(fileIO);

            SecurityPermission sec = new SecurityPermission(SecurityPermissionFlag.Execution);
            requested.AddPermission(sec);

            PermissionRequestEvidence permRequest = new PermissionRequestEvidence(requested, optional, denied);
            Evidence res = new Evidence();
            res.AddAssembly(permRequest);
            return res;
        }
        
        /// <param name="extensionDir">the directory where the entire extension is located, relative to application</param>
        /// <param name="files">an array files the extension consists of, relative to extensionDir</param>
        public void LoadExtension(String name, String extensionDir, String[] files) {
            String[] filesPath = new String[files.Length];
            int i = 0;
            foreach (String p in files) {
                filesPath[i] = Path.Combine(extensionDir, p);

                // check source code presence
                if (!File.Exists(filesPath[i])) {
                    throw new FileNotFoundException("Cannot find file for compiling extension", filesPath[i]);
                }

                i++;
            }

            // compile or load already compiled assembly
            String assemblyPath = Path.Combine(extensionDir, name + ".dll");
            Assembly compiledAssembly;
            if (File.Exists(assemblyPath) && (!checkRecompiling(assemblyPath, filesPath))) {
                compiledAssembly = loadAssembly(assemblyPath);
            } else {
                File.Delete(assemblyPath);
                compiledAssembly = compileAssembly(assemblyPath, filesPath);
            }

            // process it by the factories
            foreach (ElementFactory f in factories) {
                f.SearchAssembly(compiledAssembly);
            }

            // now run it
            ArrayList plugins = createInstances(compiledAssembly);
            foreach (IExtension p in plugins) {
                p.Init();
            }

        }

        /// returns true if the assembly should be recompiled
        protected bool checkRecompiling(String assemblyPath, String[] filesPath) {
            DateTime assemblyTime = File.GetLastWriteTime(assemblyPath);

            foreach (String path in filesPath) {
                if (File.GetLastWriteTime(path).CompareTo(assemblyTime) > 0)
                    return true;
            }

            return false;

        }

        protected Assembly loadAssembly(String filePath) {
            return Assembly.LoadFile(filePath);
        }

        protected Assembly compileAssembly(String outFilePath, String[] sourceFilePaths) {
            // compile
            String[] refAssemblies = {"System.dll", "fooTitle.exe"};
            CompilerParameters cp = new CompilerParameters(refAssemblies);
            cp.GenerateInMemory = false;
            cp.GenerateExecutable = false;
            cp.OutputAssembly = outFilePath;
            //cp.Evidence = getEvidenceForCompiledAssembly();
            CompilerResults res = compiler.CompileAssemblyFromFileBatch(cp, sourceFilePaths);


            // check and write out errors
            if (res.Errors.Count > 0) {
                throw new ExtensionCompileException(res.Errors);
            }

            return res.CompiledAssembly;
        }

        protected ArrayList createInstances(Assembly assembly) {
            Type[] types = assembly.GetTypes();
            ArrayList res = new ArrayList();
            Type pluginType = typeof(IExtension);

            foreach (Type t in types) {
                if (pluginType.IsAssignableFrom(t)) {
                    ConstructorInfo cons = t.GetConstructor(Type.EmptyTypes);
                    if (cons != null) {
                        IExtension p = (IExtension)cons.Invoke(null);
                        res.Add(p);
                    }
                }
            }

            return res;
        }

    }

};
