using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Editor.ZeroReferenceFinder
{
    public class SearchAssetsResults
    {
        private readonly string header = string.Empty;
        private readonly bool showDeleteButton = false;
        private string filter = string.Empty;
        private readonly Dictionary<string, List<string>> searchResults = new Dictionary<string, List<string>>();

        public SearchAssetsResults(string header, HashSet<string> results, bool showDeleteButton)
        {
            this.header = header;
            this.showDeleteButton = showDeleteButton;

            foreach (string result in results)
            {
                string extension;

                if (result.StartsWith("Assets/Resources/") == true)
                {
                    extension = "[Resources]";
                }
                else
                {
                    extension = System.IO.Path.GetExtension(result).ToLower();

                    if (string.IsNullOrEmpty(extension) == true)
                    {
                        extension = "<empty>";
                    }
                }

                if (searchResults.ContainsKey(extension) == false)
                {
                    searchResults.Add(extension, new List<string>());
                }

                searchResults[extension].Add(result);
            }
        }

        public void Show()
        {
            if (searchResults != null)
            {
                DrawHeader();
                DrawGroups();
                DrawAssets();
            }
        }

        private void DrawHeader()
        {
            GUILayout.Label(header);
            GUILayout.Label("total results: " + searchResults.Count);

            if (showDeleteButton == true)
            {
                GUI.color = Color.red;
                if (GUILayout.Button("DELETE ALL ASSETS LISTED BELOW") == true)
                {
                    foreach (KeyValuePair<string, List<string>> extension in searchResults)
                    {
                        foreach (string assetPath in extension.Value)
                        {
                            AssetDatabase.DeleteAsset(assetPath);
                        }
                    }
                    searchResults.Clear();
                }
                GUI.color = Color.white;
            }
        }

        private void DrawGroups()
        {
            GUILayout.Space(10f);

            int counter = 0;
            const int columns = 5;
            EditorGUILayout.BeginHorizontal();
            {
                foreach (KeyValuePair<string, List<string>> extension in searchResults)
                {
                    GUI.color = filter == extension.Key ? Color.green : Color.white;
                    if (GUILayout.Button($"{extension.Key} [{extension.Value.Count}]", GUILayout.Width(100f)))
                    {
                        filter = extension.Key;
                    }

                    if (GUILayout.Button("X", GUILayout.Width(20f)))
                    {
                        searchResults.Remove(extension.Key);
                        break;
                    }

                    counter++;
                    if (counter % columns == 0)
                    {
                        // new line
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }
                }
                GUI.color = Color.white;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawAssets()
        {
            if (string.IsNullOrEmpty(filter) == false && searchResults.ContainsKey(filter) == true)
            {
                GUILayout.Space(10f);

                foreach (string result in searchResults[filter])
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(">>>", GUILayout.Width(40f)))
                    {
                        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(result));
                    }
                    if (GUILayout.Button("X", GUILayout.Width(40f)))
                    {
                        AssetDatabase.DeleteAsset(result);
                        searchResults[filter].Remove(result);
                        AssetDatabase.Refresh();
                        break;
                    }
                    GUILayout.Label(result);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }
}