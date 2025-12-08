using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityTools.Runtime.Links;
using UnityTools.UnityRuntime.Links;

namespace UnityTools.Editor.Links
{
    public abstract class LinkToConfigPropertyDrawer : PropertyDrawer
    {
        private const string emptyLinkDisplayValue = "No Link";
        private const string refreshCommandDisplayValue = "Refresh...";

        private static readonly Dictionary<string, List<string>> valuesBufferDict = new Dictionary<string, List<string>>();

        protected static void DrawLink<T>(Rect position, SerializedProperty property, GUIContent label, string nameOfId) where T : UnityEngine.Object
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            DrawLinkToAssetGUI<T>(nameOfId, position, property);

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        private static void DrawLinkToAssetGUI<T>(string nameOfId, Rect position, SerializedProperty property) where T : UnityEngine.Object
        {
            if (property.hasMultipleDifferentValues == true)
            {
                EditorGUI.LabelField(position, "Editing multiple different links at once not supported");
                return;
            }

            string currentValue = property.FindPropertyRelative(nameOfId).stringValue;
            string typeKey = typeof(T).FullName;

            if (valuesBufferDict.ContainsKey(typeKey) == false)
            {
                valuesBufferDict.Add(typeKey, new List<string>());
                GetListOfAssets<T>(valuesBufferDict[typeKey], true);
            }

            List<string> valuesBuffer = valuesBufferDict[typeKey];

            T assetValue = GetAsset<T>(nameOfId, property);

            bool currentIsEmpty = currentValue == LinkBase.EmptyLinkKeyword;

            string currentDisplayValue = currentIsEmpty ? emptyLinkDisplayValue : currentValue;

            int currentValueIndex = valuesBuffer.IndexOf(currentDisplayValue);

            const float clearButtonWidth = 18f;
            const float showButtonWidth = 25f;

            float y = position.y;
            float h = position.height;

            float x1 = position.x;
            float w1 = position.width - showButtonWidth - clearButtonWidth;

            float x2 = x1 + w1;
            float w2 = showButtonWidth;

            float x3 = x2 + w2;
            float w3 = clearButtonWidth;

            bool valid = assetValue != null || currentIsEmpty;

            Color oldColor = GUI.color;
            GUI.color = valid ? GUI.color : Color.red;

            string[] displayOptions = FilterIfNeeded<T>(property, valuesBuffer);
            currentValueIndex = EditorGUI.Popup(new Rect(x1, y, w1, h), currentValueIndex, displayOptions);
            if (currentValueIndex >= 0 && currentValueIndex < valuesBuffer.Count)
            {
                if (valuesBuffer[currentValueIndex] == emptyLinkDisplayValue)
                {
                    property.FindPropertyRelative(nameOfId).stringValue = LinkBase.EmptyLinkKeyword;
                }
                else if (valuesBuffer[currentValueIndex] == refreshCommandDisplayValue)
                {
                    valuesBufferDict.Clear();
                    Debug.Log("Links cache refreshed");
                }
                else
                {
                    property.FindPropertyRelative(nameOfId).stringValue = valuesBuffer[currentValueIndex];
                }
            }

            GUI.color = oldColor;

            if (valid == false)
            {
                EditorGUI.LabelField(new Rect(x1, y, w1, h), $"Invalid link: " + (string.IsNullOrEmpty(currentValue) ? "null" : currentValue));
            }
            else
            {
                GUI.enabled = assetValue != null && currentIsEmpty == false;

                if (GUI.Button(new Rect(x2, y, w2, h), "go"))
                {
                    if (typeof(Component).IsAssignableFrom(typeof(T)))
                    {
                        EditorGUIUtility.PingObject((assetValue as Component).gameObject);
                    }
                    else
                    {
                        EditorGUIUtility.PingObject(assetValue);
                    }
                }

                if (GUI.Button(new Rect(x3, y, w3, h), "X"))
                {
                    property.FindPropertyRelative(nameOfId).stringValue = LinkBase.EmptyLinkKeyword;
                }
            }
        }

        protected static T GetAsset<T>(string nameOfId, SerializedProperty property) where T : UnityEngine.Object
        {
            return GetAsset<T>(property.FindPropertyRelative(nameOfId).stringValue);
        }

