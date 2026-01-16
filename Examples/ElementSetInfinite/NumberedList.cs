using UnityEngine;
using UnityTools.UnityRuntime.UI.ElementSet;

namespace UnityTools.Examples.ElementSetInfinite
{
    public class NumberedList : MonoBehaviour
    {
        [SerializeField] private UnityTools.UnityRuntime.UI.ElementSet.ElementSetInfinite elementSet;
        [SerializeField] private int elementsCount;

        private ElementSetInfinite<NumberedElement> elementSetInfinite;

        private void Awake()
        {
            elementSetInfinite = elementSet.TypedInfinite<NumberedElement>();
        }

        private void Start()
        {
            elementSetInfinite.Init(elementsCount, (element, index) => element.Init(index + 1));
        }
    }
}
