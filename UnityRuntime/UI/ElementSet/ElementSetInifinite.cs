using System;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.UnityRuntime.Helpers;
using UnityTools.UnityRuntime.UI.Element;

namespace UnityTools.UnityRuntime.UI.ElementSet
{
    public class ElementSetInfinite<T> : ElementSetBase<T> where T : ElementBase
    {
        private readonly Vector2 elementSize;
        private readonly Vector2 elementStep;

        private Action<T, int> initializerCache;
        private int firstElementIndex = 0;
        private int totalElementsCount = 0;

        public ElementSetInfinite(ElementSet elementSet, T elementPrefab, Vector2 elementSize, Vector2 elementStep)
            : base(elementSet, elementPrefab)
        {
            this.elementSize = elementSize;
            this.elementStep = elementStep;
        }

        public void AddAllChildElementsToTheList()
        {
            elementsList.Clear();
            elementsHolder.GetComponentsInChildren(true, elementsList);
        }

        public override void Init(int count, Action<T, int> initializer = null)
        {
            this.initializerCache = initializer;
            this.firstElementIndex = 0;
            this.totalElementsCount = count;

            if (elementsList.Count == 0 && count > 0)
            {
                AddNew();
                Reinit(0);
            }

            elementsHolder.anchorMin = Vector2.up;
            elementsHolder.anchorMax = Vector2.up;
            elementsHolder.pivot = Vector2.up;
            elementsHolder.sizeDelta = elementSize + elementStep * (count - 1);
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
            bool middleElementIsVisible =
                (elementsList[elementsList.Count / 2].transform as RectTransform).Overlaps(visibleFrame) == true;

            if (middleElementIsVisible == false)
            {
                int middlePosition;

                if (elementStep.x > elementStep.y)
                {
                    middlePosition = Mathf.RoundToInt(elementsHolder.anchoredPosition.x / elementStep.x);
                }
                else
                {
                    middlePosition = Mathf.RoundToInt(elementsHolder.anchoredPosition.y / elementStep.y);
                }

                this.firstElementIndex = Mathf.Clamp(middlePosition - elementsList.Count / 2, 0, totalElementsCount - elementsList.Count);

                for (int i = 0; i < elementsList.Count; i++)
                {
                    Reinit(i);
                }

                return;
            }

            bool hasInvisibleElementOnLeftSide =
                (elementsList[0].transform as RectTransform).Overlaps(visibleFrame) == false;

            bool canTakeElementOnLeftSide =
                elementsList.Count > 2
                &&
                (elementsList[1].transform as RectTransform).Overlaps(visibleFrame) == false;

            bool requiredElementOnLeftSide =
                hasInvisibleElementOnLeftSide == false
                &&
                firstElementIndex > 0;

            bool hasInvisibleElementOnRightSide =
                (elementsList[^1].transform as RectTransform).Overlaps(visibleFrame) == false;

            bool canTakeElementOnRightSide =
                elementsList.Count > 2
                &&
                (elementsList[^2].transform as RectTransform).Overlaps(visibleFrame) == false;

            bool requiredElementOnRightSide =
                hasInvisibleElementOnRightSide == false
                &&
                firstElementIndex + elementsList.Count < totalElementsCount;

            if (requiredElementOnLeftSide == true)
            {
                int lastElementIndex = elementsList.Count - 1;

                if (canTakeElementOnRightSide == true)
                {
                    elementsList.Insert(0, elementsList[lastElementIndex]);
                    elementsList.RemoveAt(lastElementIndex + 1);
                    this.firstElementIndex--;
                }
                else
                {
                    AddNewFromStart();
                    this.firstElementIndex--;
                }

                Reinit(0);
            }
            else if (requiredElementOnRightSide == true)
            {
                int lastElementIndex = elementsList.Count - 1;

                if (canTakeElementOnLeftSide == true)
                {
                    elementsList.Add(elementsList[0]);
                    elementsList.RemoveAt(0);
                    this.firstElementIndex++;
                }
                else
                {
                    AddNew();
                    lastElementIndex++;
                }

                Reinit(lastElementIndex);
            }
        }

        private void Reinit(int elementIndex)
        {
            T element = elementsList[elementIndex];

            int globalElementIndex = elementIndex + firstElementIndex;

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
