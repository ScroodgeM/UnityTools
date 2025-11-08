using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEditorInternal;

namespace UnityTools.Editor
{
    public static class LibraryReferencesUmlGenerator
    {
        private enum LibraryType : byte
        {
            Self = 3,
            Unity = 10,
            ThirdParty = 20,
            ProjectShared = 30,
            Project = 40,
            ProjectEditor = 50,
        }

        [Serializable]
        private struct AsmDefStructure
        {
            public string name;
            public string[] references;
            public bool autoReferenced;
            public bool noEngineReferences;
        }

        [Serializable]
        private struct Config
        {
            public string[] unityLibraries;
            public string[] thirdPartyLibraries;
            public string[] projectSharedLibraries;
            public string[] projectEditorLibraries;
        }

        private static Config config;

        private static readonly string[] selfLibraries = new string[]
        {
            "com.Scroodge.UnityTools.Editor",
            "com.Scroodge.UnityTools.Runtime",
            "com.Scroodge.UnityTools.UnityRuntime",
            "com.Scroodge.UnityTools.Examples",
        };

        [MenuItem(nameof(UnityTools) + "/Generate UML with library references and open it in browser")]
        private static void GenerateUMLAndOpenItInBrowser()
        {
            GenerateUML();

            Process.Start("chrome.exe", $"--allow-file-access-from-files file://{GetPathToUMLFile()}");
        }

        [MenuItem(nameof(UnityTools) + "/Generate UML with library references")]
        private static void GenerateUML()
        {
            FillSharedLibraries();

            string umlDocument = "";

            umlDocument += "@startuml" + Environment.NewLine;

            umlDocument += "scale max 1920*1080" + Environment.NewLine;

            umlDocument += "!theme crt-green" + Environment.NewLine;

            umlDocument += Environment.NewLine;

            List<AsmDefStructure> asmDefs = new List<AsmDefStructure>();

            ParseDirectory(Application.dataPath, ref asmDefs);

            CreateUml(asmDefs, ref umlDocument);

            umlDocument += Environment.NewLine;

            umlDocument += "@enduml" + Environment.NewLine;

            File.WriteAllText(GetPathToUMLFile(), umlDocument);

            UnityEngine.Debug.Log("Generation success");
        }

        private static void FillSharedLibraries()
        {
            string pathToConfig = Path.Combine(Application.dataPath, "uml_generator_config.json");

            config = File.Exists(pathToConfig) ? JsonUtility.FromJson<Config>(File.ReadAllText(pathToConfig)) : default;

            File.WriteAllText(pathToConfig, JsonUtility.ToJson(config, true));
        }

        private static void ParseDirectory(string directoryPath, ref List<AsmDefStructure> asmDefs)
        {
            foreach (string filePath in Directory.GetFiles(directoryPath))
            {
                if (filePath.EndsWith(".asmdef"))
                {
                    asmDefs.Add(JsonUtility.FromJson<AsmDefStructure>(File.ReadAllText(filePath)));
                }
            }

            foreach (string subdirectoryPath in Directory.GetDirectories(directoryPath))
            {
                ParseDirectory(subdirectoryPath, ref asmDefs);
            }
        }

