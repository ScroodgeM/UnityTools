using System;

namespace UnityTools.UnityRuntime.Cache
{
    [Serializable]
    public struct DiskStorageSettings
    {
        public string customSubfolder;
        public bool useDedicatedThread;
        public int writeToDiskInterval;
    }
}
