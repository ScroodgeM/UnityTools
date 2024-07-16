//this empty line for UTF-8 BOM header

using System.IO;
using UnityEngine;

namespace UnityTools.UnityRuntime.Cache
{
    internal class DiskStorageTexture2D : DiskStorageBase<Texture2D>
    {
        public DiskStorageTexture2D(string cacheId, IUnityInvocations unityInvocations) : base(cacheId, unityInvocations)
        {
        }

        protected override string GetFileExtension() => "png";

        protected override Texture2D ReadDataFromDisk(string filePath)
        {
            Texture2D tex = new Texture2D(2, 2);

            tex.LoadImage(File.ReadAllBytes(filePath));

            return tex;
        }

        protected override void WriteDataToDisk(string filePath, Texture2D data) => File.WriteAllBytes(filePath, data.EncodeToPNG());
    }
}