        private static void CreateUml(List<AsmDefStructure> asmDefs, ref string umlDocument)
        {
            asmDefs = new List<AsmDefStructure>(asmDefs);
            asmDefs.Sort((a, b) => a.name.FormatAsmDefName().CompareTo(b.name.FormatAsmDefName()));

            for (var i = 0; i < asmDefs.Count; i++)
            {
                AsmDefStructure asmDef = asmDefs[i];

                umlDocument += $"class {asmDef.name.FormatAsmDefName()} {GetLibraryColor(asmDef)} {{{Environment.NewLine}";

                umlDocument += GetLibraryBody(asmDef) + Environment.NewLine;

                umlDocument += "}" + Environment.NewLine;

                umlDocument += Environment.NewLine;

                if (asmDef.references != null)
                {
                    foreach (string reference in asmDef.references)
                    {
                        string referencedAssemblyDefinitionName;

                        const string guidHeader = "GUID:";
                        if (reference.StartsWith(guidHeader) == true)
                        {
                            string asmDefPath = AssetDatabase.GUIDToAssetPath(reference.Substring(guidHeader.Length));
                            if (string.IsNullOrEmpty(asmDefPath) == false)
                            {
                                AssemblyDefinitionAsset assemblyDefinition = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(asmDefPath);
                                referencedAssemblyDefinitionName = assemblyDefinition.name;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            referencedAssemblyDefinitionName = reference;
                        }

                        if (asmDefs.Exists(x => x.name == referencedAssemblyDefinitionName) == false)
                        {
                            asmDefs.Add(CreateAsmDefFromReference(referencedAssemblyDefinitionName));
                        }

                        if (GetLibraryType(referencedAssemblyDefinitionName) >= GetLibraryType(asmDef.name))
                        {
                            umlDocument += $"{referencedAssemblyDefinitionName.FormatAsmDefName()} <-- {asmDef.name.FormatAsmDefName()}{Environment.NewLine}";
                        }
                    }
                }

                umlDocument += Environment.NewLine;
            }
        }

        private static AsmDefStructure CreateAsmDefFromReference(string name)
        {
            AsmDefStructure result;
            result.name = name;
            result.noEngineReferences = false;
            result.autoReferenced = true;
            result.references = Array.Empty<string>();
            return result;
        }

        private static string FormatAsmDefName(this string name) => $"{GetLibraryType(name)}." + name.Replace("-", "_").Replace(".", "_");

        private static LibraryType GetLibraryType(string libraryName)
        {
            if (Array.Exists(selfLibraries, x => x == libraryName))
            {
                return LibraryType.Self;
            }

            if (config.unityLibraries != null && Array.Exists(config.unityLibraries, x => x == libraryName))
            {
                return LibraryType.Unity;
            }

            if (config.thirdPartyLibraries != null && Array.Exists(config.thirdPartyLibraries, x => x == libraryName))
            {
                return LibraryType.ThirdParty;
            }

            if (config.projectSharedLibraries != null && Array.Exists(config.projectSharedLibraries, x => x == libraryName))
            {
                return LibraryType.ProjectShared;
            }

            if (config.projectEditorLibraries != null && Array.Exists(config.projectEditorLibraries, x => x == libraryName))
            {
                return LibraryType.ProjectEditor;
            }

            return LibraryType.Project;
        }

        private static string GetLibraryBody(AsmDefStructure library)
        {
            string result = library.noEngineReferences ? "no Unity" : "uses Unity";

            if (library.autoReferenced == true)
            {
                result += ", auto-referenced";
            }

            switch (GetLibraryType(library.name))
            {
                case LibraryType.Self:
                    result += ", UnityTools";
                    break;

                case LibraryType.Unity:
                    result += ", Unity";
                    break;

                case LibraryType.ThirdParty:
                    result += ", ThirdParty";
                    break;

                case LibraryType.ProjectShared:
                    result += ", ProjectShared";
                    break;

                case LibraryType.ProjectEditor:
                    result += ", ProjectEditor";
                    break;

                case LibraryType.Project:
                    break;
            }

            return result;
        }

        private static string GetLibraryColor(AsmDefStructure library)
        {
            Color secondColor = library.noEngineReferences ? new Color(0.05f, 0.20f, 0.05f) : new Color(0.20f, 0.05f, 0.05f);

            Color firstColor;
            switch (GetLibraryType(library.name))
            {
                case LibraryType.Self:
                    firstColor = new Color(0.10f, 0.30f, 0.40f);
                    break;
                case LibraryType.Unity:
                    firstColor = new Color(0.40f, 0.10f, 0.30f);
                    break;
                case LibraryType.ThirdParty:
                    firstColor = new Color(0.40f, 0.30f, 0.10f);
                    break;
                case LibraryType.ProjectShared:
                    firstColor = new Color(0.30f, 0.40f, 0.10f);
                    break;
                case LibraryType.Project:
                    firstColor = new Color(0.40f, 0.40f, 0.40f);
                    break;
                case LibraryType.ProjectEditor:
                    firstColor = new Color(0.30f, 0.10f, 0.40f);
                    break;
                default:
                    firstColor = new Color(0.10f, 0.30f, 0.30f);
                    break;
            }

            return $"#{ColorUtility.ToHtmlStringRGB(firstColor)}/{ColorUtility.ToHtmlStringRGB(secondColor)}";
        }

        private static string GetPathToUMLFile()
        {
            return Path.Combine(Application.dataPath, "../architecture.plantuml");
        }
    }
}
