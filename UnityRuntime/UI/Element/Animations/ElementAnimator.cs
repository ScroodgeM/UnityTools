﻿//this empty line for UTF-8 BOM header

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Runtime.Promises;

namespace UnityTools.UnityRuntime.UI.Element.Animations
{
    public class ElementAnimator : MonoBehaviour
    {
        private const float DEFAULT_ANIMATION_DURATION = 0.3f;

        [SerializeField] private bool visibleByDefault = false;

        private readonly List<AnimationBase> animations = new List<AnimationBase>();
        private readonly List<IPromise> cache = new List<IPromise>();

        private byte lastStateCounter;
        private bool lastStateIsVisible;

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

            TryDisableWhenInvisible();
        }

        internal IPromise SetVisible(bool visible)
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

            unchecked
            {
                lastStateCounter++;
            }

            lastStateIsVisible = visible;

            return Deferred.All(cache).Done(TryDisableWhenInvisible);
        }

        private void TryDisableWhenInvisible()
        {
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
