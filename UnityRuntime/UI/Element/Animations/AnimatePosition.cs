
using UnityEngine;

namespace UnityTools.UnityRuntime.UI.Element.Animations
{
    public class AnimatePosition : AnimationBase
    {
        [SerializeField] private Vector2 positionVisible;
        [SerializeField] private Vector2 positionHidden;
        [SerializeField] private bool canBeDisabledWhenInvisible = true;

        private RectTransform rectTransform;

        protected override void Init()
        {
            this.rectTransform = GetComponent<RectTransform>();
        }

        internal override bool CanBeDisabledWhenInvisible => canBeDisabledWhenInvisible;

        protected override void ApplyVisibility(float visibility)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(positionHidden, positionVisible, visibility);
        }
    }
}
