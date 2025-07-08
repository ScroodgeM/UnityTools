using System;

namespace UnityTools.Runtime
{
    public class Lazy<T>
    {
        private readonly Func<T> constructor;
        private readonly Action<T> initializer;

        private T instance;

        public Lazy(Func<T> constructor, Action<T> initializer)
        {
            this.constructor = constructor;
            this.initializer = initializer;
        }

        public T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = constructor();

                    if (initializer != null)
                    {
                        initializer(instance);
                    }
                }

                return instance;
            }
        }
    }
}