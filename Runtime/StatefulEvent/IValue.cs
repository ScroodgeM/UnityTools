//this empty line for UTF-8 BOM header

namespace UnityTools.Runtime.StatefulEvent
{
    public interface IValue<T>
    {
        bool Equals(T other);
    }
}
