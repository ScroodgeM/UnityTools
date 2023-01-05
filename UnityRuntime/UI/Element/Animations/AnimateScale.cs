
using UnityEngine;

namespace UnityTools.UnityRuntime.UI.Element.Animations
{
    public class AnimateScale : AnimationBase
    {
        [SerializeField] private float visibleScale = 1f;
        [SerializeField] private float invisibleScale = 0f;

        private Transform scalableTransform;

        protected override void Init()
        {
            scalableTransform = GetComponent<Transform>();
        }

        internal override bool CanBeDisabledWhenInvisible => invisibleScale < 0.01f;

        protected override void ApplyVisibility(float visibility)
        {
            scalableTransform.localScale = Vector3.one * Mathf.Lerp(invisibleScale, visibleScale, visibility);
        }
    }
}
