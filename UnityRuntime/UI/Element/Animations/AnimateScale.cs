//this empty line for UTF-8 BOM header

using UnityEngine;

namespace UnityTools.UnityRuntime.UI.Element.Animations
{
    public class AnimateScale : AnimationBase
    {
        [SerializeField] private RectTransform customAnimatedTransform;
        [SerializeField] private float visibleScale = 1f;
        [SerializeField] private float invisibleScale = 0f;

        private RectTransform rectTransform;

        protected override void Initialize()
        {
            this.rectTransform = customAnimatedTransform != null ? customAnimatedTransform : GetComponent<RectTransform>();
        }

        public override bool CanBeDisabledWhenInvisible => invisibleScale < 0.01f;

        protected override void ApplyVisibility(float visibility)
        {
            rectTransform.localScale = Vector3.one * Mathf.Lerp(invisibleScale, visibleScale, visibility);
        }
    }
}
