//this empty line for UTF-8 BOM header
using UnityEngine;
using UnityTools.Runtime.Promises;
using UnityTools.UnityRuntime.Timers;

namespace UnityTools.UnityRuntime.UI.Element.Animations
{
    [RequireComponent(typeof(ElementAnimator))]
    public abstract class AnimationBase : MonoBehaviour
    {
        private bool lastVisible;
        private IPromise lastAnimation;
        private float showAnimationDuration;
        private float hideAnimationDuration;

        public void Init(bool visible, float showAnimationDuration, float hideAnimationDuration)
        {
            this.showAnimationDuration = showAnimationDuration;
            this.hideAnimationDuration = hideAnimationDuration;

            lastVisible = visible;
            lastAnimation = Deferred.Resolved();
            Init();
            ApplyVisibility(visible ? 1f : 0f);
        }

        public IPromise SetVisible(bool visible)
        {
            if (lastVisible != visible)
            {
                lastVisible = visible;
                lastAnimation = lastAnimation.Then(() => { return StartAnimation(visible); });
            }

            return lastAnimation;
        }

        private IPromise StartAnimation(bool newVisilbeState)
        {
            float duration = newVisilbeState ? showAnimationDuration : hideAnimationDuration;

            return Timer.Instance.WaitUnscaled(duration,
                progress =>
                {
                    if (this != null)
                    {
                        if (newVisilbeState == true)
                        {
                            ApplyVisibility(progress);
                        }
                        else
                        {
                            ApplyVisibility(1f - progress);
                        }
                    }
                });
        }

        protected abstract void Init();

        internal abstract bool CanBeDisabledWhenInvisible { get; }

        protected abstract void ApplyVisibility(float visibility);
    }
}
