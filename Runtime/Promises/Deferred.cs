//this empty line for UTF-8 BOM header
using System;
using System.Collections.Generic;

namespace UnityTools.Runtime.Promises
{
    public class Deferred : BaseDeferred, IPromise
    {
        private static Queue<Deferred> poolQueue = new Queue<Deferred>();

        protected Deferred()
        {
        }

        ~Deferred()
        {
            lock (poolQueue)
            {
                if (poolQueue.Contains(this) == false)
                {
                    Reset();
                    poolQueue.Enqueue(this);
                    GC.ReRegisterForFinalize(this);
                }
            }
        }

        public static Deferred GetFromPool()
        {
            if (poolQueue.Count > 0)
            {
                lock (poolQueue)
                {
                    if (poolQueue.TryDequeue(out Deferred element) == true && element != null)
                    {
                        return element;
                    }
                }
            }

            return new Deferred();
        }

        public static IPromise Resolved() => GetFromPool().Resolve();

        public static IPromise Rejected(in Exception ex) => GetFromPool().Reject(ex);

        public IPromise Resolve()
        {
            if (CurrentState != States.Pending)
            {
                throw new InvalidOperationException(string.Format("CurrentState == {0}", CurrentState));
            }

            CurrentState = States.Resolved;

            for (int i = 0, maxi = DoneCallbacks.Count - 1; i <= maxi; i++)
            {
                DoneCallbacks[i]();
            }

            ClearCallbacks();

            return this;
        }

        public IPromise Reject(in Exception exception)
        {
            if (CurrentState != States.Pending)
            {
                throw new InvalidOperationException(string.Format("CurrentState == {0}", CurrentState));
            }

            CurrentState = States.Rejected;
            RejectReason = exception;

            for (int i = 0, maxi = FailCallbacks.Count - 1; i <= maxi; i++)
            {
                FailCallbacks[i](exception);
            }

            ClearCallbacks();

            return this;
        }

        public static IPromise All(params IPromise[] collection) => AllInternal(collection);

        public static IPromise All(List<IPromise> collection) => AllInternal(collection);

        private static IPromise AllInternal(ICollection<IPromise> collection)
        {
            Deferred deferred = GetFromPool();

            if (collection.Count == 0)
            {
                deferred.Resolve();
            }
            else
            {
                int promisesToComplete = collection.Count;

                foreach (IPromise element in collection)
                {
                    element.Done(() =>
                    {
                        promisesToComplete--;
                        if (deferred.CurrentState == States.Pending && promisesToComplete == 0)
                        {
                            deferred.Resolve();
                        }
                    });

                    element.Fail(ex =>
                    {
                        if (deferred.CurrentState == States.Pending)
                        {
                            deferred.Reject(ex);
                        }
                    });
                }
            }

            return deferred;
        }

        public static IPromise Race(params IPromise[] collection)
        {
            Deferred deferred = GetFromPool();

            if (collection.Length == 0)
            {
                deferred.Reject(new Exception("Deferred.Race called with empty array - no winner"));
            }
            else
            {
                int promisesToWait = collection.Length;

                for (int i = 0, maxi = collection.Length - 1; i <= maxi; i++)
                {
                    collection[i].Done(() =>
                    {
                        if (deferred.CurrentState == States.Pending)
                        {
                            deferred.Resolve();
                        }
                    });

                    collection[i].Fail(ex =>
                    {
                        promisesToWait--;
                        if (deferred.CurrentState == States.Pending && promisesToWait == 0)
                        {
                            deferred.Reject(ex);
                        }
                    });
                }
            }

            return deferred;
        }

        public static IPromise Sequence(params Func<IPromise>[] collection)
        {
            IPromise last = GetFromPool().Resolve();

            for (int i = 0, maxi = collection.Length - 1; i <= maxi; i++)
            {
                last = last.Then(collection[i]);
            }

            return last;
        }
    }

    public class Deferred<T> : BaseDeferred, IPromise<T>
    {
        private static Queue<Deferred<T>> poolQueue = new Queue<Deferred<T>>();

        protected T result;

        ~Deferred()
        {
            lock (poolQueue)
            {
                if (poolQueue.Contains(this) == false)
                {
                    Reset();
                    poolQueue.Enqueue(this);
                    GC.ReRegisterForFinalize(this);
                }
            }
        }

        protected override void Reset()
        {
            base.Reset();
            result = default;
        }

        public static Deferred<T> GetFromPool()
        {
            if (poolQueue.Count > 0)
            {
                lock (poolQueue)
                {
                    if (poolQueue.TryDequeue(out Deferred<T> element) == true && element != null)
                    {
                        return element;
                    }
                }
            }

            return new Deferred<T>();
        }

        public static IPromise<T> Resolved(in T result) => GetFromPool().Resolve(result);

        public static IPromise<T> Rejected(in Exception ex) => GetFromPool().Reject(ex);

        public IPromise<T> Resolve(in T result)
        {
            if (CurrentState != States.Pending)
            {
                throw new InvalidOperationException(string.Format("CurrentState == {0}", CurrentState));
            }

            CurrentState = States.Resolved;
            this.result = result;

            for (int i = 0, maxi = DoneCallbacks.Count - 1; i <= maxi; i++)
            {
                DoneCallbacks[i]();
            }

            ClearCallbacks();

            return this;
        }

        public IPromise<T> Reject(in Exception exception)
        {
            if (CurrentState != States.Pending)
            {
                throw new InvalidOperationException(string.Format("CurrentState == {0}", CurrentState));
            }

            CurrentState = States.Rejected;
            RejectReason = exception;

            for (int i = 0, maxi = FailCallbacks.Count - 1; i <= maxi; i++)
            {
                FailCallbacks[i](exception);
            }

            ClearCallbacks();

            return this;
        }

        public IPromise<T> Done(Action<T> callback)
        {
            switch (CurrentState)
            {
                case States.Resolved:
                    callback(result);
                    break;
                case States.Pending:
                    DoneCallbacks.Add(() => callback(result));
                    break;
            }

            return this;
        }
    }
}
