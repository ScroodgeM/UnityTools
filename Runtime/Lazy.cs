//this empty line for UTF-8 BOM header
using System;

namespace UnityTools.Runtime
{
    public class Lazy<T>
    {
        private readonly Func<T> instanceConstructor;
        private readonly Action<T> instanceInitializer;

        private T instance;

        public T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = instanceConstructor();

                    if (instanceInitializer != null)
                    {
                        instanceInitializer(instance);
                    }
                }

                return instance;
            }
        }
    }
}
