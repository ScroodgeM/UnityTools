using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityTools.Runtime.CodeAnalyzer;

namespace UnityTools.Editor.CodeAnalyzer
{
    public static class CodeAnalyzer
    {
        private const string rootFolder = "Assets";
        private const string scriptsFolder = "Scripts";
        private const int methodBodyLengthLimit = 20;
        private const string yellow = "<color=yellow>";
        private const string green = "<color=green>";
        private const string colorend = "</color>";
        private const int methodParametersCountLimit = 2;
        private const int methodOutParametersCountLimit = 1;

        private static string rootNamespace => EditorSettings.projectGenerationRootNamespace;

        private enum ErrorType
        {
            MethodTooLong,
            NamespaceMissing,
            FormatIncorrect,
            NotImplemented,
            ToDo,
            MethodWithTooManyParameters,
            MethodWithTooManyOutParameters,
            SwitchConditionDuplicate,
        }

        private class FolderStatistic
        {
            public int LinesCount { get; private set; }
            public int FilesCount { get; private set; }
            public void AddFile(int linesCount)
            {
                FilesCount++;
                LinesCount += linesCount;
            }
        }

        [MenuItem(nameof(UnityTools) + "/Analyze Code")]
        public static void AnalyzeCode()
        {
            Dictionary<string, FolderStatistic> statisticPerFolder = new Dictionary<string, FolderStatistic>();
            Dictionary<string, List<string>> switchConditions = new Dictionary<string, List<string>>();
            List<(uint, Action)> methodTooLongMessages = new List<(uint, Action)>();

            foreach (string filePath in GetAllCSharpFiles(rootFolder))
            {
                string folder = GetFolderFromPath(filePath, 1);
                string[] lines = File.ReadAllLines(filePath);

                if (folder == scriptsFolder)
                {
                    AnalyzeCSharpFile(filePath, ref lines, ref switchConditions, ref methodTooLongMessages);
                    File.WriteAllLines(filePath, lines, new UTF8Encoding(true));
                }

                WriteStatistic(ref statisticPerFolder, rootFolder, lines.Length);
                WriteStatistic(ref statisticPerFolder, folder, lines.Length);
            }

            PrintMethodTooLongErrors(methodTooLongMessages);

            AnalyzeMethodsSignature();

            PrintStatistic(statisticPerFolder);
        }

        private static void AnalyzeCSharpFile(string filePath, ref string[] lines, ref Dictionary<string, List<string>> switchConditions, ref List<(uint, Action)> methodTooLongMessages)
        {
            RemoveEmptyDoubleLines(ref lines);
            RemoveEmptyLinesAtTheBegin(ref lines);
            RemoveEmptyLinesAtTheEnd(ref lines);
            RemoveTrailingSpaces(ref lines);
            AnalyzeToDo(filePath, ref lines);
            AnalyzeNotImplemented(filePath, ref lines);
            AnalyzeNamespace(filePath, ref lines);
            AnalyzeLongMethods(filePath, ref lines, ref methodTooLongMessages);
            AnalyzeSwitchConditions(filePath, ref lines, ref switchConditions);
        }

        private static void RemoveEmptyDoubleLines(ref string[] lines)
        {
            for (int i = 1; i < lines.Length; i++)
            {
                bool prevLineEmpty = string.IsNullOrEmpty(lines[i - 1]);
                bool thisLineEmpty = string.IsNullOrEmpty(lines[i]);

                if (prevLineEmpty == true && thisLineEmpty == true)
                {
                    ArrayUtility.RemoveAt(ref lines, i);
                    i--;
                }
            }
        }

        private static void RemoveEmptyLinesAtTheBegin(ref string[] lines)
        {
            while (lines.Length > 0 && string.IsNullOrEmpty(lines[0]) == true)
            {
                ArrayUtility.RemoveAt(ref lines, 0);
            }
        }

        private static void RemoveEmptyLinesAtTheEnd(ref string[] lines)
        {
            while (lines.Length > 0 && string.IsNullOrEmpty(lines[lines.Length - 1]) == true)
            {
                ArrayUtility.RemoveAt(ref lines, lines.Length - 1);
            }
        }

