
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.UnityRuntime.ElementSet
{
    public class ElementSet : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour element;

        public ElementSet<T> Typed<T>() where T : MonoBehaviour
        {
            if (element == null)
            {
                Debug.LogError($"ElementSet {gameObject.name} can't init without starting element set", gameObject);
                return default;
            }
            else if (element is T)
            {
                return new ElementSet<T>(this, element as T);
            }
            else
            {
                Debug.LogError($"ElementSet {gameObject.name} can't init with incorrect element type", gameObject);
                return default;
            }
        }

        public ElementSetWithSelectableElements<T> TypedWithSelectableElements<T>() where T : MonoBehaviour, ISelectableElement
        {
            if (element == null)
            {
                Debug.LogError($"ElementSet {gameObject.name} can't init without starting element set", gameObject);
                return default;
            }
            else if (element is T)
            {
                return new ElementSetWithSelectableElements<T>(this, element as T);
            }
            else
            {
                Debug.LogError($"ElementSet {gameObject.name} can't init with incorrect element type", gameObject);
                return default;
            }
        }

#if UNITY_EDITOR
        public GameObject GenerateTestElement()
        {
            GameObject go = Instantiate(element, transform).gameObject;
            foreach (MonoBehaviour scriptComponent in go.GetComponentsInChildren<MonoBehaviour>())
            {
                //So the user's Awake() won't be called
                scriptComponent.enabled = false;
            }
            go.hideFlags = HideFlags.DontSave;
            go.SetActive(true);
            return go;
        }
#endif
    }

    public class ElementSet<T> where T : MonoBehaviour
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

        public void Init(int count, Action<T, int> initializer)
        {
            for (int i = elementsList.Count; i < count; i++)
            {
                AddNew();
            }

            for (int i = 0; i < count; i++)
            {
                elementsList[i].gameObject.SetActive(true);

                if (initializer != null)
                {
                    initializer(elementsList[i], i);
                }
            }

            for (int i = count; i < elementsList.Count; i++)
            {
                elementsList[i].gameObject.SetActive(false);
            }
        }

        public void Clear()
        {
            Init(0, null);
        }

        public T GetElement(int index)
        {
            return index < elementsList.Count && index >= 0 ? elementsList[index] : default;
        }

        public void SetAsLastSibling()
        {
            foreach (T element in elementsList)
            {
                if (element.gameObject.activeSelf)
                {
                    element.transform.SetAsLastSibling();
                }
            }
        }

        private void AddNew()
        {
            T element = UnityEngine.Object.Instantiate(elementPrefab, elementsHolder, false) as T;
            element.gameObject.hideFlags = HideFlags.DontSave;
            element.gameObject.SetActive(false);
            elementsList.Add(element);
        }
    }
}
