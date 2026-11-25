using System;

namespace Nikson
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Field, Inherited = false)]
    public sealed class DontObfuscateAttribute : Attribute { }
}

#if UNITY_EDITOR
namespace Nikson
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;

    public class CodeObfuscator : IPostBuildPlayerScriptDLLs
    {
        public int callbackOrder => 0;

        public static string LastRunSummary { get; private set; } = "not run yet";

        static readonly string[] ProcessedAssemblyNames = { "Assembly-CSharp.dll", "Assembly-CSharp-firstpass.dll" };
        static readonly string[] InspectorNameExtensions = { "*.anim", "*.unity", "*.prefab", "*.asset" };
        static readonly HashSet<string> ReservedNames = new()
        {
            "Awake", "Start", "OnEnable", "OnDisable", "OnDestroy", "Update", "FixedUpdate", "LateUpdate", "OnValidate", "Reset", "OnGUI",
            "OnDrawGizmos", "OnDrawGizmosSelected", "OnApplicationFocus", "OnApplicationPause", "OnApplicationQuit", "OnBecameVisible", "OnBecameInvisible",
            "OnCollisionEnter", "OnCollisionEnter2D", "OnCollisionExit", "OnCollisionExit2D", "OnCollisionStay", "OnCollisionStay2D",
            "OnTriggerEnter", "OnTriggerEnter2D", "OnTriggerExit", "OnTriggerExit2D", "OnTriggerStay", "OnTriggerStay2D",
            "OnMouseDown", "OnMouseUp", "OnMouseUpAsButton", "OnMouseEnter", "OnMouseExit", "OnMouseOver", "OnMouseDrag",
            "OnAnimatorIK", "OnAnimatorMove", "OnParticleCollision", "OnParticleTrigger", "OnJointBreak", "OnJointBreak2D",
            "OnTransformChildrenChanged", "OnTransformParentChanged", "OnPreCull", "OnPreRender", "OnPostRender", "OnRenderImage", "OnRenderObject", "OnWillRenderObject",
            "OnBeforeSerialize", "OnAfterDeserialize", "Main",
        };

        public void OnPostBuildPlayerScriptDLLs(BuildReport report)
        {
            var files = report.GetFiles().Where(file => file.role == CommonRoles.managedLibrary && ProcessedAssemblyNames.Contains(Path.GetFileName(file.path))).ToArray();
            if (files.Length == 0) { LastRunSummary = "No Assembly-CSharp.dll in this build: nothing renamed."; return; }
            var excluded = ScanInspectorMethodNames();
            int methods = 0, fields = 0;
            var nameMap = new List<string>();
            // Scan every assembly first: code in one assembly can refer by name to a member of another
            foreach (var file in files)
            {
                using var assembly = AssemblyDefinition.ReadAssembly(file.path, new ReaderParameters { InMemory = true });
                ScanStringLiterals(assembly.MainModule, excluded);
            }
            foreach (var file in files)
            {
                using var assembly = AssemblyDefinition.ReadAssembly(file.path, new ReaderParameters { InMemory = true });
                int counter = 0;
                foreach (var type in assembly.MainModule.Types) RenameType(type, excluded, ref counter, ref methods, ref fields, nameMap);
                AddSuppressIldasm(assembly);
                assembly.Write(file.path);
            }
            LastRunSummary = $"Renamed {methods} private methods and {fields} private fields across {files.Length} assembly file(s).";
            Debug.Log($"[Nikson] Code Obfuscator: {LastRunSummary}");
            string logsFolder = Path.Combine(Application.dataPath, "../Logs");
            Directory.CreateDirectory(logsFolder);
            File.WriteAllLines(Path.Combine(logsFolder, "Obfuscation_NameMap.txt"), nameMap);
        }

        // Unity calls a method by its exact name, read as plain text, from an AnimationEvent (functionName) or an Inspector-assigned UnityEvent (m_MethodName): both must survive renaming
        static HashSet<string> ScanInspectorMethodNames()
        {
            var names = new HashSet<string>();
            foreach (var extension in InspectorNameExtensions)
                foreach (var path in Directory.GetFiles(Application.dataPath, extension, SearchOption.AllDirectories))
                    foreach (Match match in Regex.Matches(File.ReadAllText(path), @"(?:functionName|m_MethodName): (\w+)"))
                        names.Add(match.Groups[1].Value);
            return names;
        }

        // Blocks ildasm and old decompilers that respect this attribute from opening the renamed assembly directly; does nothing against a real reverse engineer (dnSpy, ILSpy), it is a free, cheap extra step
        static void AddSuppressIldasm(AssemblyDefinition assembly)
        {
            var attributeType = typeof(System.Runtime.CompilerServices.SuppressIldasmAttribute);
            if (HasAttribute(assembly, attributeType.Name)) return;
            var ctor = assembly.MainModule.ImportReference(attributeType.GetConstructor(System.Type.EmptyTypes));
            assembly.CustomAttributes.Add(new CustomAttribute(ctor));
        }

        // Any member whose name also appears as a string literal anywhere in the code might be looked up by name (Invoke, InvokeRepeating, SendMessage, StartCoroutine("X"),
        // GetMethod, JSON/reflection helpers...), so it is never renamed. Deliberately conservative: it works in any project without the user changing their code,
        // at the cost of occasionally keeping a name that only happens to equal an unrelated string.
        static void ScanStringLiterals(ModuleDefinition module, HashSet<string> excluded) { foreach (var type in module.Types) ScanStringLiteralsInType(type, excluded); }

        static void ScanStringLiteralsInType(TypeDefinition type, HashSet<string> excluded)
        {
            foreach (var method in type.Methods)
            {
                if (!method.HasBody) continue;
                foreach (var instruction in method.Body.Instructions)
                    if (instruction.OpCode == OpCodes.Ldstr) excluded.Add((string)instruction.Operand);
            }
            foreach (var nested in type.NestedTypes) ScanStringLiteralsInType(nested, excluded);
        }

        static void RenameType(TypeDefinition type, HashSet<string> excluded, ref int counter, ref int methods, ref int fields, List<string> nameMap)
        {
            if (!HasDontObfuscate(type) && !IsCompilerGenerated(type.Name))
            {
                foreach (var method in type.Methods) if (CanRename(method, excluded)) { string from = method.Name; method.Name = NextName(ref counter); methods++; nameMap.Add($"{type.FullName}.{from} -> {method.Name}"); }
                foreach (var field in type.Fields) if (CanRename(field, excluded)) { string from = field.Name; field.Name = NextName(ref counter); fields++; nameMap.Add($"{type.FullName}.{from} -> {field.Name}"); }
            }
            foreach (var nested in type.NestedTypes) RenameType(nested, excluded, ref counter, ref methods, ref fields, nameMap);
        }

        // A field or method of a generic type (UpdateList<T>) is never renamed: a use through a closed instantiation (UpdateList<Enemy>) is a separate FieldReference/MethodReference
        // with its own copy of the name, which Cecil does not update when the open type's FieldDefinition/MethodDefinition is renamed, so IL2CPP then fails with "Unresolved field reference"
        static bool CanRename(MethodDefinition method, HashSet<string> excluded) => method.IsPrivate && !method.IsVirtual && !method.IsConstructor && !method.IsSpecialName && method.Overrides.Count == 0
            && !method.DeclaringType.HasGenericParameters && !IsCompilerGenerated(method.Name) && !ReservedNames.Contains(method.Name) && !excluded.Contains(method.Name) && !HasDontObfuscate(method);

        static bool CanRename(FieldDefinition field, HashSet<string> excluded) => field.IsPrivate && !field.DeclaringType.IsEnum && !field.DeclaringType.HasGenericParameters
            && !HasAttribute(field, nameof(SerializeField)) && !HasAttribute(field, "SerializeReference")
            && !IsCompilerGenerated(field.Name) && !excluded.Contains(field.Name) && !HasDontObfuscate(field);

        static bool IsCompilerGenerated(string name) => name.Contains('<') || name.Contains('>') || name is ".ctor" or ".cctor";
        static bool HasDontObfuscate(ICustomAttributeProvider member) => HasAttribute(member, nameof(DontObfuscateAttribute));
        static bool HasAttribute(ICustomAttributeProvider member, string attributeTypeName) => member.HasCustomAttributes && member.CustomAttributes.Any(attribute => attribute.AttributeType.Name == attributeTypeName);
        static string NextName(ref int counter) => "_" + (counter++).ToString("x");
    }
}
#endif