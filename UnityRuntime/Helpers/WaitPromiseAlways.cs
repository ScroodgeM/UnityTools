//this empty line for UTF-8 BOM header
using UnityEngine;
using UnityTools.Runtime.Promises;

namespace UnityTools.UnityRuntime.Helpers
{
    public class WaitPromiseAlways : CustomYieldInstruction
    {
        private bool completed = false;

        public override bool keepWaiting => this.completed;

        public WaitPromiseAlways(IPromise promise)
        {
            promise.Always(() => this.completed = true);
        }
    }
}
