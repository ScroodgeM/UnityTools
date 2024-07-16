//this empty line for UTF-8 BOM header
using System.IO;
using UnityEngine;

namespace UnityTools.UnityRuntime.Cache
{
    internal class DiskStorageJson<T> : DiskStorageBase<T>
    {
        public DiskStorageJson(string cacheId, IUnityInvocations unityInvocations) : base(cacheId, unityInvocations)
        {
        }

        protected override string GetFileExtension() => "json";

        protected override T ReadDataFromDisk(string filePath) => JsonUtility.FromJson<T>(File.ReadAllText(filePath));

        protected override void WriteDataToDisk(string filePath, T data) => File.WriteAllText(filePath, JsonUtility.ToJson(data));
    }
}
