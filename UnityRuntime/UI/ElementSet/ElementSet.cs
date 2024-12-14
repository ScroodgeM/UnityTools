//this empty line for UTF-8 BOM header

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.UnityRuntime.Helpers;
using UnityTools.UnityRuntime.UI.Element;

namespace UnityTools.UnityRuntime.UI.ElementSet
{
    public class ElementSet : MonoBehaviour
    {
        internal bool DelayInitializeUntilElementBecomesVisible => delayInitializeUntilElementBecomesVisible;

        private event Action OnUpdate = () => { };

        [SerializeField] private ElementBase element;
        [SerializeField] private bool delayInitializeUntilElementBecomesVisible = false;

        public ElementSet<T> Typed<T>() where T : ElementBase
        {
            if (ValidateType<T>() == false)
            {
                return default;
            }

            ElementSet<T> elementSet = new ElementSet<T>(this, element as T);
            OnUpdate += () => elementSet.ProcessUpdate();
            return elementSet;
        }

        public ElementSetWithSelectableElements<T> TypedWithSelectableElements<T>() where T : ElementBase, ISelectableElement
        {
            if (ValidateType<T>() == false)
            {
                return default;
            }

            ElementSetWithSelectableElements<T> elementSet = new ElementSetWithSelectableElements<T>(this, element as T);
            OnUpdate += () => elementSet.ProcessUpdate();
            return elementSet;
        }

        private void Update()
        {
            OnUpdate();
        }

        private bool ValidateType<T>()
        {
            if (element == null)
            {
                Debug.LogError($"ElementSet {gameObject.name} can't init without starting element set", gameObject);
                return false;
            }

            if (element is T == false)
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
        private struct DelayedInitializeUntilElementBecomesVisible
        {
            public T elementBase;
            public RectTransform elementTransform;
            public int elementIndex;
            public Action<T, int> initializer;
        }

        private readonly ElementSet elementSet;
        private readonly RectTransform elementsHolder;
        private readonly T elementPrefab;

        private readonly List<T> elementsList = new List<T>();
        private readonly List<DelayedInitializeUntilElementBecomesVisible> delayedInitializes = new List<DelayedInitializeUntilElementBecomesVisible>();

        public ElementSet(ElementSet elementSet, T elementPrefab)
        {
            this.elementSet = elementSet;
            this.elementsHolder = elementSet.transform as RectTransform;
            this.elementPrefab = elementPrefab;
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
            delayedInitializes.Clear();

            for (int i = elementsList.Count; i < count; i++)
            {
                AddNew();
            }

            for (int i = 0; i < count; i++)
            {
                int elementIndex = i;
                T element = elementsList[elementIndex];
                element.SetVisible(true);

                if (initializer != null)
                {
                    if (elementSet.DelayInitializeUntilElementBecomesVisible == true)
                    {
                        DelayedInitializeUntilElementBecomesVisible delayedInitialize;
                        delayedInitialize.elementBase = elementsList[i];
                        delayedInitialize.elementTransform = elementsList[i].transform as RectTransform;
                        delayedInitialize.elementIndex = i;
                        delayedInitialize.initializer = initializer;
                        delayedInitializes.Add(delayedInitialize);
                    }
                    else
                    {
                        initializer(element, elementIndex);
                    }
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

        internal void ProcessUpdate()
        {
            for (int i = 0; i < delayedInitializes.Count; i++)
            {
                DelayedInitializeUntilElementBecomesVisible delayedInitialize = delayedInitializes[i];

                if (elementsHolder.Overlaps(delayedInitialize.elementTransform) != true)
                {
                    continue;
                }

                delayedInitialize.initializer(delayedInitialize.elementBase, delayedInitialize.elementIndex);
                delayedInitializes.RemoveAt(i);
                break;
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
