//this empty line for UTF-8 BOM header
using System;

namespace UnityTools.Runtime.StatefulEvent
{
    public interface IStatefulEvent<T>
    {
        event Action<T> OnValueChanged;
        event Action<T, T> OnValueChangedFromTo;
        T Value { get; }
    }

    public interface IStatefulEvent<T1, T2>
    {
        event Action<T1, T2> OnValueChanged;
        T1 Value1 { get; }
        T2 Value2 { get; }
    }
}
