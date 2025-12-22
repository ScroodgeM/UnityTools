using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityTools.Runtime.Promises;
using UnityTools.UnityRuntime.Timers;
using UnityTools.UnityRuntime.UI.Element;

namespace UnityTools.Examples.Promises
{
    public class Fireworks : ElementBase
    {
        [SerializeField] private Button playButton;

        [SerializeField] private Image shotPrefab;
        [SerializeField] private Vector2 shotsStartPoint;
        [SerializeField] private Vector2 shotsTargetPoint;
        [SerializeField] private float shotsTargetSpread = 200f;
        [SerializeField] private int shotsCount = 15;
        [SerializeField] private float shotSpawnInterval = 0.2f;

        [SerializeField] private Image particlePrefab;
        [SerializeField] private int particlesCount = 15;
        [SerializeField] private float particlesFlyDistance = 100f;
        [SerializeField] private float particlesFlyDistanceSpread = 50f;

        [SerializeField] private float flySpeed = 150f;
        [SerializeField] private float flySpeedSpread = 50f;

        [SerializeField] private float disappearDuration = 1.0f;
        [SerializeField] private float disappearDurationSpread = 0.5f;

        private void Awake()
        {
            playButton.onClick.AddListener(OnPlayButtonClick);
        }

        private void OnPlayButtonClick()
        {
            Play().Done(() => { Debug.Log("fireworks finished playing"); });
        }

        private IPromise Play()
        {
            List<IPromise> shots = new List<IPromise>();
            for (int i = 0; i < shotsCount; i++)
            {
                shots.Add(PlayShot(shotSpawnInterval * i));
            }

            double startTime = Time.unscaledTimeAsDouble;

            Deferred.Race(shots)
                .Done(() => { Debug.Log($"Fastest shot finished playing in {Time.unscaledTimeAsDouble - startTime:0.000} seconds"); });

            return Deferred.All(shots)
                .Done(() => { Debug.Log($"All shots finished playing in {Time.unscaledTimeAsDouble - startTime:0.000} seconds"); });
        }

        private IPromise PlayShot(float startDelay)
        {
            Image shotInstance = Instantiate(shotPrefab, transform);
            shotInstance.color = GetRandomColor();
            shotInstance.rectTransform.anchoredPosition = shotsStartPoint;

            List<Image> allImages = new List<Image>() { shotInstance };

            return Deferred.Sequence(WaitForDelay, FlyToExplosionPoint, Explode, DisappearAllImages)
                .Always(() =>
                {
                    foreach (Image image in allImages)
                    {
                        Destroy(image.gameObject);
                    }
                });

            IPromise WaitForDelay()
            {
                return Timer.Instance.WaitUnscaled(startDelay);
            }

            IPromise FlyToExplosionPoint()
            {
                Vector2 explosionPoint = shotsTargetPoint + Random.insideUnitCircle * shotsTargetSpread;
                return Move(shotInstance, explosionPoint);
            }

            IPromise Explode()
            {
                return Deferred.All(GetFlyToEndPointPromises());

                List<IPromise> GetFlyToEndPointPromises()
                {
                    List<IPromise> result = new List<IPromise>();
                    for (int i = 0; i < particlesCount; i++)
                    {
                        Image particleInstance = Instantiate(particlePrefab, transform);
                        particleInstance.color = GetRandomColor();
                        allImages.Add(particleInstance);
                        particleInstance.rectTransform.anchoredPosition = shotInstance.rectTransform.anchoredPosition;
                        float distance = RandomFromSpread(particlesFlyDistance, particlesFlyDistanceSpread);
                        Vector2 endPoint = shotInstance.rectTransform.anchoredPosition + Random.insideUnitCircle.normalized * distance;
                        result.Add(Move(particleInstance, endPoint));
                    }

                    return result;
                }
            }

            IPromise DisappearAllImages()
            {
                return Deferred.All(allImages.ConvertAll(Disappear));
            }
        }

        private IPromise Move(Image image, Vector2 goalAnchoredPosition)
        {
            Vector2 startAnchoredPosition = image.rectTransform.anchoredPosition;
            float duration = (goalAnchoredPosition - startAnchoredPosition).magnitude / RandomFromSpread(flySpeed, flySpeedSpread);
            return Timer.Instance
                .Wait(duration, progress =>
                {
                    float slowingDownProgress = 1f - (1f - progress) * (1f - progress);
                    Vector2 newPosition = Vector2.Lerp(startAnchoredPosition, goalAnchoredPosition, slowingDownProgress);
                    image.rectTransform.anchoredPosition = newPosition;
                })
                .StopOnUnityObjectDestroy(image, StopResult.Silently)
                .Done(() => { image.rectTransform.anchoredPosition = goalAnchoredPosition; });
        }

        private IPromise Disappear(Image image)
        {
            Color startColor = image.color;

            Color endColor = startColor;
            endColor.a = 0f;

            float duration = RandomFromSpread(disappearDuration, disappearDurationSpread);

            return Timer.Instance
                .Wait(duration, progress => { image.color = Color.Lerp(startColor, endColor, progress); })
                .StopOnUnityObjectDestroy(image, StopResult.Silently)
                .Done(() => { image.enabled = false; });
        }

        private Color GetRandomColor()
        {
            return Random.ColorHSV(0, 1, 0, 1, 1, 1, 1, 1);
        }

        private static float RandomFromSpread(float center, float spread)
        {
            float min = center - spread;
            float max = center + spread;
            return Random.Range(min, max);
        }
    }
}
