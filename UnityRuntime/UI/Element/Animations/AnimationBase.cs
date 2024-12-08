//this empty line for UTF-8 BOM header

using UnityEngine;
using UnityTools.Runtime.Promises;
using UnityTools.UnityRuntime.Timers;

namespace UnityTools.UnityRuntime.UI.Element.Animations
{
    [RequireComponent(typeof(ElementAnimator))]
    public abstract class AnimationBase : MonoBehaviour
    {
        internal IPromise LastStateAnimation => lastStateAnimation;

        private bool initialized = false;
        private ElementAnimator elementAnimator;
        private bool lastStateIsVisible;
        private IPromise lastStateAnimation = Deferred.Resolved();

        private void Awake()
        {
            elementAnimator = GetComponent<ElementAnimator>();
            elementAnimator.CurrentVisibility.OnValueChanged += SetVisible;

            lastStateIsVisible = elementAnimator.CurrentVisibility.Value;
            ApplyVisibility(lastStateIsVisible ? 1f : 0f);
        }

        private void SetVisible(bool newStateIsVisible)
        {
#if !UNITY_EDITOR
            if (initialized == false)
#endif
            {
                Initialize();

                initialized = true;
            }

            if (lastStateIsVisible != newStateIsVisible)
            {
                lastStateIsVisible = newStateIsVisible;
                lastStateAnimation = lastStateAnimation.Then(() => StartAnimation(newStateIsVisible));
            }
        }

        private IPromise StartAnimation(bool newStateIsVisible)
        {
            if (elementAnimator.UnscaledTime == true)
            {
                return Timer.Instance.UnityObjectWaitUnscaled(this, GetDuration(), HandleProgress);
            }
            else
            {
                return Timer.Instance.UnityObjectWait(this, GetDuration(), HandleProgress);
            }

            float GetDuration()
            {
                return newStateIsVisible
                    ? elementAnimator.ShowAnimationDuration
                    : elementAnimator.HideAnimationDuration;
            }

            void HandleProgress(float progress)
            {
                if (newStateIsVisible == true)
                {
                    ApplyVisibility(progress);
                }
                else
                {
                    ApplyVisibility(1f - progress);
                }
            }
        }

        protected abstract void Initialize();

        public abstract bool CanBeDisabledWhenInvisible { get; }

        protected abstract void ApplyVisibility(float visibility);
    }
}
