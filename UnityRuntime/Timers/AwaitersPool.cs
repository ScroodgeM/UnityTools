using System.Collections.Concurrent;
using System.Collections.Generic;

namespace UnityTools.UnityRuntime.Timers
{
    internal class AwaitersPool
    {
        private readonly ConcurrentQueue<Awaiter> inactiveAwaiters = new ConcurrentQueue<Awaiter>();
        private readonly List<Awaiter> activeAwaiters = new List<Awaiter>();

        internal int Length => activeAwaiters.Count;

        internal Awaiter this[int index] => activeAwaiters[index];

        internal void AddToRotation(Awaiter awaiter) => activeAwaiters.Add(awaiter);

        internal void DisableAt(int index)
        {
            Awaiter freedAwaiter = activeAwaiters[index];
            activeAwaiters.RemoveAt(index);

            freedAwaiter.ClearReferences();
            inactiveAwaiters.Enqueue(freedAwaiter);
        }

        internal Awaiter GetOrCreateAwaiter(AwaiterStartData startData)
        {
            if (inactiveAwaiters.TryDequeue(out Awaiter awaiter) == false)
            {
                awaiter = new Awaiter();
            }

            awaiter.PrepareNew(startData);
            return awaiter;
        }
    }
}
