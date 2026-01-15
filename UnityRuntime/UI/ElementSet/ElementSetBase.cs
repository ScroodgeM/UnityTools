using System;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.UnityRuntime.UI.Element;

namespace UnityTools.UnityRuntime.UI.ElementSet
{
    public abstract class ElementSetBase<T> where T : ElementBase
    {
        public event Action<T> OnElementCreated = element => { };

        protected readonly ElementSet elementSet;
        protected readonly RectTransform elementsHolder;
        protected readonly RectTransform visibleFrame;
        protected readonly T elementPrefab;

        protected readonly List<T> elementsList = new List<T>();

        protected ElementSetBase(ElementSet elementSet, T elementPrefab)
        {
            this.elementSet = elementSet;
            this.elementsHolder = elementSet.transform as RectTransform;
            this.visibleFrame = elementSet.GetVisibleFrame();
            this.elementPrefab = elementPrefab;
        }

        public abstract void Init(int count, Action<T, int> initializer = null);

        public void AddAllChildElementsToTheList()
        {
            elementsList.Clear();
            elementsHolder.GetComponentsInChildren(true, elementsList);
        }

        public void Clear() => Init(0);

        public void SetAsLastSibling()
        {
            foreach (T element in elementsList)
            {
                element.transform.SetAsLastSibling();
            }
        }

        protected T AddNew()
        {
            T element = UnityEngine.Object.Instantiate(elementPrefab, elementsHolder, false);
            element.gameObject.hideFlags = HideFlags.DontSave;
            elementsList.Add(element);
            OnElementCreated(element);
            return element;
        }

        protected T AddNewFromStart()
        {
            T element = AddNew();
            elementsList.Insert(0, element);
            elementsList.RemoveAt(elementsList.Count - 1);
            return element;
        }
    }
}
