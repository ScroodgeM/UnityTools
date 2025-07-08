using UnityEngine;

namespace UnityTools.UnityRuntime.Helpers
{
    public class DestroyOnAwake : MonoBehaviour
    {
        private void Awake()
        {
            Destroy(gameObject);
        }
    }
}