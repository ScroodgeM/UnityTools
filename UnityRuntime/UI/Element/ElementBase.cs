//this empty line for UTF-8 BOM header
using UnityEngine;
using UnityTools.Runtime.Promises;
using UnityTools.UnityRuntime.UI.Element.Animations;

namespace UnityTools.UnityRuntime.UI.Element
{
    public class ElementBase : MonoBehaviour
    {
        private ElementAnimator elementAnimator;

        protected virtual void Awake()
        {
            elementAnimator = GetComponent<ElementAnimator>();
        }

        public virtual IPromise SetVisible(bool visible)
        {
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
