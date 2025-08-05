using System.IO;
using UnityEngine;

namespace UnityTools.UnityRuntime.Cache
{
    internal class DiskStorageTexture2D : DiskStorageBase<Texture2D>
    {
        public DiskStorageTexture2D(string cacheId, IUnityInvocations unityInvocations, DiskStorageSettings settings) : base(cacheId, unityInvocations, settings, "png")
        {
        }

        protected override Texture2D ReadDataFromDisk(string filePath)
        {
            Texture2D tex = new Texture2D(2, 2);

            tex.LoadImage(File.ReadAllBytes(filePath));

            return tex;
        }

        protected override void WriteDataToDisk(string filePath, Texture2D data) => File.WriteAllBytes(filePath, data.EncodeToPNG());
    }
}
