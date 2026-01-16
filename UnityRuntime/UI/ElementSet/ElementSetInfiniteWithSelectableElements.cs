using UnityEngine;
using UnityTools.UnityRuntime.UI.Element;

namespace UnityTools.UnityRuntime.UI.ElementSet
{
    public class ElementSetInfiniteWithSelectableElements<T> : ElementSetInfinite<T> where T : ElementBase, ISelectableElement
    {
        public int? SelectedElementIndex => selectedElementIndex;

        private int? selectedElementIndex = null;

        public ElementSetInfiniteWithSelectableElements(ElementSet wrapper, T elementPrefab, Vector2 elementSize, Vector2 elementStep) : base(wrapper, elementPrefab, elementSize, elementStep)
        {
        }

        public void SetSingleSelected(int? selectedElementIndex)
        {
            T element;

            if (this.selectedElementIndex.HasValue == true && base.TryGetElement(this.selectedElementIndex.Value, out element) == true)
            {
                element.SetSelected(false);
            }

            this.selectedElementIndex = selectedElementIndex;

            if (this.selectedElementIndex.HasValue == true && base.TryGetElement(this.selectedElementIndex.Value, out element) == true)
            {
                element.SetSelected(true);
            }
        }

        protected override void Reinit(int localElementIndex)
        {
            base.Reinit(localElementIndex);

            int globalElementIndex = LocalToGlobalIndex(localElementIndex);

            if (TryGetElement(globalElementIndex, out T element) == true)
            {
                element.SetSelected(selectedElementIndex.HasValue == true && selectedElementIndex.Value == globalElementIndex);
            }
        }
    }
}
