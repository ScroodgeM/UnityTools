using UnityEngine;

namespace UnityTools.UnityRuntime.Cache
{
    internal class CacheStorage<T> : ICacheStorage<T>
    {
        private readonly string cacheId;
        private readonly DiskStorageBase<T> diskStorage;

        internal bool RememberBetweenSessions => diskStorage != null;

        public bool HasValue => hasCachedValue;

        public T Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        private bool hasCachedValue;
        private T cachedValue;

        internal CacheStorage(string cacheId, bool rememberBetweenSessions, IUnityInvocations unityInvocations, DiskStorageSettings settings)
        {
            this.cacheId = cacheId;

            if (rememberBetweenSessions == true)
            {
                if (typeof(T) == typeof(Texture2D))
                {
                    diskStorage = new DiskStorageTexture2D(cacheId, unityInvocations, settings) as DiskStorageBase<T>;
                }
                else
                {
                    diskStorage = new DiskStorageJson<T>(cacheId, unityInvocations, settings);
                }

                hasCachedValue = diskStorage.TryLoad(out cachedValue);
            }
            else
            {
                hasCachedValue = false;
                cachedValue = default;
            }
        }

        private T GetValue()
        {
            if (hasCachedValue == false)
            {
                Debug.LogWarning($"CacheStorage id='{cacheId}' requested to get value which doesn't exist");
                return default;
            }

            return cachedValue;
        }

        private void SetValue(T value)
        {
            hasCachedValue = true;
            cachedValue = value;

            if (diskStorage != null)
            {
                diskStorage.Save(value);
            }
        }

        public void Clear()
        {
            hasCachedValue = false;
            cachedValue = default;

            if (diskStorage != null)
            {
                diskStorage.Clear();
            }
        }
    }
}