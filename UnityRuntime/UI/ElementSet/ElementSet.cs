//this empty line for UTF-8 BOM header

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.UnityRuntime.UI.Element;

namespace UnityTools.UnityRuntime.UI.ElementSet
{
    public class ElementSet : MonoBehaviour
    {
        [SerializeField] private ElementBase element;

        public ElementSet<T> Typed<T>() where T : ElementBase
        {
            if (ValidateType<T>() == false)
            {
                return default;
            }

            return new ElementSet<T>(this, element as T);
        }

        public ElementSetWithSelectableElements<T> TypedWithSelectableElements<T>() where T : ElementBase, ISelectableElement
        {
            if (ValidateType<T>() == false)
            {
                return default;
            }

            return new ElementSetWithSelectableElements<T>(this, element as T);
        }

        private bool ValidateType<T>()
        {
            if (element == null)
            {
                Debug.LogError($"ElementSet {gameObject.name} can't init without starting element set", gameObject);
                return false;
            }

            if ((element is T) == false)
            {
                Debug.LogError($"ElementSet {gameObject.name} can't init with incorrect element type", gameObject);
                return false;
            }

            return true;
        }

#if UNITY_EDITOR
        public GameObject GenerateTestElement()
        {
            GameObject go = Instantiate(element, transform).gameObject;
            foreach (MonoBehaviour scriptComponent in go.GetComponentsInChildren<MonoBehaviour>())
            {
                if (scriptComponent is UnityEngine.UI.Graphic == false)
                {
                    //So the user's Awake() won't be called
                    scriptComponent.enabled = false;
                }
            }
            go.hideFlags = HideFlags.DontSave;
            go.SetActive(true);
            return go;
        }
#endif
    }

    public class ElementSet<T> where T : ElementBase
    {
        private readonly T elementPrefab;
        private readonly Transform elementsHolder;

        protected readonly List<T> elementsList = new List<T>();

        public ElementSet(ElementSet wrapper, T elementPrefab)
        {
            this.elementPrefab = elementPrefab;
            this.elementsHolder = wrapper.transform;
        }

        public IEnumerable<T> ActiveElements
        {
            get
            {
                foreach (T element in elementsList)
                {
                    if (element.gameObject.activeSelf)
                    {
                        yield return element;
                    }
                }
            }
        }

        public void AddAllChildElementsToTheList()
        {
            elementsList.Clear();
            elementsHolder.GetComponentsInChildren(true, elementsList);
        }

        public void Init(int count, Action<T, int> initializer = null)
        {
            for (int i = elementsList.Count; i < count; i++)
            {
                AddNew();
            }

            for (int i = 0; i < count; i++)
            {
                elementsList[i].SetVisible(true);

                if (initializer != null)
                {
                    initializer(elementsList[i], i);
                }
            }

            for (int i = count; i < elementsList.Count; i++)
            {
                elementsList[i].SetVisible(false);
            }
        }

        public void Clear()
        {
            Init(0);
        }

        public T GetElement(int index)
        {
            return index < elementsList.Count && index >= 0 ? elementsList[index] : default;
        }

        public void SetAsLastSibling()
        {
            foreach (T element in elementsList)
            {
                element.transform.SetAsLastSibling();
            }
        }

        private void AddNew()
        {
            T element = UnityEngine.Object.Instantiate(elementPrefab, elementsHolder, false);
            element.gameObject.hideFlags = HideFlags.DontSave;
            elementsList.Add(element);
        }
    }
}
