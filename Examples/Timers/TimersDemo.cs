using UnityEngine;
using UnityTools.UnityRuntime.Timers;

namespace UnityTools.Examples.Timers
{
    public class TimersDemo : MonoBehaviour
    {
        private void OnGUI()
        {
            if (GUILayout.Button("Wait timer"))
            {
                StartSimpleWaitTimer();
            }

            if (GUILayout.Button("Conditional timer"))
            {
                StartConditionalTimer();
            }

            if (GUILayout.Button("Timer stop with unity objects death"))
            {
                StartTimerAndStopWithUnityObjectDeath();
            }

            if (GUILayout.Button("Timer stop manually"))
            {
            }

            if (GUILayout.Button("How progress callback works"))
            {
            }
        }

        private void StartSimpleWaitTimer()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            Debug.Log("Starting timer 1 for 1 second...");

            Timer.Instance.Wait(1.0)
                .Done(() => Debug.Log($"Timer 1 finished after {stopwatch.Elapsed.TotalSeconds} seconds."));

            Debug.Log("Starting timer 2 for 3 seconds...");

            Timer.Instance.Wait(3.0)
                .Done(() => Debug.Log($"Timer 2 finished after {stopwatch.Elapsed.TotalSeconds} seconds."));
        }

        private void StartConditionalTimer()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            int value = 10;

            Debug.Log("Starting timer 1 to wait for value == 20");

            Timer.Instance.WaitForTrue(() => value == 20)
                .Done(() => Debug.Log($"Timer 1 (value == 20) finished after {stopwatch.Elapsed.TotalSeconds} seconds."));

            Debug.Log("Starting timer 2 to wait for value == 30");

            Timer.Instance.WaitForTrue(() => value == 30)
                .Done(() => Debug.Log($"Timer 2 (value == 30) finished after {stopwatch.Elapsed.TotalSeconds} seconds."));

            Debug.Log("Actually we will set value to 30 after 1 second and then to 20 after another second");

            Timer.Instance.Wait(1.0)
                .Done(() => value = 30)
                .Then(() => Timer.Instance.Wait(1.0))
                .Done(() => value = 20);
        }

        private void StartTimerAndStopWithUnityObjectDeath()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            GameObject gameObjectWeWillLookAt = new GameObject("test GO");

            Debug.Log("Starting 3 timers at once, one will be succeed on GO death, another one fails, and the third closes silently");

            // silent
            Timer.Instance.Wait(1000.0)
                .StopOnUnityObjectDestroy(gameObjectWeWillLookAt, StopResult.Silently)
                .Done(() => Debug.LogError($"This message should never be printed"))
                .Fail(ex => Debug.LogError($"This message should never be printed"));

            // success
            Timer.Instance.Wait(1000.0)
                .StopOnUnityObjectDestroy(gameObjectWeWillLookAt, StopResult.WithResolving)
                .Done(() => Debug.Log($"This is expected result of success timer"))
                .Fail(ex => Debug.LogError($"This message should never be printed"));

            // fail
            Timer.Instance.Wait(1000.0)
                .StopOnUnityObjectDestroy(gameObjectWeWillLookAt, StopResult.WithRejection)
                .Done(() => Debug.LogError($"This message should never be printed"))
                .Fail(ex => Debug.Log($"This is expected result of failure timer, exception is '{ex.Message}'"));

            Debug.Log("... and actually destroy Game Object after 2 seconds");

            Destroy(gameObjectWeWillLookAt, 2f);
        }
    }
}
