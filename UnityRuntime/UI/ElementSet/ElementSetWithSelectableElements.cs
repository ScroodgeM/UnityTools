//this empty line for UTF-8 BOM header
using UnityTools.UnityRuntime.UI.Element;

namespace UnityTools.UnityRuntime.UI.ElementSet
{
    public class ElementSetWithSelectableElements<T> : ElementSet<T> where T : ElementBase, ISelectableElement
    {
        public T SelectedElement
        {
            get
            {
                foreach (T element in ActiveElements)
                {
                    if (element.IsSelected)
                    {
                        return element;
                    }
                }

                return null;
            }
        }

        public ElementSetWithSelectableElements(ElementSet wrapper, T elementPrefab) : base(wrapper, elementPrefab)
        {
        }

        public void SetSingleSelected(int selectedElementIndex)
        {
            SetSingleSelected(GetElement(selectedElementIndex));
        }

        public void SetSingleSelected(ISelectableElement selectedElement)
        {
            foreach (T element in ActiveElements)
            {
                element.SetSelected(element == selectedElement);
            }
        }
    }
}
