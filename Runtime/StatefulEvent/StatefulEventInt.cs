//this empty line for UTF-8 BOM header
using System;

namespace UnityTools.Runtime.StatefulEvent
{
    public static class StatefulEventInt
    {
        public static StatefulEventInt<bool> Create(bool defaultValue)
        {
            return new StatefulEventInt<bool>(defaultValue, (a, b) => a == b);
        }
        public static StatefulEventInt<string> Create(string defaultValue)
        {
            return new StatefulEventInt<string>(defaultValue, (a, b) => a == b);
        }
        public static StatefulEventInt<sbyte> Create(sbyte defaultValue)
        {
            return new StatefulEventInt<sbyte>(defaultValue, (a, b) => a == b);
        }
        public static StatefulEventInt<byte> Create(byte defaultValue)
        {
            return new StatefulEventInt<byte>(defaultValue, (a, b) => a == b);
        }
        public static StatefulEventInt<short> Create(short defaultValue)
        {
            return new StatefulEventInt<short>(defaultValue, (a, b) => a == b);
        }
        public static StatefulEventInt<ushort> Create(ushort defaultValue)
        {
            return new StatefulEventInt<ushort>(defaultValue, (a, b) => a == b);
        }
        public static StatefulEventInt<int> Create(int defaultValue)
        {
            return new StatefulEventInt<int>(defaultValue, (a, b) => a == b);
        }
        public static StatefulEventInt<uint> Create(uint defaultValue)
        {
            return new StatefulEventInt<uint>(defaultValue, (a, b) => a == b);
        }
        public static StatefulEventInt<long> Create(long defaultValue)
        {
            return new StatefulEventInt<long>(defaultValue, (a, b) => a == b);
        }
        public static StatefulEventInt<ulong> Create(ulong defaultValue)
        {
            return new StatefulEventInt<ulong>(defaultValue, (a, b) => a == b);
        }
        public static StatefulEventInt<float> Create(float defaultValue)
        {
            return new StatefulEventInt<float>(defaultValue, (a, b) => a == b);
        }
        public static StatefulEventInt<double> Create(double defaultValue)
        {
            return new StatefulEventInt<double>(defaultValue, (a, b) => a == b);
        }
        public static StatefulEventInt<T> CreateEnum<T>(T defaultValue) where T : Enum
        {
            return new StatefulEventInt<T>(defaultValue, (a, b) => a.GetHashCode() == b.GetHashCode());
        }
        public static StatefulEventInt<T> CreateGenericStruct<T>(T defaultValue) where T : struct, IValue<T>
        {
            return new StatefulEventInt<T>(defaultValue, (a, b) => a.Equals(b));
        }
        public static StatefulEventInt<T> CreateGenericClass<T>(T defaultValue) where T : class, IValue<T>
        {
            return new StatefulEventInt<T>(defaultValue, (a, b) => a != null && b != null && a.Equals(b));
        }
    }

    public class StatefulEventInt<T> : IStatefulEvent<T>
    {
        public event Action<T> OnValueChanged = t => { };
        public T Value
        {
            get
            {
                lock (lockObject)
                {
                    return currentValue;
                }
            }
        }
        public event Action<T, T> OnValueChangedFromTo = (f, t) => { };

        private object lockObject = new();
        private readonly T defaultValue;
        private T currentValue;
        private Func<T, T, bool> equator;

        public StatefulEventInt(T defaultValue, Func<T, T, bool> equator)
        {
            this.equator = equator;
            this.defaultValue = defaultValue;
            this.currentValue = defaultValue;
        }

        public void Set(in T newValue)
        {
            T oldValue = default;
            bool valueChanged = false;

            lock (lockObject)
            {
                if (equator(currentValue, newValue) == false)
                {
                    oldValue = currentValue;
                    currentValue = newValue;
                    valueChanged = true;
                }
            }

            if (valueChanged == true)
            {
                OnValueChanged(newValue);
                OnValueChangedFromTo(oldValue, newValue);
            }
        }

        public StatefulEventInt<T, T2> Add<T2>(StatefulEventInt<T2> value2)
        {
            return new StatefulEventInt<T, T2>(this, value2);
        }

        public void Reset()
        {
            OnValueChanged = v => { };

            lock (lockObject)
            {
                currentValue = defaultValue;
            }
            OnValueChangedFromTo = (f, t) => { };
        }
    }

    public class StatefulEventInt<T1, T2> : IStatefulEvent<T1, T2>
    {
        public event Action<T1, T2> OnValueChanged = (t1, t2) => { };
        public T1 Value1 => value1.Value;
        public T2 Value2 => value2.Value;

        private object lockObject = new();
        private readonly StatefulEventInt<T1> value1;
        private readonly StatefulEventInt<T2> value2;
        private bool someValueChanged = false;

        public StatefulEventInt(StatefulEventInt<T1> value1, StatefulEventInt<T2> value2)
        {
            this.value1 = value1;
            this.value2 = value2;
            Reset();
        }

        public void SetValue1(in T1 newValue1)
        {
            T2 oldValue2;
            lock (lockObject)
            {
                value1.Set(newValue1);
                oldValue2 = value2.Value;
            }
            FireEventIfChanged(newValue1, oldValue2);
        }

        public void SetValue2(in T2 newValue2)
        {
            T1 oldValue1;
            lock (lockObject)
            {
                oldValue1 = value1.Value;
                value2.Set(newValue2);
            }
            FireEventIfChanged(oldValue1, newValue2);
        }

        public void SetValues(in T1 newValue1, in T2 newValue2)
        {
            lock (lockObject)
            {
                value1.Set(newValue1);
                value2.Set(newValue2);
            }
            FireEventIfChanged(newValue1, newValue2);
        }

        private void FireEventIfChanged(in T1 value1, in T2 value2)
        {
            bool someValueChanged;

            lock (lockObject)
            {
                someValueChanged = this.someValueChanged;
                this.someValueChanged = false;
            }

            if (someValueChanged == true)
            {
                OnValueChanged(value1, value2);
            }
        }

        public void Reset()
        {
            lock (lockObject)
            {
                OnValueChanged = (t1, t2) => { };

                value1.Reset();
                value1.OnValueChanged += v => { someValueChanged = true; };

                value2.Reset();
                value2.OnValueChanged += v => { someValueChanged = true; };
            }
        }
    }
}
