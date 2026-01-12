using UnityEngine;
using UnityTools.Runtime.Promises;

namespace UnityTools.UnityRuntime.UI.Element
{
    public class ElementBase : MonoBehaviour
    {
#if !UNITY_EDITOR
        private bool initialized = false;
#endif

        private ElementAnimator elementAnimator;

        private void Initialize()
        {
#if !UNITY_EDITOR
            if (initialized == true)
            {
                return;
            }
#endif

            elementAnimator = GetComponent<ElementAnimator>();
#if !UNITY_EDITOR
            initialized = true;
#endif
        }

        public virtual IPromise SetVisible(bool visible)
        {
            Initialize();

            if (elementAnimator != null)
            {
                return elementAnimator.SetVisible(visible);
            }
            else
            {
                gameObject.SetActive(visible);
                return Deferred.Resolved();
            }
        }
    }
}
