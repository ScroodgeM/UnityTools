using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using UnityTools.Runtime.Promises;

namespace UnityTools.UnityRuntime.Timers
{
    public class Timer : MonoBehaviour, ITimer
    {
        private static readonly Exception stopException = new OperationCanceledException();

        private readonly ConcurrentQueue<Awaiter> newAwaiters = new ConcurrentQueue<Awaiter>();

        private readonly AwaitersPool awaitersPool = new AwaitersPool();

        [ThreadStatic] private static uint nextTimerId;

        private static ITimer instance;

        public static ITimer Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject timerGameObject = new GameObject("Timer");
                    timerGameObject.hideFlags = HideFlags.HideAndDontSave;
                    DontDestroyOnLoad(timerGameObject);
                    instance = timerGameObject.AddComponent<Timer>();
                }

                return instance;
            }
        }

        private void Update()
        {
            while (newAwaiters.TryDequeue(out Awaiter awaiter) == true)
            {
                awaitersPool.AddToRotation(awaiter);
            }

            for (int i = 0; i < awaitersPool.Length; i++)
            {
                Awaiter candidate = awaitersPool[i];

                if (candidate.stopOnUnityObjectDestroyResult.HasValue == true
                    &&
                    candidate.stopOnUnityObjectDestroyReference == null
                   )
                {
                    Deferred resolver = candidate.resolver;
                    StopResult stopResult = candidate.stopOnUnityObjectDestroyResult.Value;

                    awaitersPool.DisableAt(i);
                    i--;

                    StopWithResult(resolver, stopResult);
                    continue;
                }

                if (candidate.stopOnUnityObjectDisableResult.HasValue == true
                    &&
                    (
                        candidate.stopOnUnityObjectDisableReference == null
                        ||
                        candidate.stopOnUnityObjectDisableReference.activeInHierarchy == false
                    )
                   )
                {
                    Deferred resolver = candidate.resolver;
                    StopResult stopResult = candidate.stopOnUnityObjectDisableResult.Value;

                    awaitersPool.DisableAt(i);
                    i--;

                    StopWithResult(resolver, stopResult);
                    continue;
                }

                if (GetTime(candidate.timeIsUnscaled) >= candidate.finishTime)
                {
                    if (candidate.additionalSuccessCondition == null || candidate.additionalSuccessCondition() == true)
                    {
                        Deferred resolver = candidate.resolver;
                        Action<float> progressCallback = candidate.progressCallback;

                        awaitersPool.DisableAt(i);
                        i--;

                        if (progressCallback != null)
                        {
                            progressCallback(1f);
                        }

                        resolver.Resolve();
                        continue;
                    }
                }
                else
                {
                    if (candidate.progressCallback != null)
                    {
                        double startTime = candidate.finishTime - candidate.duration;
                        float progress = Mathf.Clamp01((float)((GetTime(candidate.timeIsUnscaled) - startTime) / candidate.duration));
                        candidate.progressCallback(progress);
                    }
                }
            }
        }

        public ITimerPromise WaitOneFrame() => Wait(true, 0.001);

        public ITimerPromise Wait(double seconds, Action<float> progressCallback = null) => Wait(false, seconds, progressCallback);

        public ITimerPromise WaitUnscaled(double seconds, Action<float> progressCallback = null) => Wait(true, seconds, progressCallback);

        public ITimerPromise WaitForTrue(Func<bool> condition) => Wait(true, 0.0, null, condition);

        public ITimerPromise WaitForMainThread() => Wait(true, 0.0);

        public IPromise UnityObjectWait(UnityEngine.Object unityObjectToDieWith, double seconds, Action<float> progressCallback = null)
        {
            return Wait(false, seconds, progressCallback, null)
                .StopOnUnityObjectDestroy(unityObjectToDieWith, StopResult.Silently);
        }

        public IPromise UnityObjectWaitUnscaled(UnityEngine.Object unityObjectToDieWith, double seconds, Action<float> progressCallback = null)
        {
            return Wait(true, seconds, progressCallback, null)
                .StopOnUnityObjectDestroy(unityObjectToDieWith, StopResult.Silently);
        }

        public IPromise UnityObjectWaitForTrue(UnityEngine.Object unityObjectToDieWith, Func<bool> condition)
        {
            return Wait(true, 0.0, null, condition)
                .StopOnUnityObjectDestroy(unityObjectToDieWith, StopResult.Silently);
        }

        public void StopTimer(long timerId, StopResult stopResult)
        {
            for (int i = 0; i < awaitersPool.Length; i++)
            {
                Awaiter candidate = awaitersPool[i];

                if (candidate.id == timerId)
                {
                    awaitersPool.DisableAt(i);
                    StopWithResult(candidate.resolver, stopResult);
                    break;
                }
            }
        }

        private ITimerPromise Wait(bool timeIsUnscaled, double seconds, Action<float> progressCallback = null, Func<bool> additionalSuccessCondition = null)
        {
            AwaiterStartData startData;

            startData.id = nextTimerId | ((long)Thread.CurrentThread.ManagedThreadId << 32);
            startData.duration = seconds;
            startData.finishTime = GetTime(timeIsUnscaled) + seconds;
            startData.timeIsUnscaled = timeIsUnscaled;
            startData.additionalSuccessCondition = additionalSuccessCondition;
            startData.progressCallback = progressCallback;

            unchecked
            {
                nextTimerId++;
            }

            Awaiter awaiter = awaitersPool.GetOrCreateAwaiter(startData);
            newAwaiters.Enqueue(awaiter);
            return awaiter;
        }

        private static double GetTime(bool unscaled) => unscaled ? Time.realtimeSinceStartupAsDouble : Time.timeAsDouble;

        private static void StopWithResult(Deferred deferred, StopResult result)
        {
            switch (result)
            {
                case StopResult.WithRejection:
                    deferred.Reject(stopException);
                    break;

                case StopResult.WithResolving:
                    deferred.Resolve();
                    break;

                case StopResult.Silently:
                    break;
            }
        }
    }
}
