using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using SSL_Framework.PersistantRoutineSystem;

public static partial class SSL_Utils
{

    public static class PersistantRoutineCompiler
    {
        private static int _routineUID;
        private static CSharpCodeProvider provider = null;
        private static CompilerParameters parameters = null;


        public static string _runtimeCompilationTemplate =
        @"

        namespace SSL_RuntimeCompiler
        {                
            public class SSL_RuntimeClass
            {                
                public static void SSL_RuntimeFunction()
                {
                    SSL_RuntimeContents
                }
            }
        }";


        //custom namspace class and function to ensure it doesnt mess with anyone elses stuff
        public static string _namespaceID = "SSL_RuntimeCompiler";
        public static string _classID = "SSL_RuntimeClass";
        public static string _funcID = "SSL_RuntimeFunction";
        public static string _contentsID = "SSL_RuntimeContents";

        private static bool EnsureCodeConformity(string runtimeCodeToCompile)
        {
            return !(runtimeCodeToCompile.Contains(_namespaceID) || runtimeCodeToCompile.Contains(_classID) || runtimeCodeToCompile.Contains(_funcID) || runtimeCodeToCompile.Contains(_contentsID));
        }


        //In case someone wants to do this at runtime for whatever reason.
        public static void OverrideSetup(CSharpCodeProvider provider, CompilerParameters parameters)
        {
            PersistantRoutineCompiler.provider = provider;
            PersistantRoutineCompiler.parameters = parameters;
        }

        private static void Setup()
        {
            provider = new CSharpCodeProvider();
            parameters = new CompilerParameters();

            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = false;

            // Add ALL of the assembly references
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.Contains("Cecil") || assembly.FullName.Contains("UnityEditor"))
                    continue;
                parameters.ReferencedAssemblies.Add(assembly.Location);
            }
        }

        public static MethodInfo CreateRoutine(PersistantRoutineDatum dat)
        {
            if (provider == null || parameters == null)
            {
                Setup();
            }
            

            if (!EnsureCodeConformity(dat.Routine))
            {
                //TODO :- throw an more descriptive exception
                throw new Exception("Code contains reserved keywords");
            }


            //https://forum.unity.com/threads/compiling-c-at-runtime.376611/

            ++_routineUID;

            string currNSID = _namespaceID + _routineUID.ToString();
            string currClassID = _classID + _routineUID.ToString();
            string currFuncID = _funcID + _routineUID.ToString();

            string codeToCompile = _runtimeCompilationTemplate.Replace(_namespaceID, currNSID);
            codeToCompile = codeToCompile.Replace(_classID, currClassID);
            codeToCompile = codeToCompile.Replace(_funcID, currFuncID);
            codeToCompile = codeToCompile.Replace(_contentsID, dat.Routine);

            
           

            CompilerResults results = provider.CompileAssemblyFromSource(parameters, codeToCompile);

            if (results.Errors.Count > 0)
            {
                var msg = new StringBuilder();
                foreach (CompilerError error in results.Errors)
                {
                    msg.AppendFormat("Error ({0}): {1}\n",
                        error.ErrorNumber, error.ErrorText);
                }
                throw new Exception(msg.ToString());
            }

            Type binaryFunction = results.CompiledAssembly.GetType(currNSID +'.'+ currClassID);

            return binaryFunction.GetMethod(currFuncID);
        }
    }

    
}
