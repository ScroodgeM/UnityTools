using UnityEngine;

namespace UnityTools.UnityRuntime.UI.Element.Animations
{
    public class AnimatePosition : AnimationBase
    {
        [SerializeField] private RectTransform customAnimatedTransform;
        [SerializeField] private Vector2 positionVisible;
        [SerializeField] private Vector2 positionHidden;
        [SerializeField] private bool canBeDisabledWhenInvisible = true;

        private RectTransform rectTransform;

        protected override void Initialize()
        {
            this.rectTransform = customAnimatedTransform != null ? customAnimatedTransform : GetComponent<RectTransform>();
        }

        public override bool CanBeDisabledWhenInvisible => canBeDisabledWhenInvisible;

        protected override void ApplyVisibility(float visibility)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(positionHidden, positionVisible, visibility);
        }
    }
}
