using System;

namespace UnityTools.UnityRuntime.Cache
{
    [Serializable]
    public struct DiskStorageSettings
    {
        public bool useDedicatedThread;
        public int writeToDiskInterval;
    }
}
