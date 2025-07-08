namespace UnityTools.Runtime.StatefulEvent
{
    public interface IValue<T>
    {
        bool Equals(T other);
    }
}