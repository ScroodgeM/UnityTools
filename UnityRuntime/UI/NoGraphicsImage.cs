using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityTools.UnityRuntime.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class NoGraphicsImage : MaskableGraphic, IEventSystemHandler
    {
        public override void SetMaterialDirty() { return; }
        public override void SetVerticesDirty() { return; }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            toFill.Clear();
        }
    }
}
