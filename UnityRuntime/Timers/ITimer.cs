//this empty line for UTF-8 BOM header

using System;
using UnityTools.Runtime.Promises;

namespace UnityTools.UnityRuntime.Timers
{
    public interface ITimer
    {
        IPromise WaitOneFrame();
        IPromise Wait(double seconds, Action<float> progressCallback = null);
        IPromise WaitUnscaled(double seconds, Action<float> progressCallback = null);
        IPromise WaitForTrue(Func<bool> condition);
        IPromise WaitForMainThread();

        IPromise UnityObjectWait(UnityEngine.Object unityObjectToDieWith, double seconds, Action<float> progressCallback = null);
        IPromise UnityObjectWaitUnscaled(UnityEngine.Object unityObjectToDieWith, double seconds, Action<float> progressCallback = null);
        IPromise UnityObjectWaitForTrue(UnityEngine.Object unityObjectToDieWith, Func<bool> condition);
    }
}
