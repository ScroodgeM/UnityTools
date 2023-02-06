//this empty line for UTF-8 BOM header
using UnityEditor;
using UnityEngine;

namespace UnityTools.Editor.ElementSet
{
    [CustomEditor(typeof(UnityRuntime.UI.ElementSet.ElementSet))]
    [CanEditMultipleObjects]
    public class ElementSetCustomInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Generate editor element"))
            {
                (target as UnityRuntime.UI.ElementSet.ElementSet).GenerateTestElement();
            }
        }
    }
}
