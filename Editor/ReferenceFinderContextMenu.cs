
using UnityEditor;
using UnityEngine;

namespace UnityTools.Editor
{
    public static class ReferenceFinderContextMenu
    {
        private const string pre = "<color=#088>";
        private const string post = "</color>";

        [MenuItem("Assets/Search this asset's dependants in whole project (only direct)")]
        public static void Search1() => Search(false, false);

        [MenuItem("Assets/Search this asset's dependants in whole project (include indirect)")]
        public static void Search2() => Search(true, false);

        [MenuItem("Assets/Search this asset's dependants in whole project and delete if no dependants")]
        public static void Search3() => Search(true, true);

        private static void Search(bool includeIndirectDependants, bool deleteIfNoDependants)
        {
            if (Selection.objects == null || Selection.objects.Length == 0)
            {
                Debug.LogError("select asset to find dependants");
                return;
            }

            Object[] objects = Selection.objects;

            foreach (Object unityObject in objects)
            {
                Check(unityObject, includeIndirectDependants, deleteIfNoDependants);
            }

            EditorUtility.ClearProgressBar();
        }

        private static void Check(Object unityObject, bool includeIndirectDependants, bool deleteIfNoDependants)
        {
            if (unityObject is GameObject)
            {
                UnityEngine.SceneManagement.Scene goScene = ((GameObject)unityObject).scene;

                if (goScene != null && string.IsNullOrEmpty(goScene.name) == false)
                {
                    Debug.LogError($"object should be in project view, not in scene '{goScene.name}'");
                    return;
                }
            }

            string myPath = AssetDatabase.GetAssetPath(unityObject);

            string[] allGuids = AssetDatabase.FindAssets("");

            Debug.Log($"{pre}stating search for dependants of {myPath} in {allGuids.Length} assets{post}");

            System.DateTime startTime = System.DateTime.UtcNow;

            bool someDependencyFound = false;

            for (int i = 0, maxi = allGuids.Length - 1; i <= maxi; i++)
            {
                string candidatePath = AssetDatabase.GUIDToAssetPath(allGuids[i]);

                if (candidatePath == myPath) { continue; }

                Object candidate = AssetDatabase.LoadAssetAtPath(candidatePath, typeof(Object));

                bool directDependencyFound = false;

                // search only direct dependency
                foreach (string dependency in AssetDatabase.GetDependencies(candidatePath, false))
                {
                    if (dependency == myPath)
                    {
                        LogDependency(true);
                        directDependencyFound = true;
                        break;
                    }
                }

                if (directDependencyFound == false && includeIndirectDependants == true)
                {
                    // search all other dependencies (incl. non-direct)
                    foreach (string dependency in AssetDatabase.GetDependencies(candidatePath, true))
                    {
                        if (dependency == myPath)
                        {
                            LogDependency(false);
                            break;
                        }
                    }
                }

                void LogDependency(bool directDependency)
                {
                    string type = directDependency ? "direct" : "indirect";
                    Debug.Log($"{pre}[{type}] {candidatePath}{post}", candidate);
                    someDependencyFound = true;
                }

                if (i % 100 == 0)
                {
                    if (EditorUtility.DisplayCancelableProgressBar($"Searching dependants of {myPath}...", $"{i}/{allGuids.Length}", (float)i / maxi) == true)
                    {
                        Debug.LogError("search cancelled by user");
                        break;
                    }
                }

                if (i == maxi)
                {
                    System.TimeSpan searchTime = System.DateTime.UtcNow - startTime;

                    Debug.Log($"{pre}done in {searchTime.TotalSeconds} seconds!{post}");
                }
            }

            if (someDependencyFound == false && deleteIfNoDependants)
            {
                AssetDatabase.DeleteAsset(myPath);
                AssetDatabase.Refresh();
            }
        }
    }
}
