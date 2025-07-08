using UnityEngine;
using UnityTools.Runtime.StatefulEvent;

namespace UnityTools.Examples.StatefulEvent
{
    public interface IDataProvider
    {
        IStatefulEvent<string> displayName { get; }
        IStatefulEvent<uint, uint> healthPoints { get; }
        IStatefulEvent<Color> color { get; }
        IStatefulEvent<Vector2Int> position { get; }
    }
}