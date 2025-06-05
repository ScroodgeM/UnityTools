using UnityEngine;

namespace UnityTools.UnityRuntime.UI.Element
{
    public class PingPongAnimatedElement : ElementBase
    {
        [SerializeField] private float onOffPeriod = 1f;

        private bool lastState = false;

        private void Update()
        {
            bool newState = (Time.realtimeSinceStartupAsDouble % onOffPeriod) > 0.5;

            if (newState != lastState)
            {
                SetVisible(newState);
                lastState = newState;
            }
        }
    }
}
