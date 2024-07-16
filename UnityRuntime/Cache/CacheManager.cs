//this empty line for UTF-8 BOM header

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.UnityRuntime.Cache
{
    public class CacheManager : MonoBehaviour, ICacheManager, IUnityInvocations
    {
        public event Action OnUpdateEvent = () => { };
        public event Action OnApplicationPauseEvent = () => { };
        public event Action OnDestroyEvent = () => { };

        private readonly Dictionary<string, ICacheStorage> cacheStoragesCache = new Dictionary<string, ICacheStorage>();

        private void Update() => OnUpdateEvent();

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                OnApplicationPauseEvent();
            }
        }

        private void OnDestroy() => OnDestroyEvent();

        public ICacheManager Init()
        {
            return this;
        }

        public ICacheStorage<T> GetCacheStorage<T>(bool rememberBetweenSessions)
        {
            string cacheId = typeof(T).FullName;

            if (cacheStoragesCache.TryGetValue(cacheId, out ICacheStorage existingCacheStorage) == false)
            {
                CacheStorage<T> cacheStorage = new CacheStorage<T>(cacheId, rememberBetweenSessions, this);
                cacheStoragesCache.Add(cacheId, cacheStorage);
                return cacheStorage;
            }
            else
            {
                if (existingCacheStorage is not CacheStorage<T> cacheStorage)
                {
                    Debug.LogError($"CacheStorage with id '{cacheId}' used with different data types, this is not supported");
                    return null;
                }

                if (cacheStorage.RememberBetweenSessions != rememberBetweenSessions)
                {
                    Debug.LogError($"CacheStorage of type '{typeof(T).Name}' exists but has another 'RememberBetweenSessions' setting, this is not supported");
                    return null;
                }

                return cacheStorage;
            }
        }
    }
}
