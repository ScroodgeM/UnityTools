using UnityEngine;
using UnityTools.Runtime.Promises;

namespace UnityTools.UnityRuntime.UI.Element
{
    public class ElementBase : MonoBehaviour
    {
        private bool initialized = false;

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
            initialized = true;
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