        private static void RemoveTrailingSpaces(ref string[] lines)
        {
            char[] space = new char[] { ' ' };
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].TrimEnd(space);
            }
        }

        private static void AnalyzeToDo(string filePath, ref string[] lines)
        {
            for (uint lineCounter = 0; lineCounter < lines.Length; lineCounter++)
            {
                string line = lines[lineCounter];
                if (line.Contains("todo:", StringComparison.OrdinalIgnoreCase))
                {
                    PrintError(CreateErrorMessage(ErrorType.ToDo, line), PathToFileWithLineNumber(filePath, lineCounter));
                }
            }
        }

        private static void AnalyzeNotImplemented(string filePath, ref string[] lines)
        {
            for (uint lineCounter = 0; lineCounter < lines.Length; lineCounter++)
            {
                string line = lines[lineCounter];
                if (line.Contains("not", StringComparison.OrdinalIgnoreCase) && line.Contains("implemented", StringComparison.OrdinalIgnoreCase))
                {
                    PrintError(CreateErrorMessage(ErrorType.NotImplemented, line), PathToFileWithLineNumber(filePath, lineCounter));
                }
            }
        }

        private static void AnalyzeNamespace(string filePath, ref string[] lines)
        {
            string expectedNamespace = GetNamespaceFromPath(filePath);
            uint matchCounter = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.StartsWith("namespace"))
                {
                    lines[i] = $"namespace {expectedNamespace}";
                    matchCounter++;
                    if (i > 0 && string.IsNullOrEmpty(lines[i - 1]) == false)
                    {
                        ArrayUtility.Insert(ref lines, i, "");
                        i++;
                    }
                }
            }
            if (matchCounter != 1)
            {
                string msg = $"namespace missing ({expectedNamespace})";
                PrintError(CreateErrorMessage(ErrorType.NamespaceMissing, msg), filePath);
            }
        }

        private static void AnalyzeLongMethods(string filePath, ref string[] lines, ref List<(uint, Action)> methodTooLongMessages)
        {
            const int methodBodyDepthLevel = 3;

            int depthLevel = 0;
            Stack<uint> perLevelStartLineNumber = new Stack<uint>();
            for (uint lineCounter = 0; lineCounter < lines.Length; lineCounter++)
            {
                string line = lines[lineCounter].Trim();
                if (string.IsNullOrEmpty(line) == false)
                {
                    if (line[0] == '{')
                    {
                        depthLevel++;

                        if (depthLevel == methodBodyDepthLevel)
                        {
                            perLevelStartLineNumber.Push(lineCounter + 1);
                        }
                    }
                    if (line[0] == '}')
                    {
                        if (depthLevel == methodBodyDepthLevel)
                        {
                            uint bodyStartLine = perLevelStartLineNumber.Pop();
                            uint bodyEndLine = lineCounter - 1;
                            uint bodyLength = bodyEndLine - bodyStartLine + 1;

                            if (bodyLength > methodBodyLengthLimit)
                            {
                                string methodName = lines[bodyStartLine - 2].Trim();
                                Action printCommand = () =>
                                {
                                    string msg = $"({bodyLength} / {methodBodyLengthLimit}) [{methodName}]";
                                    PrintError(CreateErrorMessage(ErrorType.MethodTooLong, msg), PathToFileWithLineNumber(filePath, bodyStartLine - 1));
                                };
                                methodTooLongMessages.Add((bodyLength, printCommand));
                            }
                        }

                        depthLevel--;
                    }
                }
            }

            if (depthLevel != 0)
            {
                PrintError(CreateErrorMessage(ErrorType.FormatIncorrect, $"bracers pairing fail, depth level at the end is {depthLevel}"), filePath);
            }
        }

        private static void AnalyzeSwitchConditions(string filePath, ref string[] lines, ref Dictionary<string, List<string>> switches)
        {
            for (uint lineCounter = 0; lineCounter < lines.Length; lineCounter++)
            {
                string line = lines[lineCounter].Trim();
                if (string.IsNullOrEmpty(line) == false && line.StartsWith("switch"))
                {
                    if (switches.TryGetValue(line, out List<string> usages) == false)
                    {
                        usages = new List<string>();
                        switches.Add(line, usages);
                    }

                    usages.Add(filePath);

                    if (usages.Count >= 2)
                    {
                        PrintError(CreateErrorMessage(ErrorType.SwitchConditionDuplicate, $"switch line: {line}, total copies: {usages.Count}"), PathToFileWithLineNumber(filePath, lineCounter));
                    }
                }
            }
        }

        private static void WriteStatistic(ref Dictionary<string, FolderStatistic> statisticPerFolder, string folder, int linesCount)
        {
            if (statisticPerFolder.ContainsKey(folder) == false)
            {
                statisticPerFolder.Add(folder, new FolderStatistic());
            }

            statisticPerFolder[folder].AddFile(linesCount);
        }

        private static void PrintMethodTooLongErrors(List<(uint, Action)> methodTooLongMessages)
        {
            methodTooLongMessages.Sort((a, b) => { return -a.Item1.CompareTo(b.Item1); });
            const int limit = 5;
            int counter = 0;
            foreach ((uint, Action) message in methodTooLongMessages)
            {
                counter++;
                if (counter > limit)
                {
                    Debug.Log($"Only first {limit} methods shown");
                    break;
                }

                message.Item2();
            }
        }

        private static void AnalyzeMethodsSignature()
        {
            const BindingFlags methodsSearchFlags =
                BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Namespace != null && type.Namespace.StartsWith(rootNamespace) == true)
                    {
                        foreach (MethodInfo method in type.GetMethods(methodsSearchFlags))
                        {
                            ParameterInfo[] parameters = method.GetParameters();

                            if (method.GetCustomAttribute<SuppressMethodWithTooManyParametersWarningAttribute>() == null)
                            {
                                int parametersCount = parameters.Length;
                                if (parametersCount > methodParametersCountLimit)
                                {
                                    string message = $"recommended: {methodParametersCountLimit}, actual: {parametersCount}";
                                    PrintError(CreateErrorMessage(ErrorType.MethodWithTooManyParameters, message), $"{type.FullName} {method.Name}");
                                }
                            }

                            int outParametersCount = 0;
                            foreach (ParameterInfo parameter in parameters)
                            {
                                if (parameter.IsOut == true)
                                {
                                    outParametersCount++;
                                }
                            }
                            if (outParametersCount > methodOutParametersCountLimit)
                            {
                                string message = $"recommended: {methodOutParametersCountLimit}, actual: {outParametersCount}";
                                PrintError(CreateErrorMessage(ErrorType.MethodWithTooManyOutParameters, message), $"{type.FullName} {method.Name}");
                            }
                        }
                    }
                }
            }
        }

        private static void PrintStatistic(Dictionary<string, FolderStatistic> statisticPerFolder)
        {
            int totalLinesCount = statisticPerFolder[rootFolder].LinesCount;

            foreach (KeyValuePair<string, FolderStatistic> folderStatistic in statisticPerFolder)
            {
                string folder = folderStatistic.Key;
                if (folder != rootFolder)
                {
                    int filesCount = folderStatistic.Value.FilesCount;
                    int linesCount = folderStatistic.Value.LinesCount;
                    float linesCountPerc = 100f * (float)linesCount / (float)totalLinesCount;
                    string folderInYellow = $"{yellow}{folder}{colorend}";
                    string linesCountInYellow = $"{yellow}{linesCount}{colorend}";
                    string filesCountInYellow = $"{yellow}{filesCount}{colorend}";
                    string linesCountPercentInYellow = $"{yellow}{linesCountPerc:0.0}%{colorend}";
                    Debug.Log($"[{green}Statistic{colorend}] [{folderInYellow}] {linesCountInYellow} lines in {filesCountInYellow} files ({linesCountPercentInYellow})");
                }
            }
        }

        private static string CreateErrorMessage(ErrorType warnType, string message)
        {
            return $"[{green}{warnType}{colorend}] {message}";
        }

        private static string PathToFileWithLineNumber(string pathToFile, uint lineNumber)
        {
            return $"{pathToFile}:{lineNumber}";
        }

        private static void PrintError(string message, string pathToFile)
        {
            Debug.LogError($"{message} ({yellow}{pathToFile.Replace('\\', '/')}{colorend})");
        }

        private static IEnumerable<string> GetAllCSharpFiles(string rootFolder)
        {
            foreach (string file in Directory.GetFiles(rootFolder))
            {
                if (file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    yield return file;
                }
            }
            foreach (string folder in Directory.GetDirectories(rootFolder))
            {
                foreach (string file in GetAllCSharpFiles(folder))
                {
                    yield return file;
                }
            }
        }

        private static string GetNamespaceFromPath(string filePath)
        {
            string result = rootNamespace;
            int level = 2; // skip assets/scripts/
            while (true)
            {
                string namespacePart = GetFolderFromPath(filePath, level);
                if (string.IsNullOrEmpty(namespacePart) == true)
                {
                    return result;
                }
                else
                {
                    result += $".{namespacePart}";
                    level++;
                }
            }
        }

        private static readonly char[] pathSeparators = new char[] { Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar };
        private static string GetFolderFromPath(string filePath, int folderLevel)
        {
            string[] path = Path.GetDirectoryName(filePath).Split(pathSeparators);
            if (path.Length > folderLevel)
            {
                return path[folderLevel];
            }
            return string.Empty;
        }
    }
}
