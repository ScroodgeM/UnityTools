
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
        private float animationDuration;

        public void Init(bool visible, float animationSpeed)
        {
            animationDuration = 1f / animationSpeed;

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
            return Timer.Instance.WaitUnscaled(animationDuration,
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
