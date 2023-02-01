using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Runtime.Promises;

namespace UnityTools.UnityRuntime.UI.Element.Animations
{
    public class ElementAnimator : MonoBehaviour
    {
        private const float DEFAULT_ANIMATION_SPPED = 4f;

        [SerializeField] private bool visibleByDefault = false;

        private readonly List<AnimationBase> animations = new List<AnimationBase>();
        private readonly List<IPromise> cache = new List<IPromise>();

        internal bool lastStateIsVisible;

        private void Awake()
        {
            ElementAnimatorCustomSpeed customSpeed = GetComponent<ElementAnimatorCustomSpeed>();

            float animationSpeed = customSpeed == null ? DEFAULT_ANIMATION_SPPED : customSpeed.AnimationSpeed;

            GetComponents(animations);

            foreach (AnimationBase animation in animations)
            {
                animation.Init(visibleByDefault, animationSpeed);
            }

            lastStateIsVisible = visibleByDefault;

            TryDisableWhenInvisible();
        }

        public IPromise SetVisible(bool visible)
        {
            gameObject.SetActive(true);

#if UNITY_EDITOR
            animations.Clear();
            GetComponents(animations);
#endif

            cache.Clear();

            foreach (AnimationBase animation in animations)
            {
                cache.Add(animation.SetVisible(visible));
            }

            lastStateIsVisible = visible;

            return Deferred.All(cache).Done(() =>
            {
                TryDisableWhenInvisible();
            });
        }

        private void TryDisableWhenInvisible()
        {
            if (lastStateIsVisible == false)
            {
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

                    if (lastStateIsVisible == false)
                    {
                        DoAction();
                    }
                }

                void DoAction()
                {
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
