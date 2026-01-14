using TMPro;
using UnityEngine;
using UnityTools.UnityRuntime.UI.Element;

namespace UnityTools.Examples.ElementSetInfinite
{
    public class NumberedElement : ElementBase
    {
        [SerializeField] private TMP_Text label;

        internal void Init(int number)
        {
            label.text = number.ToString();
        }
    }
}
