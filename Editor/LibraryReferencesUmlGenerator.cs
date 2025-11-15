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
            Unknown = 0,
            Self = 3,
            Unity = 10,
            ThirdParty = 20,
            ProjectShared = 30,
            Project = 40,
            ThirdPartyEditor = 45,
            ProjectEditor = 50,
        }

        [Serializable]
        private class Config
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
            public string[] projectAssemblies;
            public string[] projectEditorAssemblies;

            public string[] hiddenAssemblies;

            public string[] outputUnusedAssemblies;
            public string[] outputMissingAssemblies;

            public ColorHighlight[] colorHighlights;
        }

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
            Config config = ReadConfig();

            System.Text.StringBuilder umlDocument = new System.Text.StringBuilder();

            umlDocument.AppendLine("@startuml");

            umlDocument.AppendLine("scale max 1920*1080");

            umlDocument.AppendLine("!theme crt-green");

            umlDocument.AppendLine();

            config.CreateUml(AppDomain.CurrentDomain.GetAssemblies(), ref umlDocument);

            umlDocument.AppendLine();

            umlDocument.AppendLine("@enduml");

            File.WriteAllText(GetPathToUMLFile(), umlDocument.ToString());

            WriteConfig(config);

            UnityEngine.Debug.Log("Generation success");
        }

        private static Config ReadConfig()
        {
            return File.Exists(GetPathToConfig()) ? JsonUtility.FromJson<Config>(File.ReadAllText(GetPathToConfig())) : default;
        }

        private static void WriteConfig(Config config)
        {
            File.WriteAllText(GetPathToConfig(), JsonUtility.ToJson(config, true));
        }

        private static void CreateUml(this Config config, Assembly[] assemblies, ref System.Text.StringBuilder umlDocument)
        {
            List<Assembly> assembliesSorted = new List<Assembly>(assemblies);

            assembliesSorted.Sort((a, b) => config.FormatAssemblyName(a).CompareTo(config.FormatAssemblyName(b)));

            HashSet<string> unusedAssemblies = new HashSet<string>();
            HashSet<string> missingAssemblies = new HashSet<string>();

            foreach (string mentionedAssembly in config.GetAllMentionedAssemblies())
            {
                unusedAssemblies.Add(mentionedAssembly);
            }

            for (int i = 0; i < assembliesSorted.Count; i++)
            {
                Assembly assembly = assembliesSorted[i];

                bool hasEngineReferences = assembly.HasEngineReferences();

                umlDocument.AppendLine($"class {config.FormatAssemblyName(assembly)} {config.GetAssemblyColor(assembly, hasEngineReferences)} {{");

                umlDocument.AppendLine(config.GetAssemblyBody(assembly));

                umlDocument.AppendLine("}");

                umlDocument.AppendLine();

                foreach (AssemblyName reference in assembly.GetReferencedAssemblies())
                {
                    if (assembliesSorted.Exists(x => x.GetName().Name == reference.Name) == false)
                    {
                        assembliesSorted.Add(CreateAssemblyFromName(reference));
                    }

                    if (config.GetAssemblyType(reference) >= config.GetAssemblyType(assembly))
                    {
                        umlDocument.AppendLine($"{config.FormatAssemblyName(reference)} <-- {config.FormatAssemblyName(assembly)}");
                    }
                }

                umlDocument.AppendLine();

                string assemblyName = assembly.GetName().Name;
                if (unusedAssemblies.Remove(assemblyName) == false)
                {
                    missingAssemblies.Add(assemblyName);
                }
            }

            if (config.hiddenAssemblies != null)
            {
                foreach (string hiddenAssembly in config.hiddenAssemblies)
                {
                    umlDocument.AppendLine("remove " + config.FormatAssemblyName(hiddenAssembly));
                }
            }

            config.outputUnusedAssemblies = new List<string>(unusedAssemblies).ToArray();
            config.outputMissingAssemblies = new List<string>(missingAssemblies).ToArray();
        }

        private static Assembly CreateAssemblyFromName(AssemblyName assemblyName)
        {
            return System.Reflection.Emit.AssemblyBuilder
                .DefineDynamicAssembly(assemblyName, System.Reflection.Emit.AssemblyBuilderAccess.ReflectionOnly);
        }

        private static string FormatAssemblyName(this Config config, Assembly assembly) => config.FormatAssemblyName(assembly.GetName());

        private static string FormatAssemblyName(this Config config, AssemblyName assemblyName) => config.FormatAssemblyName(assemblyName.Name);

        private static string FormatAssemblyName(this Config config, string assemblyName) => $"{config.GetAssemblyType(assemblyName)}." + assemblyName.Replace(" ", "_").Replace("-", "_").Replace(".", "_");

        private static AssemblyType GetAssemblyType(this Config config, Assembly assembly) => config.GetAssemblyType(assembly.GetName());

        private static AssemblyType GetAssemblyType(this Config config, AssemblyName assemblyName) => config.GetAssemblyType(assemblyName.Name);

        private static AssemblyType GetAssemblyType(this Config config, string assemblyName)
        {
            if (assemblyName.IsInArray(config.unityAssemblies)) return AssemblyType.Unity;
            if (assemblyName.IsInArray(config.thirdPartyAssemblies)) return AssemblyType.ThirdParty;
            if (assemblyName.IsInArray(config.thirdPartyEditorAssemblies)) return AssemblyType.ThirdPartyEditor;
            if (assemblyName.IsInArray(config.projectSharedAssemblies)) return AssemblyType.ProjectShared;
            if (assemblyName.IsInArray(config.projectAssemblies)) return AssemblyType.Project;
            if (assemblyName.IsInArray(config.projectEditorAssemblies)) return AssemblyType.ProjectEditor;
            if (assemblyName.IsInArray(selfAssemblies)) return AssemblyType.Self;
            return AssemblyType.Unknown;
        }

        private static string GetAssemblyBody(this Config config, Assembly assembly)
        {
            bool hasEngineReferences = Array.Exists(assembly.GetReferencedAssemblies(), x => x.Name == UnityCoreModuleAssemblyName);
            string result = hasEngineReferences ? "uses Unity" : "no Unity";

            result += ", ";
            result += config.GetAssemblyType(assembly).ToString();

            return result;
        }

        private static string GetAssemblyColor(this Config config, Assembly assembly, bool hasEngineReferences) => config.GetAssemblyColor(assembly.GetName(), hasEngineReferences);

        private static string GetAssemblyColor(this Config config, AssemblyName assemblyName, bool hasEngineReferences)
        {
            Color secondColor = hasEngineReferences ? new Color(0.20f, 0.05f, 0.05f) : new Color(0.05f, 0.20f, 0.05f);

            Color firstColor = config.GetAssemblyType(assemblyName).GetColor();

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

        private static List<string> GetAllMentionedAssemblies(this Config config)
        {
            List<string> allMentionedAssemblies = new List<string>();
            if (config.unityAssemblies != null) allMentionedAssemblies.AddRange(config.unityAssemblies);
            if (config.thirdPartyAssemblies != null) allMentionedAssemblies.AddRange(config.thirdPartyAssemblies);
            if (config.thirdPartyEditorAssemblies != null) allMentionedAssemblies.AddRange(config.thirdPartyEditorAssemblies);
            if (config.projectSharedAssemblies != null) allMentionedAssemblies.AddRange(config.projectSharedAssemblies);
            if (config.projectAssemblies != null) allMentionedAssemblies.AddRange(config.projectAssemblies);
            if (config.projectEditorAssemblies != null) allMentionedAssemblies.AddRange(config.projectEditorAssemblies);
            return allMentionedAssemblies;
        }

        private static string GetPathToConfig() => Path.Combine(Application.dataPath, "uml_generator_config.json");

        private static string GetPathToUMLFile() => Path.Combine(Application.dataPath, "../architecture.plantuml");
    }
}
