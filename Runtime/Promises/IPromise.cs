//this empty line for UTF-8 BOM header
using System;

namespace UnityTools.Runtime.Promises
{
    public interface IPromise
    {
        IPromise Done(Action callback);
        IPromise Fail(Action<Exception> callback);
        IPromise Always(Action callback);
        IPromise Then(Func<IPromise> next);
    }

    public interface IPromise<T> : IPromise
    {
        IPromise<T> Done(Action<T> callback);
    }
}
