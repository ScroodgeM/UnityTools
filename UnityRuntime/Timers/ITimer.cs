
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
    }
}
