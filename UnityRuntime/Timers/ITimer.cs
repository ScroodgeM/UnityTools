using System;
using UnityTools.Runtime.Promises;

namespace UnityTools.UnityRuntime.Timers
{
    public interface ITimer
    {
        ITimerPromise WaitOneFrame();
        ITimerPromise Wait(double seconds, Action<float> progressCallback = null);
        ITimerPromise WaitUnscaled(double seconds, Action<float> progressCallback = null);
        ITimerPromise WaitForTrue(Func<bool> condition);
        ITimerPromise WaitForMainThread();

        [Obsolete("Use Wait instead and add UnityObject with StopOnUnityObjectDestroy() call")]
        IPromise UnityObjectWait(UnityEngine.Object unityObjectToDieWith, double seconds, Action<float> progressCallback = null);

        [Obsolete("Use WaitUnscaled instead and add UnityObject with StopOnUnityObjectDestroy() call")]
        IPromise UnityObjectWaitUnscaled(UnityEngine.Object unityObjectToDieWith, double seconds, Action<float> progressCallback = null);

        [Obsolete("Use WaitForTrue instead and add UnityObject with StopOnUnityObjectDestroy() call")]
        IPromise UnityObjectWaitForTrue(UnityEngine.Object unityObjectToDieWith, Func<bool> condition);

        void StopTimer(long timerId, StopResult stopResult);
    }
}
