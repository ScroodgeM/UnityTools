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

#if !UNITY_EDITOR
        private bool initialized = false;
#endif

        private ElementAnimatorCustomDuration customDuration;
        private readonly List<AnimationBase> animations = new List<AnimationBase>();

        private bool visibilitySet = false;
        private readonly StatefulEventInt<bool> currentVisibility = StatefulEventInt.Create(false);
        private byte currentCommandId = 0;
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

#if !UNITY_EDITOR
            initialized = true;
#endif
        }

        private void Awake()
        {
            Initialize();

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
                currentCommandId++;
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
                byte myCommandId = currentCommandId;

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

                    if (myCommandId == currentCommandId)
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
