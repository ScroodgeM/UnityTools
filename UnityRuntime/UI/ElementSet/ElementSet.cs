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
        [SerializeField] private RectTransform customVisibleFrame;

        public ElementSet<T> Typed<T>() where T : ElementBase
        {
            if (ValidateType<T>() == false)
            {
                return null;
            }

            ElementSet<T> elementSet = new ElementSet<T>(this, element as T);
            OnUpdate += () => elementSet.ProcessUpdate();
            return elementSet;
        }

        public ElementSetWithSelectableElements<T> TypedWithSelectableElements<T>() where T : ElementBase, ISelectableElement
        {
            if (ValidateType<T>() == false)
            {
                return null;
            }

            ElementSetWithSelectableElements<T> elementSet = new ElementSetWithSelectableElements<T>(this, element as T);
            OnUpdate += () => elementSet.ProcessUpdate();
            return elementSet;
        }

        public ElementSetInfinite<T> TypedInfinite<T>(Vector2 elementSize, Vector2 elementStep) where T : ElementBase
        {
            if (ValidateType<T>() == false)
            {
                return null;
            }

            ElementSetInfinite<T> elementSet = new ElementSetInfinite<T>(this, element as T, elementSize, elementStep);
            OnUpdate += () => elementSet.ProcessUpdate();
            return elementSet;
        }

        internal RectTransform GetVisibleFrame()
        {
            if (customVisibleFrame != null)
            {
                return customVisibleFrame;
            }

            return transform as RectTransform;
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

    public class ElementSet<T> : ElementSetBase<T> where T : ElementBase
    {
        private struct DelayedInitializeUntilElementBecomesVisible
        {
            public T elementBase;
            public RectTransform elementTransform;
            public int elementIndex;
            public Action<T, int> initializer;
        }

        private readonly List<DelayedInitializeUntilElementBecomesVisible> delayedInitializes = new List<DelayedInitializeUntilElementBecomesVisible>();

        public ElementSet(ElementSet elementSet, T elementPrefab)
            : base(elementSet, elementPrefab)
        {
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

        public override void Init(int count, Action<T, int> initializer = null)
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

        public T GetElement(int index)
        {
            return index < elementsList.Count && index >= 0 ? elementsList[index] : default;
        }

        internal void ProcessUpdate()
        {
            for (int i = 0; i < delayedInitializes.Count; i++)
            {
                DelayedInitializeUntilElementBecomesVisible delayedInitialize = delayedInitializes[i];

                if (delayedInitialize.elementTransform.Overlaps(visibleFrame) == false)
                {
                    continue;
                }

                delayedInitialize.initializer(delayedInitialize.elementBase, delayedInitialize.elementIndex);
                delayedInitializes.RemoveAt(i);
                break;
            }
        }
    }
}
