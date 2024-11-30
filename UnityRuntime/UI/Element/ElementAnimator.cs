//this empty line for UTF-8 BOM header

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityTools.Runtime.Promises;
using UnityTools.UnityRuntime.UI.Element.Animations;

namespace UnityTools.UnityRuntime.UI.Element
{
    public class ElementAnimator : MonoBehaviour
    {
        internal const float DEFAULT_ANIMATION_DURATION = 0.3f;

        [SerializeField] private bool visibleByDefault = false;
        [SerializeField] protected bool unscaledTime = false;

        private readonly List<AnimationBase> animations = new List<AnimationBase>();

        private byte lastStateCounter;
        private bool lastStateIsVisible;
        private IPromise lastStatePromise;

        private void Awake()
        {
            ElementAnimatorCustomDuration customDuration = GetComponent<ElementAnimatorCustomDuration>();
            float showAnimationDuration = customDuration == null ? DEFAULT_ANIMATION_DURATION : customDuration.ShowAnimationDuration;
            float hideAnimationDuration = customDuration == null ? DEFAULT_ANIMATION_DURATION : customDuration.HideAnimationDuration;

            GetComponents(animations);

            foreach (AnimationBase animation in animations)
            {
                animation.Init(visibleByDefault, showAnimationDuration, hideAnimationDuration);
            }

            lastStateCounter = 0;
            lastStateIsVisible = visibleByDefault;
            lastStatePromise = Deferred.Resolved();

            TryDisableWhenInvisible();
        }

        internal virtual IPromise SetVisible(bool visible)
        {
            using PooledObject<List<IPromise>> _ = ListPool<IPromise>.Get(out List<IPromise> promises);

            if (lastStateIsVisible == visible)
            {
                return lastStatePromise;
            }

            gameObject.SetActive(true);

#if UNITY_EDITOR
            animations.Clear();
            GetComponents(animations);
#endif

            foreach (AnimationBase animation in animations)
            {
                promises.Add(animation.SetVisible(visible, unscaledTime));
            }

            unchecked
            {
                lastStateCounter++;
            }

            lastStateIsVisible = visible;
            lastStatePromise = Deferred.All(promises).Done(TryDisableWhenInvisible);

            return lastStatePromise;
        }

        private void TryDisableWhenInvisible()
        {
            if (this == null)
            {
                return;
            }

            if (lastStateIsVisible == false)
            {
                byte myStateCounter = lastStateCounter;

                if (gameObject.activeInHierarchy == true)
                {
                    StartCoroutine(WaitAndDoAction());
                }
                else
                {
                    DoAction();
                }

                IEnumerator WaitAndDoAction()
                {
                    yield return null;

                    if (myStateCounter == lastStateCounter)
                    {
                        DoAction();
                    }
                }

                void DoAction()
                {
                    if (animations.Count == 0)
                    {
                        gameObject.SetActive(false);
                        return;
                    }

                    foreach (AnimationBase animation in animations)
                    {
                        if (animation.CanBeDisabledWhenInvisible)
                        {
                            gameObject.SetActive(false);
                            break;
                        }
                    }
                }
            }
        }
    }
}
