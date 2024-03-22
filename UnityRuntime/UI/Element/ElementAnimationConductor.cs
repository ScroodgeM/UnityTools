//this empty line for UTF-8 BOM header

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Runtime.Promises;
using UnityTools.UnityRuntime.Timers;

namespace UnityTools.UnityRuntime.UI.Element
{
    public class ElementAnimationConductor : ElementAnimator
    {
        [Serializable]
        private struct OtherElement
        {
            public ElementBase element;
            public float showDelay;
            public float hideDelay;
        }

        [SerializeField] private OtherElement[] otherElements;
        [SerializeField] private float selfShowDelay;
        [SerializeField] private float selfHideDelay;

        internal override IPromise SetVisible(bool visible)
        {
            List<IPromise> allPromises = UnityEngine.Pool.ListPool<IPromise>.Get();

            allPromises.Add(WaitAndGoSelf(visible));

            foreach (OtherElement otherElement in otherElements)
            {
                allPromises.Add(WaitAndGo(visible, otherElement));
            }

            IPromise result = Deferred.All(allPromises);

            UnityEngine.Pool.ListPool<IPromise>.Release(allPromises);

            return result;
        }

        private IPromise WaitAndGoSelf(bool visible)
        {
            float delay = visible == true ? selfShowDelay : selfHideDelay;
            return Timer.Instance.UnityObjectWaitUnscaled(this, delay).Then(() => base.SetVisible(visible));
        }

        private IPromise WaitAndGo(bool visible, OtherElement element)
        {
            float delay = visible == true ? element.showDelay : element.hideDelay;
            return Timer.Instance.UnityObjectWaitUnscaled(this, delay).Then(() => element.element.SetVisible(visible));
        }
    }
}