        private static T GetAsset<T>(string assetObjectId) where T : UnityEngine.Object
        {
            string assetPath = Path.Combine("Assets", LinkBase.GetResourcesPathForAsset<T>(), $"{assetObjectId}.{GetExtension<T>()}");

            if (typeof(Component).IsAssignableFrom(typeof(T)))
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(assetPath)?.GetComponent<T>();
            }

            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        internal static void GetListOfAssets<T>(List<string> buffer, bool addCommands)
        {
            string pathToSearch = Path.Combine(Application.dataPath, LinkBase.GetResourcesPathForAsset<T>());

            buffer.Clear();

            buffer.AddRange(Directory.GetFiles(pathToSearch, "*." + GetExtension<T>(), SearchOption.AllDirectories));

            for (int i = buffer.Count - 1; i >= 0; i--)
            {
                buffer[i] =
                    Path.Combine(Path.GetDirectoryName(buffer[i]), Path.GetFileNameWithoutExtension(buffer[i])) // get same path without extension
                        .Substring(pathToSearch.Length + 1) // remove path to config root && 1 separator
                        .Replace(Path.DirectorySeparatorChar, '/')
                        .Replace(Path.AltDirectorySeparatorChar, '/'); // replace all kind of path separators to unity-style path separator
            }

            if (addCommands == true)
            {
                buffer.Insert(0, emptyLinkDisplayValue);

                buffer.Add(refreshCommandDisplayValue);
            }
        }

        internal static string GetExtension<T>()
        {
            if (typeof(ScriptableObject).IsAssignableFrom(typeof(T)))
            {
                return "asset";
            }

            if (typeof(MonoBehaviour).IsAssignableFrom(typeof(T)))
            {
                return "prefab";
            }

            if (typeof(GameObject).IsAssignableFrom(typeof(T)))
            {
                return "prefab";
            }

            if (typeof(AudioClip).IsAssignableFrom(typeof(T)))
            {
                return "wav";
            }

            if (typeof(SceneAsset).IsAssignableFrom(typeof(T)))
            {
                return "unity";
            }

            return "*";
        }

        private static string[] FilterIfNeeded<T>(SerializedProperty sourceProperty, List<string> values) where T : UnityEngine.Object
        {
            string[] result = values.ToArray();

            Type propertyParentObjectType = sourceProperty.serializedObject.targetObject.GetType();

            FieldInfo propertyFieldInfo = GetPropertyFieldInfo(sourceProperty);

            if (propertyFieldInfo == null)
            {
                return result;
            }

            LinksDisplayFilterForInspectorAttribute filterAttribute =
                propertyFieldInfo.GetCustomAttribute<LinksDisplayFilterForInspectorAttribute>();

            if (filterAttribute == null)
            {
                return result;
            }

            BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            MethodInfo filterMethod = propertyParentObjectType.GetMethod(filterAttribute.filterMethodName, bindingFlags);

            if (filterMethod == null)
            {
                throw new InvalidOperationException($"LinkFilter static method '{filterAttribute.filterMethodName}' could not be found");
            }

            ParameterInfo[] parameters = filterMethod.GetParameters();

            if (parameters.Length != 1)
            {
                throw new InvalidOperationException($"LinkFilter method '{filterAttribute.filterMethodName}' should only have one parameter");
            }

            ParameterInfo parameter = parameters[0];

            if (parameter.ParameterType != typeof(T))
            {
                throw new InvalidOperationException($"LinkFilter method '{filterAttribute.filterMethodName}' parameter type '{parameter.ParameterType.Name}' must be of type '{typeof(T).Name}'");
            }

            if (filterMethod.ReturnType != typeof(bool))
            {
                throw new InvalidOperationException($"LinkFilter method '{filterAttribute.filterMethodName}' should return a boolean");
            }

            return Array.FindAll(result, FilterCheck);

            bool FilterCheck(string value)
            {
                if (value == emptyLinkDisplayValue || value == refreshCommandDisplayValue)
                {
                    return true;
                }

                T asset = GetAsset<T>(value);
                return (bool)filterMethod.Invoke(null, new[] { asset });
            }
        }

        private static FieldInfo GetPropertyFieldInfo(SerializedProperty sourceProperty)
        {
            string[] pathSegments = sourceProperty.propertyPath.Split('.');

            FieldInfo fieldInfo = null;

            Type currentType = sourceProperty.serializedObject.targetObject.GetType();

            for (int i = 0; i < pathSegments.Length; i++)
            {
                string segment = pathSegments[i];

                if (segment == "Array"
                    &&
                    pathSegments.Length > i + 1
                    &&
                    pathSegments[i + 1].StartsWith("data["))
                {
                    if (currentType.IsArray)
                    {
                        currentType = currentType.GetElementType();
                    }
                    else if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        currentType = currentType.GetGenericArguments()[0];
                    }

                    i++;
                    continue;
                }

                fieldInfo = currentType.GetField(segment, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (fieldInfo == null)
                {
                    Type baseType = currentType.BaseType;
                    while (baseType != null && fieldInfo == null)
                    {
                        fieldInfo = baseType.GetField(segment, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        baseType = baseType.BaseType;
                    }
                }

                if (fieldInfo == null)
                {
                    throw new InvalidOperationException($"FieldInfo for property '{sourceProperty.propertyPath}' could not be found");
                }

                currentType = fieldInfo.FieldType;
            }

            return fieldInfo;
        }
    }
}
