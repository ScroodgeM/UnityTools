using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Runtime.Promises;

namespace UnityTools.UnityRuntime.Timers
{
    public class Timer : MonoBehaviour, ITimer
    {
        private struct Awaiter
        {
            public double duration;
            public double finishTime;
            public bool timeIsUnscaled;
            public bool dieWithUnityObject;
            public UnityEngine.Object unityObjectToDieWith;
            public Func<bool> additionalCondition;
            public Action<float> progressCallback;
            public Deferred resolver;
        }

        private readonly ConcurrentQueue<Awaiter> newAwaiters = new ConcurrentQueue<Awaiter>();

        private readonly List<Awaiter> awaiters = new List<Awaiter>();

        private static ITimer instance;

        public static ITimer Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject timerGameObject = new GameObject("Timer");
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
                awaiters.Add(awaiter);
            }

            for (int i = 0; i < awaiters.Count; i++)
            {
                Awaiter candidate = awaiters[i];

                if (candidate.dieWithUnityObject == true && candidate.unityObjectToDieWith == null)
                {
                    awaiters.RemoveAt(i);
                    i--;

                    candidate.resolver.Reject(new OperationCanceledException());
                    continue;
                }

                if (GetTime(candidate.timeIsUnscaled) >= candidate.finishTime)
                {
                    if (candidate.additionalCondition == null || candidate.additionalCondition() == true)
                    {
                        awaiters.RemoveAt(i);
                        i--;

                        if (candidate.progressCallback != null)
                        {
                            candidate.progressCallback(1f);
                        }

                        candidate.resolver.Resolve();
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

        public IPromise WaitOneFrame()
        {
            return Wait(true, 0.001);
        }

        public IPromise Wait(double seconds, Action<float> progressCallback = null)
        {
            return Wait(false, seconds, progressCallback);
        }

        public IPromise WaitUnscaled(double seconds, Action<float> progressCallback = null)
        {
            return Wait(true, seconds, progressCallback);
        }

        public IPromise WaitForTrue(Func<bool> condition)
        {
            return Wait(true, 0.0, null, condition);
        }

        public IPromise WaitForMainThread()
        {
            return Wait(true, 0.0);
        }

        public IPromise UnityObjectWait(UnityEngine.Object unityObjectToDieWith, double seconds, Action<float> progressCallback = null)
        {
            return Wait(false, seconds, progressCallback, null, true, unityObjectToDieWith);
        }

        public IPromise UnityObjectWaitUnscaled(UnityEngine.Object unityObjectToDieWith, double seconds, Action<float> progressCallback = null)
        {
            return Wait(true, seconds, progressCallback, null, true, unityObjectToDieWith);
        }

        public IPromise UnityObjectWaitForTrue(UnityEngine.Object unityObjectToDieWith, Func<bool> condition)
        {
            return Wait(true, 0.0, null, condition, true, unityObjectToDieWith);
        }

        private IPromise Wait(bool timeIsUnscaled, double seconds, Action<float> progressCallback = null, Func<bool> additionalCondition = null, bool dieWithUnityObject = false, UnityEngine.Object unityObjectToDieWith = null)
        {
            Awaiter awaiter;

            awaiter.duration = seconds;
            awaiter.finishTime = GetTime(timeIsUnscaled) + seconds;
            awaiter.timeIsUnscaled = timeIsUnscaled;
            awaiter.additionalCondition = additionalCondition;
            awaiter.progressCallback = progressCallback;
            awaiter.resolver = Deferred.GetFromPool();
            awaiter.dieWithUnityObject = dieWithUnityObject;
            awaiter.unityObjectToDieWith = unityObjectToDieWith;

            newAwaiters.Enqueue(awaiter);

            return awaiter.resolver;
        }

        private static double GetTime(bool unscaled) => unscaled ? Time.realtimeSinceStartupAsDouble : Time.timeAsDouble;
    }
}
