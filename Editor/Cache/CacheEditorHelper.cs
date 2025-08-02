using UnityEditor;
using UnityEngine;

namespace UnityTools.Editor.Cache
{
    public static class CacheEditorHelper
    {
        [MenuItem(nameof(UnityTools) + "/Open cache storage folder")]
        public static void AnalyzeCode()
        {
            EditorUtility.RevealInFinder(System.IO.Path.Combine(Application.persistentDataPath, "*"));
        }
    }
}
