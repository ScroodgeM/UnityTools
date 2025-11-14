using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace UnityTools.Editor
{
    public static class LibraryReferencesUmlGenerator
    {
        private const string UnityCoreModuleAssemblyName = "UnityEngine.CoreModule";

        private enum AssemblyType : byte
        {
            Self = 3,
            Unity = 10,
            ThirdParty = 20,
            ProjectShared = 30,
            Project = 40,
            ThirdPartyEditor = 45,
            ProjectEditor = 50,
        }

        [Serializable]
        private struct Config
        {
            [Serializable]
            public struct ColorHighlight
            {
                public Color color;
                public string[] assemblies;
            }

            public string[] unityAssemblies;
            public string[] thirdPartyAssemblies;
            public string[] thirdPartyEditorAssemblies;
            public string[] projectSharedAssemblies;
            public string[] projectEditorAssemblies;
            public string[] hiddenAssemblies;
            public ColorHighlight[] colorHighlights;
        }

        private static Config config;

        private static readonly string[] selfAssemblies = new string[]
        {
            "com.Scroodge.UnityTools.Editor",
            "com.Scroodge.UnityTools.Runtime",
            "com.Scroodge.UnityTools.UnityRuntime",
            "com.Scroodge.UnityTools.Examples",
        };

        [UnityEditor.MenuItem(nameof(UnityTools) + "/Generate UML with assembly references and open it in browser")]
        private static void GenerateUMLAndOpenItInBrowser()
        {
            GenerateUML();

            Process.Start("chrome.exe", $"--allow-file-access-from-files file://{GetPathToUMLFile()}");
        }

        [UnityEditor.MenuItem(nameof(UnityTools) + "/Generate UML with assembly references")]
        private static void GenerateUML()
        {
            ReadConfig();

            System.Text.StringBuilder umlDocument = new System.Text.StringBuilder();

            umlDocument.AppendLine("@startuml");

            umlDocument.AppendLine("scale max 1920*1080");

            umlDocument.AppendLine("!theme crt-green");

            umlDocument.AppendLine();

            CreateUml(System.AppDomain.CurrentDomain.GetAssemblies(), ref umlDocument);

            umlDocument.AppendLine();

            umlDocument.AppendLine("@enduml");

            File.WriteAllText(GetPathToUMLFile(), umlDocument.ToString());

            UnityEngine.Debug.Log("Generation success");
        }

        private static void ReadConfig()
        {
            string pathToConfig = Path.Combine(Application.dataPath, "uml_generator_config.json");

            config = File.Exists(pathToConfig) ? JsonUtility.FromJson<Config>(File.ReadAllText(pathToConfig)) : default;

            File.WriteAllText(pathToConfig, JsonUtility.ToJson(config, true));
        }

        private static void CreateUml(Assembly[] assemblies, ref System.Text.StringBuilder umlDocument)
        {
            List<Assembly> assembliesSorted = new List<Assembly>(assemblies);

            assembliesSorted.Sort((a, b) => a.FormatAssemblyName().CompareTo(b.FormatAssemblyName()));

            for (int i = 0; i < assembliesSorted.Count; i++)
            {
                Assembly assembly = assembliesSorted[i];

                bool hasEngineReferences = assembly.HasEngineReferences();

                umlDocument.AppendLine($"class {assembly.FormatAssemblyName()} {assembly.GetAssemblyColor(hasEngineReferences)} {{");

                umlDocument.AppendLine(GetAssemblyBody(assembly));

                umlDocument.AppendLine("}");

                umlDocument.AppendLine();

                foreach (AssemblyName reference in assembly.GetReferencedAssemblies())
                {
                    if (assembliesSorted.Exists(x => x.GetName().Name == reference.Name) == false)
                    {
                        assembliesSorted.Add(CreateAssemblyFromName(reference));
                    }

                    if (reference.GetAssemblyType() >= assembly.GetAssemblyType())
                    {
                        umlDocument.AppendLine($"{reference.FormatAssemblyName()} <-- {assembly.FormatAssemblyName()}");
                    }
                }

                umlDocument.AppendLine();
            }

            if (config.hiddenAssemblies != null)
            {
                foreach (string hiddenAssembly in config.hiddenAssemblies)
                {
                    umlDocument.AppendLine("remove " + hiddenAssembly.FormatAssemblyName());
                }
            }
        }

        private static Assembly CreateAssemblyFromName(AssemblyName assemblyName)
        {
            return System.Reflection.Emit.AssemblyBuilder
                .DefineDynamicAssembly(assemblyName, System.Reflection.Emit.AssemblyBuilderAccess.ReflectionOnly);
        }

        private static string FormatAssemblyName(this Assembly assembly) => assembly.GetName().FormatAssemblyName();

        private static string FormatAssemblyName(this AssemblyName assemblyName) => assemblyName.Name.FormatAssemblyName();

        private static string FormatAssemblyName(this string assemblyName) => $"{assemblyName.GetAssemblyType()}." + assemblyName.Replace(" ", "_").Replace("-", "_").Replace(".", "_");

        private static AssemblyType GetAssemblyType(this Assembly assembly) => assembly.GetName().GetAssemblyType();

        private static AssemblyType GetAssemblyType(this AssemblyName assemblyName) => assemblyName.Name.GetAssemblyType();

        private static AssemblyType GetAssemblyType(this string assemblyName)
        {
            if (assemblyName.IsInArray(config.unityAssemblies)) return AssemblyType.Unity;
            if (assemblyName.IsInArray(config.thirdPartyAssemblies)) return AssemblyType.ThirdParty;
            if (assemblyName.IsInArray(config.projectSharedAssemblies)) return AssemblyType.ProjectShared;
            if (assemblyName.IsInArray(config.thirdPartyEditorAssemblies)) return AssemblyType.ThirdPartyEditor;
            if (assemblyName.IsInArray(config.projectEditorAssemblies)) return AssemblyType.ProjectEditor;
            if (assemblyName.IsInArray(selfAssemblies)) return AssemblyType.Self;
            return AssemblyType.Project;
        }

        private static string GetAssemblyBody(Assembly assembly)
        {
            bool hasEngineReferences = Array.Exists(assembly.GetReferencedAssemblies(), x => x.Name == UnityCoreModuleAssemblyName);
            string result = hasEngineReferences ? "uses Unity" : "no Unity";

            result += ", ";
            result += assembly.GetAssemblyType().ToString();

            return result;
        }

        private static string GetAssemblyColor(this Assembly assembly, bool hasEngineReferences) => assembly.GetName().GetAssemblyColor(hasEngineReferences);

        private static string GetAssemblyColor(this AssemblyName assemblyName, bool hasEngineReferences)
        {
            Color secondColor = hasEngineReferences ? new Color(0.20f, 0.05f, 0.05f) : new Color(0.05f, 0.20f, 0.05f);

            Color firstColor = assemblyName.GetAssemblyType().GetColor();

            if (config.colorHighlights != null)
            {
                foreach (Config.ColorHighlight colorHighlight in config.colorHighlights)
                {
                    if (assemblyName.IsInArray(colorHighlight.assemblies) == true)
                    {
                        firstColor = colorHighlight.color;
                        break;
                    }
                }
            }

            return $"#{ColorUtility.ToHtmlStringRGB(firstColor)}/{ColorUtility.ToHtmlStringRGB(secondColor)}";
        }

        private static bool HasEngineReferences(this Assembly assembly)
        {
            return Array.Exists(assembly.GetReferencedAssemblies(), x => x.Name == UnityCoreModuleAssemblyName);
        }

        private static Color GetColor(this AssemblyType assemblyType)
        {
            switch (assemblyType)
            {
                case AssemblyType.Self:
                    return new Color(0.10f, 0.30f, 0.40f);
                case AssemblyType.Unity:
                    return new Color(0.40f, 0.10f, 0.30f);
                case AssemblyType.ThirdParty:
                    return new Color(0.40f, 0.30f, 0.10f);
                case AssemblyType.ProjectShared:
                    return new Color(0.30f, 0.40f, 0.10f);
                case AssemblyType.Project:
                    return new Color(0.40f, 0.40f, 0.40f);
                case AssemblyType.ProjectEditor:
                    return new Color(0.30f, 0.10f, 0.40f);
                default:
                    return new Color(0.10f, 0.30f, 0.30f);
            }
        }

        private static bool IsInArray(this AssemblyName assemblyName, string[] searchIn) => assemblyName.Name.IsInArray(searchIn);

        private static bool IsInArray(this string text, string[] searchIn) => searchIn != null && Array.Exists(searchIn, x => x == text);

        private static string GetPathToUMLFile() => Path.Combine(Application.dataPath, "../architecture.plantuml");
    }
}
