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
            Myself = 3,
            Shared = 10,
            Project = 20,
        }

        [Serializable]
        private struct AsmDefStructure
        {
            public string name;
            public string[] references;
            public bool autoReferenced;
            public bool noEngineReferences;
        }

        private static string[] sharedLibraries;

        private static readonly string[] myLibraries = new string[]
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
            string pathToSharedLibrariesList = Path.Combine(Application.dataPath, "shared_libraries.txt");

            if (File.Exists(pathToSharedLibrariesList) == true)
            {
                sharedLibraries = File.ReadAllLines(pathToSharedLibrariesList);
            }
            else
            {
                File.WriteAllText(pathToSharedLibrariesList, string.Empty);
                sharedLibraries = new string[0];
            }
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
            List<AsmDefStructure> sortedAsmDefs = new List<AsmDefStructure>(asmDefs);
            sortedAsmDefs.Sort((a, b) => a.name.Length.CompareTo(b.name.Length));

            foreach (AsmDefStructure asmDef in sortedAsmDefs)
            {
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

                        if (GetLibraryType(referencedAssemblyDefinitionName) >= GetLibraryType(asmDef.name))
                        {
                            umlDocument += $"{referencedAssemblyDefinitionName.FormatAsmDefName()} <-- {asmDef.name.FormatAsmDefName()}{Environment.NewLine}";
                        }
                    }
                }

                umlDocument += Environment.NewLine;
            }
        }

        private static string FormatAsmDefName(this string name) => name.Replace("-", "_").Replace(".", "_");

        private static LibraryType GetLibraryType(string libraryName)
        {
            if (Array.Exists(myLibraries, x => x == libraryName))
            {
                return LibraryType.Myself;
            }

            if (Array.Exists(sharedLibraries, x => x == libraryName))
            {
                return LibraryType.Shared;
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
                case LibraryType.Myself:
                    result += ", UnityTools";
                    break;

                case LibraryType.Project:
                    break;

                case LibraryType.Shared:
                    result += ", Shared";
                    break;
            }

            return result;
        }

        private static string GetLibraryColor(AsmDefStructure library)
        {
            string secondColor = library.noEngineReferences ? "40fafa" : "fafa40";

            switch (GetLibraryType(library.name))
            {
                case LibraryType.Myself: return $"#ad2fff/{secondColor}";
                case LibraryType.Shared: return $"#adff2f/{secondColor}";
                case LibraryType.Project: return $"#ffffff/{secondColor}";
                default: return $"#808080/{secondColor}";
            }
        }

        private static string GetPathToUMLFile()
        {
            return Path.Combine(Application.dataPath, "../architecture.plantuml");
        }
    }
}
