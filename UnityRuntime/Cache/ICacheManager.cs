namespace UnityTools.UnityRuntime.Cache
{
    public interface ICacheManager
    {
        ICacheManager Init();
        ICacheStorage<T> GetCacheStorage<T>(bool rememberBetweenSessions);
    }

    public interface ICacheStorage
    {
        bool HasValue { get; }
        void Clear();
    }

    public interface ICacheStorage<T> : ICacheStorage
    {
        T Value { get; set; }
    }
}