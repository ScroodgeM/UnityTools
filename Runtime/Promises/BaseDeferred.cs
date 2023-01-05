
using System;
using System.Collections.Generic;

namespace UnityTools.Runtime.Promises
{
    public abstract class BaseDeferred : IPromise
    {
        protected enum States
        {
            Pending,
            Resolved,
            Rejected,
        }

        protected States CurrentState;

        protected List<Action> DoneCallbacks = new List<Action>(10);
        protected List<Action<Exception>> FailCallbacks = new List<Action<Exception>>(10);

        protected Exception RejectReason;

        public IPromise Done(Action callback)
        {
            switch (CurrentState)
            {
                case States.Resolved:
                    callback();
                    break;
                case States.Pending:
                    DoneCallbacks.Add(callback);
                    break;
            }

            return this;
        }

        public IPromise Fail(Action<Exception> callback)
        {
            switch (CurrentState)
            {
                case States.Rejected:
                    callback(RejectReason);
                    break;
                case States.Pending:
                    FailCallbacks.Add(callback);
                    break;
            }

            return this;
        }

        public IPromise Always(Action callback)
        {
            switch (CurrentState)
            {
                case States.Resolved:
                case States.Rejected:
                    callback();
                    break;
                case States.Pending:
                    DoneCallbacks.Add(callback);
                    FailCallbacks.Add(ex => callback());
                    break;
            }

            return this;
        }

        public IPromise Then(Func<IPromise> next)
        {
            Deferred deferred = Deferred.GetFromPool();

            Done(() =>
            {
                next()
                    .Done(() => deferred.Resolve())
                    .Fail(ex => deferred.Reject(ex));

            });

            Fail(ex => deferred.Reject(ex));

            return deferred;
        }

        protected virtual void ClearCallbacks()
        {
            DoneCallbacks.Clear();
            FailCallbacks.Clear();
        }

        protected virtual void Reset()
        {
            ClearCallbacks();
            CurrentState = States.Pending;
            RejectReason = null;
        }
    }
}
