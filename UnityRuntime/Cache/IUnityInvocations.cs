//this empty line for UTF-8 BOM header

using System;

namespace UnityTools.UnityRuntime.Cache
{
    internal interface IUnityInvocations
    {
        event Action OnUpdateEvent;
        event Action OnApplicationPauseEvent;
        event Action OnDestroyEvent;
    }
}
