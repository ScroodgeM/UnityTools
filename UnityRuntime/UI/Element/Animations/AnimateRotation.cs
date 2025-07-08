using UnityEngine;

namespace UnityTools.UnityRuntime.UI.Element.Animations
{
    public class AnimateRotation : AnimationBase
    {
        [SerializeField] private RectTransform customAnimatedTransform;
        [SerializeField] private Vector3 rotationHidden;
        [SerializeField] private Vector3 rotationVisible;
        [SerializeField] private bool canBeDisabledWhenInvisible = true;

        private RectTransform rectTransform;
        private Quaternion quaternionHidden;
        private Quaternion quaternionVisible;

        protected override void Initialize()
        {
            this.rectTransform = customAnimatedTransform != null ? customAnimatedTransform : GetComponent<RectTransform>();
            quaternionHidden = Quaternion.Euler(rotationHidden);
            quaternionVisible = Quaternion.Euler(rotationVisible);
        }

        public override bool CanBeDisabledWhenInvisible => canBeDisabledWhenInvisible;

        protected override void ApplyVisibility(float visibility)
        {
            rectTransform.localRotation = Quaternion.Lerp(quaternionHidden, quaternionVisible, visibility);
        }
    }
}