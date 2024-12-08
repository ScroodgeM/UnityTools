//this empty line for UTF-8 BOM header

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityTools.Runtime.Promises;
using UnityTools.Runtime.StatefulEvent;
using UnityTools.UnityRuntime.UI.Element.Animations;

namespace UnityTools.UnityRuntime.UI.Element
{
    public class ElementAnimator : MonoBehaviour
    {
        internal const float DEFAULT_ANIMATION_DURATION = 0.3f;
        internal bool UnscaledTime => unscaledTime;

        internal IStatefulEvent<bool> CurrentVisibility => currentVisibility;

        internal float ShowAnimationDuration
        {
            get
            {
                Initialize();
                return customDuration == null ? DEFAULT_ANIMATION_DURATION : customDuration.ShowAnimationDuration;
            }
        }

        internal float HideAnimationDuration
        {
            get
            {
                Initialize();
                return customDuration == null ? DEFAULT_ANIMATION_DURATION : customDuration.HideAnimationDuration;
            }
        }

        [SerializeField] private bool visibleByDefault = false;
        [SerializeField] protected bool unscaledTime = false;

        private bool initialized = false;

        private ElementAnimatorCustomDuration customDuration;
        private readonly List<AnimationBase> animations = new List<AnimationBase>();

        private bool visibilitySet = false;
        private readonly StatefulEventInt<bool> currentVisibility = StatefulEventInt.Create(false);
        private byte lastStateCounter;
        private IPromise lastStatePromise;

        private void Initialize()
        {
#if !UNITY_EDITOR
            if (initialized == true)
            {
                return;
            }
#endif

            if (visibilitySet == false)
            {
                currentVisibility.Set(visibleByDefault);
                visibilitySet = true;
            }

            customDuration = GetComponent<ElementAnimatorCustomDuration>();

            animations.Clear();
            GetComponents(animations);

            initialized = true;
        }

        private void Awake()
        {
            Initialize();

            lastStateCounter = 0;
            lastStatePromise = Deferred.Resolved();

            TryDisableWhenInvisible();
        }

        internal virtual IPromise SetVisible(bool visible)
        {
            if (currentVisibility.Value == visible)
            {
                return lastStatePromise;
            }

            currentVisibility.Set(visible);
            visibilitySet = true;

            Initialize();

            gameObject.SetActive(true);

            using PooledObject<List<IPromise>> _ = ListPool<IPromise>.Get(out List<IPromise> promises);

            foreach (AnimationBase animation in animations)
            {
                promises.Add(animation.LastStateAnimation);
            }

            unchecked
            {
                lastStateCounter++;
            }

            lastStatePromise = Deferred.All(promises).Done(TryDisableWhenInvisible);

            return lastStatePromise;
        }

        private void TryDisableWhenInvisible()
        {
            if (this == null)
            {
                return;
            }

            if (currentVisibility.Value == false)
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
