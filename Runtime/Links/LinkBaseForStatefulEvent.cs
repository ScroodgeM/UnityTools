//this empty line for UTF-8 BOM header

using UnityTools.Runtime.Links;
using UnityTools.Runtime.StatefulEvent;

namespace UnityTools.Runtime.Links
{
    public abstract class LinkBaseForStatefulEvent<T> : LinkBase, IValue<T> where T : LinkBase
    {
        public bool Equals(T other) => this == other;
    }
}
