
using UnityEditor;
using UnityEngine;
using UnityTools.UnityRuntime.UI.ElementSet;

namespace UnityTools.Editor
{
    [CustomEditor(typeof(ElementSet))]
    [CanEditMultipleObjects]
    public class ElementSetCustomInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Generate editor element"))
            {
                (target as ElementSet).GenerateTestElement();
            }
        }
    }
}
