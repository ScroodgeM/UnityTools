using System;

namespace UnityTools.Runtime.Promises
{
    public interface IPromise
    {
        IPromise Done(Action callback);
        IPromise Fail(Action<Exception> callback);
        IPromise Always(Action callback);
        IPromise Then(Func<IPromise> next);
        IPromise<TNext> Then<TNext>(Func<IPromise<TNext>> next);
    }

    public interface IPromise<T> : IPromise
    {
        IPromise<T> Done(Action<T> callback);
        IPromise<TNext> Then<TNext>(Func<T, IPromise<TNext>> next);
    }
}