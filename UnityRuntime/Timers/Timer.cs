
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
            public bool unscaledTime;
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

                double currentTime = candidate.unscaledTime ? Time.realtimeSinceStartupAsDouble : Time.timeAsDouble;

                if (currentTime >= candidate.finishTime)
                {
                    if (candidate.additionalCondition == null || candidate.additionalCondition() == true)
                    {
                        awaiters.RemoveAt(i);
                        if (candidate.progressCallback != null)
                        {
                            candidate.progressCallback(1f);
                        }
                        candidate.resolver.Resolve();
                        i--;
                    }
                }
                else
                {
                    if (candidate.progressCallback != null)
                    {
                        double startTime = candidate.finishTime - candidate.duration;
                        float progress = Mathf.Clamp01((float)((currentTime - startTime) / candidate.duration));
                        candidate.progressCallback(progress);
                    }
                }
            }
        }

        public IPromise WaitOneFrame() => Wait(true, 0.001);

        public IPromise Wait(double seconds, Action<float> progressCallback = null) => Wait(false, seconds, progressCallback);

        public IPromise WaitUnscaled(double seconds, Action<float> progressCallback = null) => Wait(true, seconds, progressCallback);

        public IPromise WaitForTrue(Func<bool> condition) => Wait(true, 0.0, null, condition);

        public IPromise WaitForMainThread() => Wait(true, 0.0);

        private IPromise Wait(bool unscaledTime, double seconds, Action<float> progressCallback = null, Func<bool> additionalCondition = null)
        {
            Deferred deferred = Deferred.GetFromPool();

            newAwaiters.Enqueue(new Awaiter
            {
                duration = seconds,
                finishTime = (unscaledTime ? Time.realtimeSinceStartupAsDouble : Time.timeAsDouble) + seconds,
                unscaledTime = unscaledTime,
                additionalCondition = additionalCondition,
                progressCallback = progressCallback,
                resolver = deferred,
            });

            return deferred;
        }
    }
}
