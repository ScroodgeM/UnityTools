using System.IO;
using UnityEngine;

namespace UnityTools.UnityRuntime.Cache
{
    internal class DiskStorageJson<T> : DiskStorageBase<T>
    {
        public DiskStorageJson(string cacheId, IUnityInvocations unityInvocations, DiskStorageSettings settings) : base(cacheId, unityInvocations, settings, "json")
        {
        }

        protected override T ReadDataFromDisk(string filePath) => JsonUtility.FromJson<T>(File.ReadAllText(filePath));

        protected override void WriteDataToDisk(string filePath, T data)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            const bool prettyPrint = true;
#else
            const bool prettyPrint = false;
#endif
            File.WriteAllText(filePath, JsonUtility.ToJson(data, prettyPrint));
        }
    }
}