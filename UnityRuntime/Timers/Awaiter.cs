using System;
using UnityTools.Runtime.Promises;

namespace UnityTools.UnityRuntime.Timers
{
    internal class Awaiter : ITimerPromise
    {
        internal long id => startData.id;
        internal double duration => startData.duration;
        internal double finishTime => startData.finishTime;
        internal bool timeIsUnscaled => startData.timeIsUnscaled;
        internal Func<bool> additionalSuccessCondition => startData.additionalSuccessCondition;
        internal Action<float> progressCallback => startData.progressCallback;

        internal StopResult? stopOnUnityObjectDisableResult;
        internal UnityEngine.GameObject stopOnUnityObjectDisableReference;

        internal StopResult? stopOnUnityObjectDestroyResult;
        internal UnityEngine.Object stopOnUnityObjectDestroyReference;

        internal Deferred resolver { get; private set; }

        private AwaiterStartData startData;

        internal void PrepareNew(AwaiterStartData startData)
        {
            this.startData = startData;
            this.stopOnUnityObjectDisableResult = null;
            this.stopOnUnityObjectDestroyResult = null;
            this.resolver = Deferred.GetFromPool();
        }

        internal void ClearReferences()
        {
            startData = default;
            stopOnUnityObjectDisableReference = null;
            stopOnUnityObjectDestroyReference = null;
            resolver = null;
        }

        public IPromise Done(Action callback) => resolver.Done(callback);

        public IPromise Fail(Action<Exception> callback) => resolver.Fail(callback);

        public IPromise Always(Action callback) => resolver.Always(callback);

        public IPromise Then(Func<IPromise> next) => resolver.Then(next);

        public IPromise<TNext> Then<TNext>(Func<IPromise<TNext>> next) => resolver.Then(next);

        public ITimerPromise StopNow(StopResult stopResult)
        {
            startData.finishTime = double.MinValue;
            return this;
        }

        public ITimerPromise StopOnUnityGameObjectDisable(UnityEngine.GameObject unityObject, StopResult stopResult)
        {
            if (stopOnUnityObjectDestroyResult.HasValue || stopOnUnityObjectDisableResult.HasValue)
            {
                throw new InvalidOperationException($"unity object to stop on disable/destroy already registered");
            }

            stopOnUnityObjectDisableReference = unityObject;
            stopOnUnityObjectDisableResult = stopResult;
            return this;
        }

        public ITimerPromise StopOnUnityObjectDestroy(UnityEngine.Object unityObject, StopResult stopResult)
        {
            if (stopOnUnityObjectDestroyResult.HasValue || stopOnUnityObjectDisableResult.HasValue)
            {
                throw new InvalidOperationException($"unity object to stop on disable/destroy already registered");
            }

            stopOnUnityObjectDestroyReference = unityObject;
            stopOnUnityObjectDestroyResult = stopResult;
            return this;
        }

        public ITimerPromise GetTimerId(out long timerId)
        {
            timerId = id;
            return this;
        }
    }
}