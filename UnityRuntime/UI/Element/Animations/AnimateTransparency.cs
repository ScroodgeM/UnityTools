using UnityEngine;

namespace UnityTools.UnityRuntime.UI.Element.Animations
{
    public class AnimateTransparency : AnimationBase
    {
        [SerializeField] private float visibleAlpha = 1f;
        [SerializeField] private float invisibleAlpha = 0f;

        private CanvasGroup canvasGroup;

        protected override void Initialize()
        {
            canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        public override bool CanBeDisabledWhenInvisible => invisibleAlpha < 0.01f;

        protected override void ApplyVisibility(float visibility)
        {
            canvasGroup.alpha = Mathf.Lerp(invisibleAlpha, visibleAlpha, visibility);
        }
    }
}