using System;
using UnityEngine;
using UnityEngine.UI;
using UnityTools.UnityRuntime.Helpers;
using UnityTools.UnityRuntime.UI.Element;

namespace UnityTools.UnityRuntime.UI.ElementSet
{
    public class ElementSetInfinite : ElementSet
    {
        [SerializeField] private Vector2 elementSize;
        [SerializeField] private Vector2 elementStep;

        public ElementSetInfinite<T> TypedInfinite<T>() where T : ElementBase
        {
            if (ValidateType<T>() == false)
            {
                return null;
            }

            ElementSetInfinite<T> elementSet = new ElementSetInfinite<T>(this, element as T, elementSize, elementStep);
            OnUpdate += () => elementSet.ProcessUpdate();
            return elementSet;
        }

        public ElementSetInfiniteWithSelectableElements<T> TypedInfiniteWithSelectableElements<T>() where T : ElementBase, ISelectableElement
        {
            if (ValidateType<T>() == false)
            {
                return null;
            }

            ElementSetInfiniteWithSelectableElements<T> elementSet = new ElementSetInfiniteWithSelectableElements<T>(this, element as T, elementSize, elementStep);
            OnUpdate += () => elementSet.ProcessUpdate();
            return elementSet;
        }

        private void OnValidate()
        {
            if (GetComponent<ContentSizeFitter>() != null
                ||
                GetComponent<AspectRatioFitter>() != null
                ||
                GetComponent<LayoutGroup>() != null)
            {
                Debug.LogError("please remove all fitters and layouts from ElementSetInfinite's Game Object. it will handle it for you.", this);
            }
        }
    }

    public class ElementSetInfinite<T> : ElementSetBase<T> where T : ElementBase
    {
        private readonly Vector2 elementSize;
        private readonly Vector2 elementStep;

        private bool elementsLoopIsActive = false;
        private Action<T, int> initializerCache;
        private int firstLocalElementGlobalIndex = 0;
        private int totalElementsCount = 0;

        public ElementSetInfinite(ElementSet elementSet, T elementPrefab, Vector2 elementSize, Vector2 elementStep)
            : base(elementSet, elementPrefab)
        {
            this.elementSize = elementSize;
            this.elementStep = elementStep;
        }

        public override void Init(int count, Action<T, int> initializer = null)
        {
            this.initializerCache = initializer;
            this.totalElementsCount = count;
            this.firstLocalElementGlobalIndex =
                elementsList.Count >= count
                    ? 0
                    : Mathf.Clamp(firstLocalElementGlobalIndex, 0, totalElementsCount - elementsList.Count);

            elementsHolder.anchorMin = Vector2.up;
            elementsHolder.anchorMax = Vector2.up;
            elementsHolder.pivot = Vector2.up;
            elementsHolder.sizeDelta = elementSize + elementStep * (count - 1);

            if (elementsList.Count == 0 && count > 0)
            {
                AddNew();
            }

            int elementsCountToInit = Mathf.Min(count, elementsList.Count);
            for (int i = 0; i < elementsCountToInit; i++)
            {
                Reinit(i);
            }

            for (int i = count; i < elementsList.Count; i++)
            {
                elementsList[i].SetVisible(false);
            }

            elementsLoopIsActive = count > elementsList.Count;
        }

        public bool TryGetElement(int index, out T element)
        {
            if (index < firstLocalElementGlobalIndex || index >= firstLocalElementGlobalIndex + elementsList.Count)
            {
                element = null;
                return false;
            }

            element = elementsList[GlobalToLocalIndex(index)];
            return true;
        }

        internal void ProcessUpdate()
        {
            if (elementsLoopIsActive == false)
            {
                return;
            }

            if (IsElementVisible(elementsList.Count / 2) == false)
            {
                // middle element is invisible, assume we scrolled list away too far, so reset all elements to the center of view area

                int middlePosition;

                if (elementStep.x > elementStep.y)
                {
                    middlePosition = Mathf.RoundToInt(elementsHolder.anchoredPosition.x / elementStep.x);
                }
                else
                {
                    middlePosition = Mathf.RoundToInt(elementsHolder.anchoredPosition.y / elementStep.y);
                }

                this.firstLocalElementGlobalIndex = Mathf.Clamp(middlePosition - elementsList.Count / 2, 0, totalElementsCount - elementsList.Count);

                for (int i = 0; i < elementsList.Count; i++)
                {
                    Reinit(i);
                }

                return;
            }

            if (IsElementVisible(0) && firstLocalElementGlobalIndex > 0)
            {
                // required +1 element at top

                int lastElementIndex = elementsList.Count - 1;

                if (elementsList.Count > 2 && IsElementVisible(elementsList.Count - 2) == false)
                {
                    // move last element to top

                    elementsList.Insert(0, elementsList[lastElementIndex]);
                    elementsList.RemoveAt(lastElementIndex + 1);
                    this.firstLocalElementGlobalIndex--;
                }
                else
                {
                    // create new element at top

                    AddNewFromStart();
                    this.firstLocalElementGlobalIndex--;

                    elementsLoopIsActive = totalElementsCount > elementsList.Count;
                }

                Reinit(0);
            }
            else if (IsElementVisible(elementsList.Count - 1) && firstLocalElementGlobalIndex + elementsList.Count < totalElementsCount)
            {
                // required +1 element at bottom

                int lastElementIndex = elementsList.Count - 1;

                if (elementsList.Count > 2 && IsElementVisible(1) == false)
                {
                    // move first element to bottom

                    elementsList.Add(elementsList[0]);
                    elementsList.RemoveAt(0);
                    this.firstLocalElementGlobalIndex++;
                }
                else
                {
                    // create new element at bottom

                    AddNew();
                    lastElementIndex++;

                    elementsLoopIsActive = totalElementsCount > elementsList.Count;
                }

                Reinit(lastElementIndex);
            }
        }

        private bool IsElementVisible(int localElementIndex) => (elementsList[localElementIndex].transform as RectTransform).Overlaps(visibleFrame);

        protected int GlobalToLocalIndex(int localIndex) => localIndex - firstLocalElementGlobalIndex;
        protected int LocalToGlobalIndex(int localIndex) => localIndex + firstLocalElementGlobalIndex;

        protected virtual void Reinit(int localElementIndex)
        {
            T element = elementsList[localElementIndex];
            element.SetVisible(true);

            int globalElementIndex = LocalToGlobalIndex(localElementIndex);

            if (initializerCache != null)
            {
                initializerCache(element, globalElementIndex);
            }

            RectTransform elementTransform = element.transform as RectTransform;
            elementTransform.anchorMin = Vector2.up;
            elementTransform.anchorMax = Vector2.up;
            elementTransform.pivot = Vector2.up;
            elementTransform.sizeDelta = elementSize;
            float xFromLeft = elementStep.x * (globalElementIndex);
            float yFromTop = elementStep.y * (globalElementIndex);
            elementTransform.anchoredPosition = new Vector2(xFromLeft, -yFromTop);
        }
    }
}
