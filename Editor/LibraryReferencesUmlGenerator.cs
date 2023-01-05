
using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace UnityTools.Editor
{
    public static class LibraryReferencesUmlGenerator
    {
        private enum LibraryType : byte
        {
            Shared = 10,
            Project = 20,
        }

        [Serializable]
        private struct AsmDefStructure
        {
            public string name;
            public string[] references;
            public bool noEngineReferences;
        }

        private static readonly string[] sharedLibraries = new string[]
        {
        };

        private static readonly string[] unityLibraries = new string[]
        {
            "com.Scroodge.UnityTools.Editor",
            "com.Scroodge.UnityTools.Runtime",
            "com.Scroodge.UnityTools.UnityRuntime",
            "Unity.TextMeshPro",
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
            string umlDocument = "";

            umlDocument += "@startuml" + Environment.NewLine;

            umlDocument += "scale max 1920*1080" + Environment.NewLine;

            umlDocument += Environment.NewLine;

            List<AsmDefStructure> asmDefs = new List<AsmDefStructure>();

            ParseDirectory(Application.dataPath, ref asmDefs);

            CreateUml(asmDefs, ref umlDocument);

            umlDocument += Environment.NewLine;

            foreach (string editorLibrary in unityLibraries)
            {
                umlDocument += $"remove {editorLibrary}{Environment.NewLine}";
            }

            umlDocument += Environment.NewLine;

            umlDocument += "@enduml" + Environment.NewLine;

            File.WriteAllText(GetPathToUMLFile(), umlDocument);

            UnityEngine.Debug.Log("Generation success");
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
            foreach (AsmDefStructure asmDef in asmDefs)
            {
                umlDocument += $"class {asmDef.name} {GetLibraryColor(asmDef)} {{{Environment.NewLine}";

                umlDocument += "}" + Environment.NewLine;

                umlDocument += Environment.NewLine;

                if (asmDef.references != null)
                {
                    foreach (string reference in asmDef.references)
                    {
                        if (GetLibraryType(reference) >= GetLibraryType(asmDef.name))
                        {
                            umlDocument += $"{reference} <-- {asmDef.name}{Environment.NewLine}";
                        }
                    }
                }

                umlDocument += Environment.NewLine;
            }
        }

        private static LibraryType GetLibraryType(string libraryName)
        {
            if (Array.Exists(sharedLibraries, x => x == libraryName))
            {
                return LibraryType.Shared;
            }

            return LibraryType.Project;
        }

        private static string GetLibraryColor(AsmDefStructure library)
        {
            string secondColor = library.noEngineReferences ? "40fafa" : "fafa40";

            switch (GetLibraryType(library.name))
            {
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
