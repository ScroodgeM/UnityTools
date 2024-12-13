//this empty line for UTF-8 BOM header

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
                CheckObject(unityObject, includeIndirectDependants, deleteIfNoDependants);
            }

            EditorUtility.ClearProgressBar();
        }

        private static void CheckObject(Object unityObject, bool includeIndirectDependants, bool deleteIfNoDependants)
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

            System.DateTime startTime = System.DateTime.UtcNow;

            AnalyzeDependencies(includeIndirectDependants, myPath, out bool someDependencyFound, out bool stopSignal);

            if (stopSignal == false)
            {
                System.TimeSpan searchTime = System.DateTime.UtcNow - startTime;
                Debug.Log($"{pre}done in {searchTime.TotalSeconds} seconds!{post}");

                if (someDependencyFound == false && deleteIfNoDependants == true)
                {
                    AssetDatabase.DeleteAsset(myPath);
                    AssetDatabase.Refresh();
                }
            }
        }

        private static void AnalyzeDependencies(bool includeIndirectDependants, string myPath, out bool someDependencyFound, out bool stopSignal)
        {
            string[] allGuids = AssetDatabase.FindAssets("");

            Debug.Log($"{pre}stating search for dependants of {myPath} in {allGuids.Length} assets{post}");

            stopSignal = false;
            someDependencyFound = false;

            for (int i = 0, maxi = allGuids.Length - 1; i <= maxi && stopSignal == false; i++)
            {
                string candidatePath = AssetDatabase.GUIDToAssetPath(allGuids[i]);

                if (candidatePath == myPath) { continue; }

                bool directDependencyFound = false;

                // search only direct dependency
                if (HasDirectDependency(myPath, candidatePath) == true)
                {
                    LogDependency(candidatePath, true);
                    directDependencyFound = true;
                    someDependencyFound = true;
                }

                if (directDependencyFound == false && includeIndirectDependants == true && HasIndirectDependency(myPath, candidatePath) == true)
                {
                    LogDependency(candidatePath, false);
                    someDependencyFound = true;
                }

                if (i % 100 == 0)
                {
                    if (EditorUtility.DisplayCancelableProgressBar($"Searching dependants of {myPath}...", $"{i}/{allGuids.Length}", (float)i / maxi) == true)
                    {
                        Debug.LogError("search cancelled by user");
                        stopSignal = true;
                    }
                }
            }
        }

        private static bool HasDirectDependency(string asset, string dependant) => HasDependency(asset, dependant, false);

        private static bool HasIndirectDependency(string asset, string dependant) => HasDependency(asset, dependant, true);

        private static bool HasDependency(string asset, string dependant, bool recursive)
        {
            foreach (string dependency in AssetDatabase.GetDependencies(dependant, recursive))
            {
                if (dependency == asset)
                {
                    return true;
                }
            }

            return false;
        }

        private static void LogDependency(string candidatePath, bool directDependency)
        {
            Object candidate = AssetDatabase.LoadAssetAtPath(candidatePath, typeof(Object));
            string type = directDependency ? "direct" : "indirect";
            Debug.Log($"{pre}[{type}] {candidatePath}{post}", candidate);
        }
    }
}
