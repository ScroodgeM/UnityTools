//this empty line for UTF-8 BOM header

using System;
using UnityEngine;
using UnityTools.Runtime.Promises;

namespace UnityTools.UnityRuntime.UI.Element.Animations
{
    [RequireComponent(typeof(ElementAnimator))]
    public abstract class AnimationBase : MonoBehaviour
    {
        private struct AnimationTask
        {
            public bool goalVisibilityState;
            public float changeSpeed;
            public Deferred completePromise;
        }

        internal IPromise LastStateAnimation => currentAnimationTask.HasValue ? currentAnimationTask.Value.completePromise : Deferred.Resolved();

        private bool initialized = false;
        private ElementAnimator elementAnimator;
        private float currentVisibilityState;
        private AnimationTask? currentAnimationTask;

        private void Awake()
        {
            InitializeInternal();

            elementAnimator = GetComponent<ElementAnimator>();
            elementAnimator.CurrentVisibility.OnValueChanged += SetVisible;
            ApplyVisibility(elementAnimator.CurrentVisibility.Value ? 1f : 0f);
        }

        private void Update()
        {
            if (currentAnimationTask.HasValue == true)
            {
                InitializeInternal();

                AnimationTask task = currentAnimationTask.Value;

                float deltaTime = elementAnimator.UnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float goalVisibilityState = task.goalVisibilityState ? 1f : 0f;

                currentVisibilityState = Mathf.MoveTowards(currentVisibilityState, goalVisibilityState, task.changeSpeed * deltaTime);

                ApplyVisibility(currentVisibilityState);

                if (Mathf.Approximately(currentVisibilityState, goalVisibilityState) == true)
                {
                    currentAnimationTask = null;
                    task.completePromise.Resolve();
                }
            }
        }

        private void SetVisible(bool newGoalVisibilityState)
        {
            if (currentAnimationTask.HasValue == true)
            {
                AnimationTask task = currentAnimationTask.Value;
                if (task.goalVisibilityState != newGoalVisibilityState)
                {
                    currentAnimationTask = null;
                    task.completePromise.Reject(new OperationCanceledException());
                }
            }

            if (currentAnimationTask.HasValue == false)
            {
                AnimationTask task;
                task.goalVisibilityState = newGoalVisibilityState;
                task.changeSpeed = 1f / (newGoalVisibilityState ? elementAnimator.ShowAnimationDuration : elementAnimator.HideAnimationDuration);
                task.completePromise = Deferred.GetFromPool();
                currentAnimationTask = task;
            }
        }

        private void InitializeInternal()
        {
#if !UNITY_EDITOR
            if (initialized == false)
#endif
            {
                Initialize();

                initialized = true;
            }
        }

        protected abstract void Initialize();

        public abstract bool CanBeDisabledWhenInvisible { get; }

        protected abstract void ApplyVisibility(float visibility);
    }
}
