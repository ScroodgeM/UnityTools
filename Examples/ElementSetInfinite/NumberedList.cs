using UnityEngine;
using UnityTools.UnityRuntime.UI.ElementSet;

namespace UnityTools.Examples.ElementSetInfinite
{
    public class NumberedList : MonoBehaviour
    {
        [SerializeField] private ElementSet elementSet;
        [SerializeField] private Vector2 elementSize;
        [SerializeField] private Vector2 elementStep;
        [SerializeField] private int elementsCount;

        private ElementSetInfinite<NumberedElement> elementSetInfinite;

        private void Awake()
        {
            elementSetInfinite = elementSet.TypedInfinite<NumberedElement>(elementSize, elementStep);
        }

        private void Start()
        {
            elementSetInfinite.Init(elementsCount, (element, index) => element.Init(index + 1));
        }
    }
}
