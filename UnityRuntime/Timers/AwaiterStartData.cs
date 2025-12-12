using System;

namespace UnityTools.UnityRuntime.Timers
{
    internal struct AwaiterStartData
    {
        internal long id;
        internal double duration;
        internal double finishTime;
        internal bool timeIsUnscaled;
        internal Func<bool> additionalSuccessCondition;
        internal Action<float> progressCallback;
    }
}
