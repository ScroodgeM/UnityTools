//this empty line for UTF-8 BOM header
using UnityEngine;
using UnityTools.Runtime.StatefulEvent;

namespace UnityTools.UnityRuntime.StatefulEvent
{
    public static class StatefulEventForUnity
    {
        public static StatefulEventInt<Vector2Int> Create(Vector2Int defaultValue)
        {
            return new StatefulEventInt<Vector2Int>(defaultValue, (a, b) => a == b);
        }
        public static StatefulEventInt<Vector3Int> Create(Vector3Int defaultValue)
        {
            return new StatefulEventInt<Vector3Int>(defaultValue, (a, b) => a == b);
        }
        public static StatefulEventInt<Vector2> Create(Vector2 defaultValue)
        {
            return new StatefulEventInt<Vector2>(defaultValue, (a, b) => a == b);
        }
        public static StatefulEventInt<Vector3> Create(Vector3 defaultValue)
        {
            return new StatefulEventInt<Vector3>(defaultValue, (a, b) => a == b);
        }
    }
}
