using UnityTools.Runtime.Promises;

namespace UnityTools.UnityRuntime.Timers
{
    public interface ITimerPromise : IPromise
    {
        ITimerPromise StopNow(StopResult stopResult);
        ITimerPromise StopOnUnityGameObjectDisable(UnityEngine.GameObject unityObject, StopResult stopResult);
        ITimerPromise StopOnUnityObjectDestroy(UnityEngine.Object unityObject, StopResult stopResult);
        ITimerPromise GetTimerId(out long timerId);
    }
}
