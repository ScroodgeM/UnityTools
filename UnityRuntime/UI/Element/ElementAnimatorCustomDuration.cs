//this empty line for UTF-8 BOM header

using UnityEngine;

namespace UnityTools.UnityRuntime.UI.Element
{
    public class ElementAnimatorCustomDuration : MonoBehaviour
    {
        internal float ShowAnimationDuration => showAnimationDuration;
        internal float HideAnimationDuration => hideAnimationDuration;

        [SerializeField] private float showAnimationDuration;
        [SerializeField] private float hideAnimationDuration;
    }
}
