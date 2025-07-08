using UnityEngine;
using UnityTools.Runtime.Promises;

namespace UnityTools.UnityRuntime.Helpers
{
    public class WaitPromiseDone : CustomYieldInstruction
    {
        private bool completed = false;

        public override bool keepWaiting => this.completed == false;

        public WaitPromiseDone(IPromise promise)
        {
            promise.Done(() => this.completed = true);
        }
    }
}