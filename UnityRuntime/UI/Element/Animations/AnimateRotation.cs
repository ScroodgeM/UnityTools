//this empty line for UTF-8 BOM header
using UnityEngine;

namespace UnityTools.UnityRuntime.UI.Element.Animations
{
    public class AnimateRotation : AnimationBase
    {
        [SerializeField] private Vector3 rotationHidden;
        [SerializeField] private Vector3 rotationVisible;
        [SerializeField] private bool canBeDisabledWhenInvisible = true;

        private RectTransform rectTransform;
        private Quaternion quaternionHidden;
        private Quaternion quaternionVisible;

        protected override void Init()
        {
            this.rectTransform = GetComponent<RectTransform>();
            quaternionHidden = Quaternion.Euler(rotationHidden);
            quaternionVisible = Quaternion.Euler(rotationVisible);
        }

        internal override bool CanBeDisabledWhenInvisible => canBeDisabledWhenInvisible;

        protected override void ApplyVisibility(float visibility)
        {
            rectTransform.localRotation = Quaternion.Lerp(quaternionHidden, quaternionVisible, visibility);
        }
    }
}
