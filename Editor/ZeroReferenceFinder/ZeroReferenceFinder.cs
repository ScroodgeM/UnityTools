//this empty line for UTF-8 BOM header
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Editor.ZeroReferenceFinder
{
    public class ZeroReferenceFinder : EditorWindow
    {
        [MenuItem(nameof(UnityTools) + "/Zero Reference Finder")]
        private static void Init()
        {
            ZeroReferenceFinder window = GetWindow(typeof(ZeroReferenceFinder)) as ZeroReferenceFinder;
            window.titleContent = new GUIContent("Zero Reference Finder Window");
            window.searchAssetsResults = null;
            window.Show();
        }

        private string searchPathFilter;
        private SearchAssetsResults searchAssetsResults;
        private Vector2 scrollPosition;

        private static readonly char[] trimChars = new char[] { ' ', '-' };

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            searchPathFilter = EditorGUILayout.TextField("Path Filter", searchPathFilter);

            if (GUILayout.Button("Search unused assets"))
            {
                SearchUnused();
            }

            GUILayout.Space(20f);

            if (searchAssetsResults != null)
            {
                searchAssetsResults.Show();
            }

            EditorGUILayout.EndScrollView();
        }

        private void SearchUnused()
        {
            try
            {
                // get all known assets
                HashSet<string> allUnusedPaths = new HashSet<string>();
                HashSet<string> allAssetPaths = new HashSet<string>();
                foreach (string guid in AssetDatabase.FindAssets("t:Object"))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);

                    allAssetPaths.Add(path);

                    if (string.IsNullOrEmpty(searchPathFilter) == false && path.ToLower().Contains(searchPathFilter.ToLower()) == false)
                    {
                        continue;
                    }

                    if (AssetDatabase.LoadMainAssetAtPath(path) is DefaultAsset)
                    {
                        continue;
                    }

                    if (path.StartsWith("Packages/"))
                    {
                        continue;
                    }

                    allUnusedPaths.Add(path);
                }

                Debug.Log("total assets found: " + allUnusedPaths.Count);

                // for each asset get deps
                int counter = 0;
                foreach (string assetPath in allAssetPaths)
                {
                    string progress = $"Searching... {counter + 1} of {allAssetPaths.Count} ({allUnusedPaths.Count} w/o dependency)";

                    if (EditorUtility.DisplayCancelableProgressBar("Searching...", progress, (float)counter / (float)allAssetPaths.Count))
                    {
                        searchAssetsResults = null;
                        throw new OperationCanceledException();
                    }

                    foreach (string dependency in AssetDatabase.GetDependencies(assetPath, false))
                    {
                        // don't count dependency on itself
                        if (dependency != assetPath)
                        {
                            allUnusedPaths.Remove(dependency);
                        }
                    }

                    counter++;
                }

                Debug.Log("total w/o dependency: " + allUnusedPaths.Count);

                searchAssetsResults = new SearchAssetsResults("assets that not used directly", allUnusedPaths, true);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                searchAssetsResults = null;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
